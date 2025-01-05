using AutoMapper;
using HotelListing.API.Core.Contracts;
using HotelListing.API.Core.Models.Users;
using HotelListing.API.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HotelListing.API.Core.Repository;
public class AuthManager : IAuthManager
{
    private readonly IMapper _mapper;
    public IConfiguration _configuration { get; }
    private readonly ILogger<AuthManager> _logger;

    private readonly UserManager<ApiUser> _userManager;
    private ApiUser _user;

    private const string _loginProvider = "HotelListingApi";
    private const string _refreshToken = "RefreshToken";

    public AuthManager(IMapper mapper, IConfiguration configuration, UserManager<ApiUser> userManager, ILogger<AuthManager> logger)
    {
        _mapper = mapper;
        _configuration = configuration;
        _userManager = userManager;
        _logger = logger;
    }

    public UserManager<ApiUser> UserManager { get; }

    public async Task<IEnumerable<IdentityError>> Register(ApiUserDto userDto)
    {
        _user = _mapper.Map<ApiUser>(userDto);
        _user.UserName = userDto.Email;

        var result = await _userManager.CreateAsync(_user, userDto.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(_user, "User");
        }

        return result.Errors;
    }

    public async Task<AuthResponseDto> Login(LoginDto loginDto)
    {
        _logger.LogInformation($"Looking for user with email {loginDto.Email}");
        _user = await _userManager.FindByEmailAsync(loginDto.Email);
        bool IsValidUser = await _userManager.CheckPasswordAsync(_user, loginDto.Password);

        if (_user == null || !IsValidUser)
        {
            _logger.LogWarning($"User with email {loginDto.Email} not found");
            return null;
        }

        var token = await GenerateToken();
        _logger.LogInformation($"Generated new token for user {loginDto.Email} | Token: {token}");

        return new AuthResponseDto { Token = token, UserId = _user.Id, RefreshToken = await CreateRefreshToken() };
    }

    private async Task<string> GenerateToken()
    {
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["JtwSettings:Key"]));

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var roles = await _userManager.GetRolesAsync(_user);
        var roleClaims = roles.Select(r => new Claim(ClaimTypes.Role, r)).ToList();

        var userClaims = await _userManager.GetClaimsAsync(_user);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, _user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, _user.Email),
        }.Union(userClaims).Union(roleClaims);

        var token = new JwtSecurityToken(
            issuer: _configuration["JtwSettings:Issuer"],
            audience: _configuration["JtwSettings:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["JtwSettings:DurationInMinutes"])),
            signingCredentials: credentials
            );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> CreateRefreshToken()
    {
        await _userManager.RemoveAuthenticationTokenAsync(_user, _loginProvider, _refreshToken);
        var newRefreshToken = await _userManager.GenerateUserTokenAsync(_user, _loginProvider, _refreshToken);
        var result = await _userManager.SetAuthenticationTokenAsync(_user, _loginProvider, _refreshToken, newRefreshToken);

        return newRefreshToken;
    }

    public async Task<AuthResponseDto> VerifyRefreshToken(AuthResponseDto authResponseDto)
    {
        var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        var tokenContent = jwtSecurityTokenHandler.ReadJwtToken(authResponseDto.Token);
        var userName = tokenContent.Claims.FirstOrDefault(q => q.Type == JwtRegisteredClaimNames.Email)?.Value;

        _user = await _userManager.FindByNameAsync(userName);

        if (_user == null)
        {
            return null;
        }

        var isValidRefreshToken = await _userManager.VerifyUserTokenAsync(_user, _loginProvider, _refreshToken, authResponseDto.RefreshToken);


        if (isValidRefreshToken)
        {
            return new AuthResponseDto
            {
                Token = await GenerateToken(),
                UserId = _user.Id,
                RefreshToken = await CreateRefreshToken()
            };
        }

        await _userManager.UpdateSecurityStampAsync(_user);
        return null;
    }
}
