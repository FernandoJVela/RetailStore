using FluentAssertions;
using RetailStore.Api.Features.Products.Domain;
using RetailStore.SharedKernel.Domain;
using RetailStore.SharedKernel.Domain.ValueObjects;

namespace RetailStore.Tests.Unit.Domain;

public class ProductTests
{
    private static readonly Money ValidPrice = new(29.99m, "USD");

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidData_SetsPropertiesCorrectly()
    {
        var product = Product.Create("Widget Pro", "  wgt-001  ", ValidPrice, "Electronics", "A great widget");

        product.Name.Should().Be("Widget Pro");
        product.Sku.Should().Be("WGT-001");       // trimmed + uppercased
        product.Price.Should().Be(ValidPrice);
        product.Category.Should().Be("Electronics");
        product.Description.Should().Be("A great widget");
        product.IsActive.Should().BeTrue();
        product.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_WithoutDescription_DescriptionIsNull()
    {
        var product = Product.Create("Widget", "WGT-001", ValidPrice, "Electronics");

        product.Description.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankName_ThrowsDomainException(string name)
    {
        var act = () => Product.Create(name, "WGT-001", ValidPrice, "Electronics");

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankSku_ThrowsDomainException(string sku)
    {
        var act = () => Product.Create("Widget", sku, ValidPrice, "Electronics");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithZeroPrice_ThrowsDomainException()
    {
        var act = () => Product.Create("Widget", "WGT-001", new Money(0m, "USD"), "Electronics");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithNegativePrice_ThrowsDomainException()
    {
        var act = () => Product.Create("Widget", "WGT-001", new Money(-1m, "USD"), "Electronics");

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankCategory_ThrowsDomainException(string category)
    {
        var act = () => Product.Create("Widget", "WGT-001", ValidPrice, category);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_RaisesProductCreatedEvent()
    {
        var product = Product.Create("Widget", "WGT-001", ValidPrice, "Electronics");

        product.DomainEvents.Should().ContainSingle(e => e is ProductCreatedEvent);
    }

    // ── UpdateDetails ─────────────────────────────────────────────────────────

    [Fact]
    public void UpdateDetails_WithValidData_UpdatesNameCategoryDescription()
    {
        var product = Product.Create("Old Name", "WGT-001", ValidPrice, "Electronics");

        product.UpdateDetails("New Name", "Software", "Updated description");

        product.Name.Should().Be("New Name");
        product.Category.Should().Be("Software");
        product.Description.Should().Be("Updated description");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateDetails_WithBlankName_ThrowsDomainException(string name)
    {
        var product = Product.Create("Widget", "WGT-001", ValidPrice, "Electronics");

        var act = () => product.UpdateDetails(name, "Electronics");

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateDetails_WithBlankCategory_ThrowsDomainException(string category)
    {
        var product = Product.Create("Widget", "WGT-001", ValidPrice, "Electronics");

        var act = () => product.UpdateDetails("Widget", category);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateDetails_RaisesProductUpdatedEvent()
    {
        var product = Product.Create("Widget", "WGT-001", ValidPrice, "Electronics");
        product.ClearDomainEvents();

        product.UpdateDetails("Updated Widget", "Electronics");

        product.DomainEvents.Should().ContainSingle(e => e is ProductUpdatedEvent);
    }

    // ── UpdatePrice ───────────────────────────────────────────────────────────

    [Fact]
    public void UpdatePrice_WithValidPrice_UpdatesPriceAndIncrementsVersion()
    {
        var product = Product.Create("Widget", "WGT-001", ValidPrice, "Electronics");
        var newPrice = new Money(49.99m, "USD");
        var versionBefore = product.Version;

        product.UpdatePrice(newPrice);

        product.Price.Should().Be(newPrice);
        product.Version.Should().Be(versionBefore + 1);
    }

    [Fact]
    public void UpdatePrice_WithZeroPrice_ThrowsDomainException()
    {
        var product = Product.Create("Widget", "WGT-001", ValidPrice, "Electronics");

        var act = () => product.UpdatePrice(new Money(0m, "USD"));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdatePrice_RaisesProductPriceChangedEvent()
    {
        var product = Product.Create("Widget", "WGT-001", ValidPrice, "Electronics");
        product.ClearDomainEvents();

        product.UpdatePrice(new Money(49.99m, "USD"));

        product.DomainEvents.Should().ContainSingle(e => e is ProductPriceChangedEvent);
    }

    // ── Deactivate ────────────────────────────────────────────────────────────

    [Fact]
    public void Deactivate_ActiveProduct_IsActiveBecomeFalse()
    {
        var product = Product.Create("Widget", "WGT-001", ValidPrice, "Electronics");

        product.Deactivate();

        product.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_AlreadyInactiveProduct_ThrowsDomainException()
    {
        var product = Product.Create("Widget", "WGT-001", ValidPrice, "Electronics");
        product.Deactivate();

        var act = () => product.Deactivate();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Deactivate_RaisesProductDeactivatedEvent()
    {
        var product = Product.Create("Widget", "WGT-001", ValidPrice, "Electronics");
        product.ClearDomainEvents();

        product.Deactivate();

        product.DomainEvents.Should().ContainSingle(e => e is ProductDeactivatedEvent);
    }

    // ── Reactivate ────────────────────────────────────────────────────────────

    [Fact]
    public void Reactivate_InactiveProduct_IsActiveBecomeTrue()
    {
        var product = Product.Create("Widget", "WGT-001", ValidPrice, "Electronics");
        product.Deactivate();

        product.Reactivate();

        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Reactivate_AlreadyActiveProduct_ThrowsDomainException()
    {
        var product = Product.Create("Widget", "WGT-001", ValidPrice, "Electronics");

        var act = () => product.Reactivate();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Reactivate_RaisesProductReactivatedEvent()
    {
        var product = Product.Create("Widget", "WGT-001", ValidPrice, "Electronics");
        product.Deactivate();
        product.ClearDomainEvents();

        product.Reactivate();

        product.DomainEvents.Should().ContainSingle(e => e is ProductReactivatedEvent);
    }
}
