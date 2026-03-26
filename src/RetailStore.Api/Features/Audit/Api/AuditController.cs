using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailStore.Api.Features.Audit.Application.Queries;
 
namespace RetailStore.Api.Features.Audit.Api;
 
[ApiController, Route("api/v1/audit"), Authorize]
public sealed class AuditController(ISender sender) : ControllerBase
{
    // ─── Search ─────────────────────────────────────────────
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] Guid? userId,
        [FromQuery] string? module,
        [FromQuery] string? entityType,
        [FromQuery] string? entityId,
        [FromQuery] string? outcome,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int limit = 50,
        CancellationToken ct = default)
        => Ok(await sender.Send(new SearchAuditLogQuery(
            userId, module, entityType, entityId, outcome, from, to, limit), ct));
 
    // ─── Detail ─────────────────────────────────────────────
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(await sender.Send(new GetAuditEntryByIdQuery(id), ct));
 
    // ─── Entity History ─────────────────────────────────────
    [HttpGet("entity/{entityType}/{entityId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEntityHistory(
        string entityType, string entityId, CancellationToken ct)
        => Ok(await sender.Send(new GetEntityHistoryQuery(entityType, entityId), ct));
 
    // ─── Recent Failures ────────────────────────────────────
    [HttpGet("failures")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFailures(
        [FromQuery] int limit = 50, CancellationToken ct = default)
        => Ok(await sender.Send(new GetRecentFailuresQuery(limit), ct));
 
    // ─── Analytics ──────────────────────────────────────────
    [HttpGet("activity/by-module")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetModuleActivity(CancellationToken ct)
        => Ok(await sender.Send(new GetModuleActivityQuery(), ct));
 
    [HttpGet("activity/by-user")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserActivity(CancellationToken ct)
        => Ok(await sender.Send(new GetUserActivityQuery(), ct));
}