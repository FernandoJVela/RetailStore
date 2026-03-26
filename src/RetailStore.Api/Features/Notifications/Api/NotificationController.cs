using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailStore.Api.Features.Notifications.Application.Commands;
using RetailStore.Api.Features.Notifications.Application.Queries;
 
namespace RetailStore.Api.Features.Notifications.Api;
 
[ApiController, Route("api/v1/notifications"), Authorize]
public sealed class NotificationsController(ISender sender) : ControllerBase
{
    // ─── Send Notification ──────────────────────────────────
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Send(
        SendNotificationCommand cmd, CancellationToken ct)
    {
        var id = await sender.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }
 
    [HttpPost("direct")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> SendDirect(
        SendDirectNotificationCommand cmd, CancellationToken ct)
    {
        var id = await sender.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }
 
    // ─── Mark Read ──────────────────────────────────────────
    [HttpPut("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    { await sender.Send(new MarkNotificationReadCommand(id), ct); return NoContent(); }
 
    // ─── Queries ────────────────────────────────────────────
    [HttpGet("recipient/{recipientId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetForRecipient(
        Guid recipientId,
        [FromQuery] string? status,
        [FromQuery] string? category,
        [FromQuery] int limit = 50,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetNotificationsQuery(recipientId, status, category, limit), ct));
 
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(await sender.Send(new GetNotificationByIdQuery(id), ct));
 
    [HttpGet("recipient/{recipientId:guid}/unread-count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount(Guid recipientId, CancellationToken ct)
        => Ok(new { count = await sender.Send(new GetUnreadCountQuery(recipientId), ct) });
 
    // ─── Preferences ────────────────────────────────────────
    [HttpGet("preferences/{recipientId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPreferences(Guid recipientId, CancellationToken ct)
        => Ok(await sender.Send(new GetPreferencesQuery(recipientId), ct));
 
    [HttpPut("preferences")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdatePreference(
        UpdatePreferenceCommand cmd, CancellationToken ct)
    { await sender.Send(cmd, ct); return NoContent(); }
}