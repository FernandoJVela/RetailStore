using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailStore.Api.Features.Users.Application.Commands;
using RetailStore.Api.Features.Users.Application.Queries;

namespace RetailStore.Api.Features.Users.Api;

[ApiController, Route("api/v1/users")]
public sealed class UsersController(ISender sender) : ControllerBase
{
    [HttpPost("register"), AllowAnonymous]
    public async Task<IActionResult> Register(RegisterUserCommand cmd, CancellationToken ct)
        => CreatedAtAction(nameof(GetById),
           new { id = await sender.Send(cmd, ct) }, null);

    [HttpPost("login"), AllowAnonymous]
    public async Task<IActionResult> Login(LoginCommand cmd, CancellationToken ct)
        => Ok(await sender.Send(cmd, ct));

    [HttpPost("refresh"), AllowAnonymous]
    public async Task<IActionResult> Refresh(RefreshTokenCommand cmd, CancellationToken ct)
        => Ok(await sender.Send(cmd, ct));

    [HttpGet, Authorize]
    public async Task<IActionResult> GetUsers(CancellationToken ct)
        => Ok(await sender.Send(new GetUsersQuery(), ct));

    [HttpGet("{id:guid}"), Authorize]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(await sender.Send(new GetUserByIdQuery(id), ct));

    [HttpGet("roles"), Authorize]
    public async Task<IActionResult> GetRoles(CancellationToken ct)
        => Ok(await sender.Send(new GetRolesQuery(), ct));

    [HttpPost("{userId:guid}/roles"), Authorize]
    public async Task<IActionResult> AssignRole(
        Guid userId, AssignRoleCommand cmd, CancellationToken ct)
    { await sender.Send(cmd with { UserId = userId }, ct); return NoContent(); }

    [HttpDelete("{userId:guid}/roles/{roleId:guid}"), Authorize]
    public async Task<IActionResult> RevokeRole(
        Guid userId, Guid roleId, CancellationToken ct)
    { await sender.Send(new RevokeRoleCommand(userId, roleId), ct); return NoContent(); }

    [HttpPut("{id:guid}/deactivate"), Authorize]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    { await sender.Send(new DeactivateUserCommand(id), ct); return NoContent(); }
}