using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailStore.Api.Features.Reports.Application.Queries;
 
namespace RetailStore.Api.Features.Reports.Api;
 
[ApiController, Route("api/v1/reports"), Authorize]
public sealed class ReportsController(ISender sender) : ControllerBase
{
    // ─── Dashboard ──────────────────────────────────────────
    [HttpGet("dashboard")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
        => Ok(await sender.Send(new GetDashboardKpisQuery(), ct));
 
    // ─── Sales ──────────────────────────────────────────────
    [HttpGet("sales/daily")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDailySales(
        [FromQuery] int days = 30, CancellationToken ct = default)
        => Ok(await sender.Send(new GetDailySalesQuery(days), ct));
 
    [HttpGet("sales/by-category")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSalesByCategory(CancellationToken ct)
        => Ok(await sender.Send(new GetSalesByCategoryQuery(), ct));
 
    [HttpGet("sales/revenue-by-period")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRevenueByPeriod(
        [FromQuery] int months = 12, CancellationToken ct = default)
        => Ok(await sender.Send(new GetRevenueByPeriodQuery(months), ct));
 
    // ─── Products ───────────────────────────────────────────
    [HttpGet("products/top-selling")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopProducts(
        [FromQuery] int top = 10, CancellationToken ct = default)
        => Ok(await sender.Send(new GetTopProductsQuery(top), ct));
 
    // ─── Customers ──────────────────────────────────────────
    [HttpGet("customers/top-spending")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopCustomers(
        [FromQuery] int top = 20, CancellationToken ct = default)
        => Ok(await sender.Send(new GetTopCustomersQuery(top), ct));
 
    // ─── Payments ───────────────────────────────────────────
    [HttpGet("payments/methods")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentAnalytics(CancellationToken ct)
        => Ok(await sender.Send(new GetPaymentAnalyticsQuery(), ct));
 
    // ─── Inventory ──────────────────────────────────────────
    [HttpGet("inventory/health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInventoryHealth(CancellationToken ct)
        => Ok(await sender.Send(new GetInventoryHealthQuery(), ct));
 
    // ─── Shipping ───────────────────────────────────────────
    [HttpGet("shipping/performance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShippingPerformance(CancellationToken ct)
        => Ok(await sender.Send(new GetShippingPerformanceQuery(), ct));
}