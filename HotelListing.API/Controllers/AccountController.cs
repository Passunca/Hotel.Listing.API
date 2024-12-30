using AutoMapper;
using HotelListing.API.Contracts;
using HotelListing.API.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;
    public IAuthManager _authManager { get; }

    public AccountController(IAuthManager authManager, ILogger<AccountController> logger)
    {
        this._authManager = authManager;
        this._logger = logger;
    }

    // POST: api/Account/register
    [HttpPost]
    [Route("register")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Register([FromBody] ApiUserDto apiUserDto)
    {
        _logger.LogInformation($"Registration attempt for {apiUserDto.Email}");

        var errors = await _authManager.Register(apiUserDto);

        if (errors.Any())
        {
            foreach (var error in errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            return BadRequest(ModelState);
        }

        _logger.LogInformation($"Registered new user {apiUserDto.Email}");
        return Ok();

    }

    // POST: api/Account/login
    [HttpPost]
    [Route("login")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Login([FromBody] LoginDto loginDto)
    {
        _logger.LogInformation($"Login attempt for {loginDto.Email}");

        var authResponse = await _authManager.Login(loginDto);

        if (authResponse == null)
        {
            return Unauthorized();
        }

        return Ok(authResponse);
    }

    // POST: api/Account/refreshtoken
    [HttpPost]
    [Route("refreshtoken")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> RefreshToken([FromBody] AuthResponseDto authResponseDto)
    {
        var authResponse = await _authManager.VerifyRefreshToken(authResponseDto);

        if (authResponse == null)
        {
            return Unauthorized();
        }

        return Ok(authResponse);
    }
}
