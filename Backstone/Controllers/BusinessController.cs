using System;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Library.Models;
using Library.Repositories.Interfaces;
using Library.DataAccess;
using Library.Models.Yelp;
using Library.Models.Business;
using Library.Repositories.Utilities;

namespace Backstone.Controllers
{
    [ApiController]
    [Route("Business")]
    public class LocationController : ControllerBase
    {
        private readonly IBusinessRepository locationRepository;
        private readonly IYelpDataAccess yelpDataAccess;

        public LocationController(IBusinessRepository locationRepository,
                                  IYelpDataAccess yelpDataAccess)
        {
            this.locationRepository = locationRepository;
            this.yelpDataAccess = yelpDataAccess;
        }

        /// <summary>
        /// get points of interest around a location using geohashing
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns>a list of POIs</returns>
        [HttpGet]
        [Route("Search")]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(IEnumerable<POI>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetLocations([FromQuery] double latitude, [FromQuery] double longitude)
        {
            try
            {
                if (latitude == 0 || longitude == 0)
                    return BadRequest();

                var result = await locationRepository.GetPOIs(new Coordinate() { Latitude = latitude, Longitude = longitude });

                if (!result.IsSuccessful)
                    return Problem(detail: result.ErrorMessage, statusCode: (int)result.ErrorId);

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                ex.Data["Request"] = Request;
                ex.Log("Get Locations Failed");
                return Problem(detail: ex.Message, statusCode: (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// get detailed business data
        /// </summary>
        /// <param name="businessId"></param>
        /// <returns>a list of POIs</returns>
        [HttpGet]
        [Route("{businessId}/Reviews")]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(BusinessReviewsResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetReviews(string businessId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(businessId))
                    return BadRequest();

                var result = await yelpDataAccess.GetReviews(businessId);

                if (!result.IsSuccessful)
                    return Problem(detail: result.ErrorMessage, statusCode: (int)result.ErrorId);

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                ex.Data["Request"] = Request;
                ex.Log("Get Reviews Failed");
                return Problem(detail: ex.Message, statusCode: (int) HttpStatusCode.InternalServerError);
            }
        }
    }
}

