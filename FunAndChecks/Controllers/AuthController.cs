using System.Text;
using FunAndChecks.DTO;
using FunAndChecks.Models;
using FunAndChecks.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FunAndChecks.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public AuthController(UserManager<User> userManager, ITokenService tokenService, IConfiguration configuration)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterUserDto dto)
    {
        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            UserName = dto.TelegramUsername,
            Email = dto.Email,
            GroupId = dto.GroupId,
            GitHubUrl = dto.GitHubUrl,
            TelegramUserId = dto.TelegramUserId,
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);
        
        await _userManager.AddToRoleAsync(user, "User");
        
        return StatusCode(201, new { message = "User registered successfully." });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginUserDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.TelegramUsername);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            return Unauthorized("Invalid credentials.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.CreateToken(user, roles);

        return Ok(new AuthResponseDto(token));
    }
    
    [HttpPost("telegram-login")]
    public async Task<ActionResult<AuthResponseDto>> TelegramLogin(TelegramLoginDto dto)
    {
        var secret = _configuration["BotAuth:SharedSecret"];
    
        if (!IsHashValid(dto.TelegramId, secret, dto.AuthHash))
        {
            return Unauthorized("Invalid auth hash.");
        }
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.TelegramUserId == dto.TelegramId);

        if (user == null)
        {
            return NotFound("No user is linked to this Telegram account.");
        }
    
        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.CreateToken(user, roles);

        return Ok(new AuthResponseDto(token));
    }

    private bool IsHashValid(long telegramId, string secret, string providedHash)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var computedHashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(telegramId.ToString()));
        var computedHashString = Convert.ToBase64String(computedHashBytes);
        return computedHashString == providedHash;
    }
}