using System;
using System.Security;
using System.Threading.Tasks;
using Auth.Service;
using Contract.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace API.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }
    
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(string),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] Login.Request request)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        try
        {
            var token = await _authService.LoginUser(request);

            return Ok(token);
        }
        catch (ArgumentException e)
        {
            _logger.LogError(e, $"There was an error while executing {nameof(Login)}");
            return BadRequest();
        }
        catch (SecurityException e)
        {
            _logger.LogError(e, $"There was an error while executing {nameof(Login)}");
            return Unauthorized();
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, $"There was a fatal error while executing {nameof(Login)}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}