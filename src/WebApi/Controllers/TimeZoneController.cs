using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Auth.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TimeZoneController: ControllerBase
    {
        private readonly ITimeZoneService _timeZoneService;

        public TimeZoneController(ITimeZoneService timeZoneService)
        {
            _timeZoneService = timeZoneService ?? throw new ArgumentNullException(nameof(timeZoneService));
        }
        
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Contract.Responses.TimeZone.Response))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetDateTimeInTimeZone(Contract.Requests.TimeZone.Request request)
        {
            try
            {
                var dateTime = _timeZoneService.GetDateTimeInTimeZone(request);
                return Ok(dateTime);
            }
            catch (KeyNotFoundException e)
            {
                return NotFound();
            }
            catch (ArgumentException e)
            {
                return BadRequest();
            }
            catch (Exception e)
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
