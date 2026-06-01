using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Customers.Domain;
using RetailStore.Api.Features.Inventory.Domain;
using RetailStore.Api.Features.Orders.Domain;
using RetailStore.Api.Features.Products.Domain;
using RetailStore.Api.Features.Providers.Domain;
using RetailStore.Api.Features.Users.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Domain.ValueObjects;

namespace RetailStore.Api.Configuration;

/// <summary>
/// Seeds realistic demo data into the database when running in Development
/// (local or Docker). The seeder is idempotent: it checks whether products
/// already exist and exits immediately if they do, so restarting the container
/// never duplicates data.
///
/// Seeded credentials:
///   admin@retailstore.com  /  Admin@RetailStore1!   (Admin role)
///   john.doe@retailstore.com  /  Staff@RetailStore1!  (Staff role)
///   jane.smith@retailstore.com  /  Staff@RetailStore1!  (Staff role)
/// </summary>
public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(IServiceProvider services, ILogger logger)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<RetailStoreDbContext>();

        if (await db.Set<Product>().AnyAsync())
        {
            logger.LogDebug("Development data already present — skipping seed.");
            return;
        }

        logger.LogInformation("Seeding development data...");

        // ── Phase 1: entities with no FK dependencies between each other ──────
        // Users, products, customers, providers and inventory are independent.
        // We add them all to the change tracker, assign roles while still
        // in-memory (EF tracks the mutation), then flush everything in one call.
        var users     = SeedUsers(db, logger);
        var products  = SeedProducts(db, logger);
        var customers = SeedCustomers(db, logger);
        SeedProviders(db, logger);
        SeedInventory(db, products, logger);

        // Roles (Admin / Staff) are already in the DB — DatabaseSeeder.SeedAsync
        // runs before this seeder in Program.cs.
        await AssignRolesAsync(db, users, logger);

        await db.SaveChangesAsync();

        // ── Phase 2: orders ───────────────────────────────────────────────────
        // Orders store product and customer IDs as FK columns. SQL Server
        // enforces these constraints, so referenced rows must be committed
        // before we can insert the orders.
        SeedOrders(db, products, customers, logger);
        await db.SaveChangesAsync();

        logger.LogInformation("Development data seeded successfully.");
    }

    // ─── Users ──────────────────────────────────────────────────────────────

    private static List<User> SeedUsers(RetailStoreDbContext db, ILogger logger)
    {
        var users = new List<User>
        {
            // index 0 — Admin
            User.Register("admin",       "admin@retailstore.com",       "Admin@RetailStore1!"),
            // index 1 — Staff
            User.Register("john.doe",    "john.doe@retailstore.com",    "Staff@RetailStore1!"),
            // index 2 — Staff
            User.Register("jane.smith",  "jane.smith@retailstore.com",  "Staff@RetailStore1!"),
        };

        db.Set<User>().AddRange(users);
        logger.LogInformation("Seeded {Count} users.", users.Count);
        return users;
    }

    // Loads the Admin and Staff roles that DatabaseSeeder already created and
    // assigns them to the correct users while they are still in the change
    // tracker. EF Core detects the mutation via the ValueComparer on RoleIds
    // and persists the assignments in the next SaveChangesAsync call.
    private static async Task AssignRolesAsync(RetailStoreDbContext db, List<User> users, ILogger logger)
    {
        var adminRole = await db.Set<Role>().FirstOrDefaultAsync(r => r.Name == "Admin");
        var staffRole = await db.Set<Role>().FirstOrDefaultAsync(r => r.Name == "Staff");

        var admin = users[0]; // admin@retailstore.com
        var john  = users[1]; // john.doe@retailstore.com
        var jane  = users[2]; // jane.smith@retailstore.com

        if (adminRole is not null)
            admin.AssignRole(adminRole.Id, adminRole.Name);

        if (staffRole is not null)
        {
            john.AssignRole(staffRole.Id, staffRole.Name);
            jane.AssignRole(staffRole.Id, staffRole.Name);
        }

        logger.LogInformation("Roles assigned: admin → Admin, john.doe & jane.smith → Staff.");
    }

    // ─── Providers ──────────────────────────────────────────────────────────

    private static void SeedProviders(RetailStoreDbContext db, ILogger logger)
    {
        var providers = new[]
        {
            Provider.Register("TechSupply Co",      "John Smith",   "john@techsupply.com",     "+1 555-0101"),
            Provider.Register("Global Goods Ltd",   "Maria Garcia", "maria@globalgoods.com",   "+1 555-0102"),
            Provider.Register("Fresh Products Inc", "David Lee",    "david@freshproducts.com", "+1 555-0103"),
        };

        db.Set<Provider>().AddRange(providers);
        logger.LogInformation("Seeded {Count} providers.", providers.Length);
    }

    // ─── Products ───────────────────────────────────────────────────────────

    private static List<Product> SeedProducts(RetailStoreDbContext db, ILogger logger)
    {
        var products = new List<Product>
        {
            Product.Create("Laptop Pro 15\"",     "LAPTOP-PRO-15",  new Money(1299.99m, "USD"), "Electronics", "High-performance laptop for professionals"),
            Product.Create("Wireless Mouse",      "MOUSE-WL-001",   new Money(  29.99m, "USD"), "Electronics", "Ergonomic wireless mouse"),
            Product.Create("Standing Desk",       "DESK-STAND-001", new Money( 449.99m, "USD"), "Furniture",   "Height-adjustable standing desk"),
            Product.Create("Office Chair",        "CHAIR-OFF-001",  new Money( 299.99m, "USD"), "Furniture",   "Ergonomic lumbar-support office chair"),
            Product.Create("USB-C Hub 6-Port",    "HUB-USBC-6P",    new Money(  49.99m, "USD"), "Electronics", "6-port USB-C hub with HDMI and Power Delivery"),
            Product.Create("Mechanical Keyboard", "KB-MECH-001",    new Money( 129.99m, "USD"), "Electronics", "Compact tenkeyless mechanical keyboard"),
        };

        db.Set<Product>().AddRange(products);
        logger.LogInformation("Seeded {Count} products.", products.Count);
        return products;
    }

    // ─── Inventory ──────────────────────────────────────────────────────────

    private static void SeedInventory(RetailStoreDbContext db, List<Product> products, ILogger logger)
    {
        // (initialQuantity, reorderThreshold) indexed to match the products list above
        (int qty, int threshold)[] stock =
        [
            (15,  5),   // Laptop
            (50, 20),   // Mouse
            ( 8,  3),   // Desk
            (12,  5),   // Chair
            (35, 15),   // USB-C Hub
            (20, 10),   // Keyboard
        ];

        var items = products
            .Select((p, i) => InventoryItem.Create(p.Id, stock[i].qty, stock[i].threshold))
            .ToList();

        db.Set<InventoryItem>().AddRange(items);
        logger.LogInformation("Seeded {Count} inventory items.", items.Count);
    }

    // ─── Customers ──────────────────────────────────────────────────────────

    private static List<Customer> SeedCustomers(RetailStoreDbContext db, ILogger logger)
    {
        var customers = new List<Customer>
        {
            Customer.Register("Alice", "Johnson",  "alice.johnson@email.com",  "+1 555-1001"),
            Customer.Register("Bob",   "Martinez", "bob.martinez@email.com",   "+1 555-1002"),
            Customer.Register("Carol", "Williams", "carol.williams@email.com", "+1 555-1003"),
            Customer.Register("David", "Brown",    "david.brown@email.com",    "+1 555-1004"),
        };

        db.Set<Customer>().AddRange(customers);
        logger.LogInformation("Seeded {Count} customers.", customers.Count);
        return customers;
    }

    // ─── Orders ─────────────────────────────────────────────────────────────

    private static void SeedOrders(
        RetailStoreDbContext db, List<Product> products, List<Customer> customers, ILogger logger)
    {
        var laptop   = products[0];
        var mouse    = products[1];
        var chair    = products[3];
        var hub      = products[4];
        var keyboard = products[5];

        var alice = customers[0];
        var bob   = customers[1];
        var carol = customers[2];
        var david = customers[3];

        // Order 1 — Alice: Mouse × 2 + USB-C Hub × 1
        var order1 = Order.Create(alice.Id);
        order1.AddItem(mouse.Id,    2, mouse.Price);
        order1.AddItem(hub.Id,      1, hub.Price);

        // Order 2 — Bob: Laptop × 1
        var order2 = Order.Create(bob.Id);
        order2.AddItem(laptop.Id,   1, laptop.Price);

        // Order 3 — Carol: Keyboard × 1 + Chair × 1
        var order3 = Order.Create(carol.Id);
        order3.AddItem(keyboard.Id, 1, keyboard.Price);
        order3.AddItem(chair.Id,    1, chair.Price);

        // Order 4 — David: Mouse × 3
        var order4 = Order.Create(david.Id);
        order4.AddItem(mouse.Id,    3, mouse.Price);

        db.Set<Order>().AddRange(order1, order2, order3, order4);
        logger.LogInformation("Seeded 4 orders.");
    }
}
