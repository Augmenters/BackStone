using System;
using System.Net;
using Library.DataAccess;
using Library.DataAccess.Interfaces;
using Library.Models;
using Library.Repositories.Interfaces;
using Library.Repositories.Utilities;
using Microsoft.Extensions.Logging;
using Serilog.Core;

namespace Library.Repositories
{
	public class AddressRepository : IAddressRepository
	{
		private readonly IAddressDataAccess dataAccess;
        private readonly IGridRepository gridRepository;

		public AddressRepository(IAddressDataAccess dataAccess,
                                 IGridRepository gridRepository)
		{
			this.dataAccess = dataAccess;
            this.gridRepository = gridRepository;
		}

        public Result HashUnhashedAddresses()
        {
            var addresses = dataAccess.GetUnhashedAddresses();
            if (addresses == null || !addresses.Any())
                return new Result { IsSuccessful = false, ErrorId = HttpStatusCode.NotFound };

            var hashedAddresses = new List<(int, string)>();

            foreach(var address in addresses)
            {
                var hash = gridRepository.GenerateHash(address.coordinate);

                hashedAddresses.Add((address.id, hash));
            }

            return dataAccess.SaveAddressHashes(hashedAddresses);
        }
    }
}

