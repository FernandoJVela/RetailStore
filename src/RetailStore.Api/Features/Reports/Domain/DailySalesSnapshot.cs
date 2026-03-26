namespace RetailStore.Api.Features.Reports.Domain;
 
/// <summary>
/// Read model entity — not a DDD aggregate.
/// Represents pre-aggregated daily sales data for fast dashboard queries.
/// Populated by the snapshot generator (background job or on-demand).
/// </summary>
public sealed class DailySalesSnapshot
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public int TotalOrders { get; set; }
    public int ConfirmedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public int CompletedOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalItemsSold { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int UniqueCustomers { get; set; }
    public int NewCustomers { get; set; }
    public decimal TotalPaymentsCaptured { get; set; }
    public decimal TotalRefunds { get; set; }
    public string? TopCategory { get; set; }
    public DateTime CreatedAt { get; set; }
}