using System;
using Library.Models.Crime;
using Library.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Backstone.Controllers
{
    [ApiController]
    [Route("Crime")]
    public class CrimeController : ControllerBase
    {

        public CrimeController()
        {
        }

        /// <summary>
        /// get crime statistics for a day/time
        /// </summary>
        /// <param name="timeSlotId">day of week and time</param>
        /// <returns>a list of POIs</returns>
        [HttpGet("{timeSlotId}")]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(IEnumerable<CrimeResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CrimeSearch(int timeSlotId)
        {
            try
            {
                return Ok();
            }
            catch (Exception ex)
            {
                ex.Data["Request"] = Request;
                return Problem(detail: ex.Message, statusCode: (int)HttpStatusCode.InternalServerError);
            }
        }

        // <summary>
        /// get available time slots
        /// </summary>
        /// <param name="request">day of week and time</param>
        /// <returns>a list of POIs</returns>
        [HttpGet("TimeSlots")]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(IEnumerable<TimeSlot>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetTimeSlots()
        {
            try
            {
                return Ok();
            }
            catch (Exception ex)
            {
                ex.Data["Request"] = Request;
                return Problem(detail: ex.Message, statusCode: (int)HttpStatusCode.InternalServerError);
            }
        }
    }
}

