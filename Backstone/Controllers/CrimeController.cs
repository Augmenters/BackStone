using System;
using Library.Models.Crime;
using Library.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Library.DataAccess.Interfaces;
using Library.Repositories.Interfaces;

namespace Backstone.Controllers
{
    [ApiController]
    [Route("Crime")]
    public class CrimeController : ControllerBase
    {
        private readonly ICrimeRepository crimeRepository;
        private readonly ICrimeDataAccess crimeDataAccess;

        public CrimeController(ICrimeRepository crimeRepository, ICrimeDataAccess crimeDataAccess)
        {
            this.crimeRepository = crimeRepository;
            this.crimeDataAccess = crimeDataAccess;
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
        public async Task<IActionResult> GetCrimes(int timeSlotId)
        {
            try
            {
                if (timeSlotId == 0)
                    return BadRequest();

                var result = crimeRepository.GetCrimes(timeSlotId);

                return result != null
                     ? Ok(result)
                     : NotFound();
            }
            catch (Exception ex)
            {
                ex.Data["Request"] = Request;
                return Problem(detail: ex.Message, statusCode: (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// get all crime statistics 
        /// </summary>
        /// <param name="timeSlotId">day of week and time</param>
        /// <returns>a list of POIs</returns>
        [HttpGet("All")]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(IEnumerable<CrimeTimeResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAllCrimes()
        {
            try
            {
                var result = crimeRepository.GetAllCrimes();

                return result != null
                     ? Ok(result)
                     : NotFound();
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
                var result = crimeDataAccess.GetTimeSlots();

                return result != null
                     ? Ok(result)
                     : NotFound();
            }
            catch (Exception ex)
            {
                ex.Data["Request"] = Request;
                return Problem(detail: ex.Message, statusCode: (int)HttpStatusCode.InternalServerError);
            }
        }
    }
}

