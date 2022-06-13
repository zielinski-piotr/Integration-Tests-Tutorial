using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Auth.Common;
using Contract.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Service;

namespace API.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class HouseController : ControllerBase
{
    private readonly IHouseService _houseService;
    private readonly ILogger<HouseController> _logger;

    public HouseController(IHouseService houseService, ILogger<HouseController> logger)
    {
        _houseService = houseService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(AuthorizationPolicies.CanGetHouses)]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(IEnumerable<Contract.Responses.House.ListItem>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetHouses()
    {
        try
        {
            var houses = await _houseService.GetHouses();
            return Ok(houses);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, $"There was a fatal error while executing {nameof(GetHouses)}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("{id:guid}")]
    [Authorize(AuthorizationPolicies.CanGetHouse)]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(Contract.Responses.House.Response))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]    
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetHouseById(Guid id)
    {
        try
        {
            var house = await _houseService.GetHouseById(id);
            return Ok(house);
        }
        catch (KeyNotFoundException e)
        {
            _logger.LogError(e, $"There was an error while executing {nameof(GetHouseById)}");
            return NotFound();
        }
        catch (ArgumentException e)
        {
            _logger.LogError(e, $"There was an error while executing {nameof(GetHouseById)}");
            return BadRequest();
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"There was an error while executing {nameof(GetHouseById)}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPatch("{id:guid}")]
    [Authorize(AuthorizationPolicies.CanUpdateHouse)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PatchHouse([FromBody] JsonPatchDocument<House.Patch> patchDocument, Guid id)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        try
        {
            await _houseService.UpdateHouse(patchDocument, id);
        }
        catch (ArgumentException e)
        {
            _logger.LogError(e, $"There was an error while executing {nameof(PatchHouse)}");
            return BadRequest();
        }
        catch (JsonPatchException e)
        {
            _logger.LogError(e, $"There was an error while executing {nameof(PatchHouse)}");
            return UnprocessableEntity();
        }
        catch (KeyNotFoundException e)
        {
            _logger.LogError(e, $"There was an error while executing {nameof(PatchHouse)}");
            return NotFound();
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, $"There was a fatal error while executing {nameof(PatchHouse)}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        return Accepted();
    }

    [HttpPut("{id:guid}")]
    [Authorize(AuthorizationPolicies.CanUpdateHouse)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateHouse(House.Update updateDefinition, Guid id)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        try
        {
            await _houseService.UpdateHouse(updateDefinition, id);
        }
        catch (ArgumentException e)
        {
            _logger.LogError(e, $"There was an error while executing {nameof(UpdateHouse)}");
            return BadRequest();
        }
        catch (KeyNotFoundException e)
        {
            _logger.LogError(e, $"There was an error while executing {nameof(UpdateHouse)}");
            return NotFound();
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, $"There was a fatal error while executing {nameof(UpdateHouse)}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        return Accepted();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(AuthorizationPolicies.CanRemoveHouse)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveHouse(Guid id)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        try
        {
            await _houseService.RemoveHouseById(id);
        }
        catch (ArgumentException e)
        {
            _logger.LogError(e, $"There was an error while executing {nameof(RemoveHouse)}");
            return BadRequest();
        }
        catch (KeyNotFoundException e)
        {
            _logger.LogError(e, $"There was an error while executing {nameof(RemoveHouse)}");
            return NotFound();
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, $"There was a fatal error while executing {nameof(RemoveHouse)}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        return Accepted();
    }

    [HttpPost]
    [Authorize(AuthorizationPolicies.CanCreateHouse)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateHouse([FromBody] House.Request request)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        try
        {
            var house = await _houseService.CreateHouse(request);

            return CreatedAtAction(nameof(GetHouseById), new {id = house.Id}, house);
        }
        catch (ArgumentException e)
        {
            _logger.LogError(e, $"There was an error while executing {nameof(RemoveHouse)}");
            return BadRequest();
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, $"There was a fatal error while executing {nameof(RemoveHouse)}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}