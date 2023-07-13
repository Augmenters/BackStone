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

            return SaveAddressHashes(hashedAddresses);
        }

        private Result SaveAddressHashes(IEnumerable<(int id, string hash)> addresses)
        {
            var result = new Result { IsSuccessful = true };
            var count = 0;

            foreach (var entry in addresses)
            {
                var updateResult = dataAccess.SaveAddressHash(entry);

                if (!updateResult.IsSuccessful)
                    return updateResult;

                count++;

                if (count % 1000 == 0)
                    LoggingProvider.LogInfo($"Hashed {count} addresses out of {addresses.Count()}");
            }

            return result;
        }
    }
}

