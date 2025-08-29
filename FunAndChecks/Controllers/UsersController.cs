using System.Security.Claims;
using FunAndChecks.Data;
using FunAndChecks.DTO;
using FunAndChecks.Models;
using FunAndChecks.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FunAndChecks.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _context;

    public UsersController(UserManager<User> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserInfoDto>> GetMe()
    {
        var user = await _userManager.Users
            .Include(u => u.Group)
            .FirstOrDefaultAsync(u => u.Id.ToString() == User.FindFirstValue(ClaimTypes.NameIdentifier));

        if (user == null) return NotFound();

        return Ok(new UserInfoDto(user.Id, user.FirstName, user.LastName, user.Email,  user.Group?.Name));
    }
    
    [HttpPut("me/link-telegram")]
    public async Task<IActionResult> LinkTelegramAccount([FromBody] LinkTelegramDto dto)
    {
        if (User.IsInRole("Admin"))
        {
            return Forbid("Administrators cannot link");
        }
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
        {
            return Unauthorized();
        }

        var currentUser = await _userManager.FindByIdAsync(userIdString);
        if (currentUser == null)
        {
            return NotFound("Current user not found.");
        }
        
        var isTelegramIdTaken = await _userManager.Users
            .AnyAsync(u => u.TelegramUserId == dto.TelegramId && u.Id != currentUser.Id);

        if (isTelegramIdTaken)
        {
            return Conflict("This Telegram account is already linked to another user.");
        }

        currentUser.TelegramUserId = dto.TelegramId;
        var result = await _userManager.UpdateAsync(currentUser);

        if (result.Succeeded)
        {
            return NoContent();
        }
        return BadRequest(result.Errors);
    }

    [HttpPost("queue/{eventId}/join")]
    public async Task<IActionResult> JoinQueue(int eventId)
    {
        if (User.IsInRole("Admin"))
        {
            return Forbid("Administrators cannot join the queue.");
        }
        
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound("User not found.");
        }

       
        var userGroupId = user.GroupId;
        if (userGroupId == null)
        {
            return Forbid("You are not assigned to any group.");
        }
    

        var isGroupAllowed = await _context.QueueEvents
            .Where(qe => qe.Id == eventId)
            .AnyAsync(qe => qe.Subject.GroupSubjects.Any(gs => gs.GroupId == userGroupId.Value));

        if (!isGroupAllowed)
        {
            return Forbid("Your group does not have access to the subject of this event.");
        }

        var alreadyInQueue = await _context.QueueUsers
            .AnyAsync(qu => qu.QueueEventId == eventId && qu.UserId == userId);
        if (alreadyInQueue) return Conflict("User already in queue.");
        
        var queueUser = new QueueUser
        {
            QueueEventId = eventId,
            UserId = userId,
            JoinTime = DateTime.UtcNow,
            Status = QueueUserStatus.Waiting
        };
        
        _context.QueueUsers.Add(queueUser);
        await _context.SaveChangesAsync();
        
        return Ok();
    }
}