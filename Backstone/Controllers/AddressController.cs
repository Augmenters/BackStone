using System;
using Library.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Library.Repositories.Utilities;

namespace Backstone.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AddressController : ControllerBase
    {
        private readonly IAddressRepository addressRepository;

        public AddressController(IAddressRepository addressRepository)
        {
            this.addressRepository = addressRepository;
        }

        /// <summary>
        /// Hash all unhashed addresses in the db
        /// </summary>
        /// <returns>Ok</returns>
        [HttpGet]
        [Route("HashUnhashed")]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public IActionResult HashUnhashed()
        {
            try
            {
                var result = addressRepository.HashUnhashedAddresses();

                if (!result.IsSuccessful)
                    return Problem(detail: result.ErrorMessage, statusCode: (int)result.ErrorId);

                return Ok();
            }
            catch (Exception ex)
            {
                ex.Log("Failed to hash unhashed addresses");
                return Problem(detail: ex.Message, statusCode: (int)HttpStatusCode.InternalServerError);
            }
        }
    }
}

