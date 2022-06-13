using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Claims;
using Auth.Common;
using Auth.Domain;
using Contract.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Service;

public class AuthService : IAuthService
{
    private readonly JwtTokenConfig _jwtTokenConfig;
    private readonly SignInManager<SampleIdentityUser> _signInManager;
    private readonly UserManager<SampleIdentityUser> _userManager;

    public AuthService(UserManager<SampleIdentityUser> userManager, SignInManager<SampleIdentityUser> signInManager,
        JwtTokenConfig jwtTokenConfig)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        _jwtTokenConfig = jwtTokenConfig;
    }

    public async Task<Login.Response> LoginUser(Contract.Requests.Login.Request loginRequest)
    {
        var password = loginRequest.Password;
        var email = loginRequest.Email;
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null) throw new SecurityException("Unable to login user");

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, password, false);

        if (signInResult.Succeeded) return await GenerateTokenAsync(user);

        throw new SecurityException("Unable to login user");
    }

    private async Task<Login.Response> GenerateTokenAsync(SampleIdentityUser user)
    {
        var claimsPrincipal = await _signInManager.ClaimsFactory.CreateAsync(user);

        var now = DateTime.UtcNow;

        var claims = new List<Claim>(claimsPrincipal.Claims.Where(x => x.Type == PermissionClaim.PermissionClaimType))
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var jwtToken = new JwtSecurityToken(
            _jwtTokenConfig.Issuer,
            _jwtTokenConfig.Audience,
            claims,
            expires: now.AddMinutes(_jwtTokenConfig.AccessTokenExpiration),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Convert.FromBase64String(_jwtTokenConfig.Secret)),
                SecurityAlgorithms.HmacSha256Signature)
        );

        return new Login.Response()
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken)
        };
    }
}