using System;
using Library.Models;

namespace Library.DataAccess.Interfaces
{
	public interface IAddressDataAccess
	{
        IEnumerable<(int id, Coordinate coordinate)> GetUnhashedAddresses();
        Result SaveAddressHashes(IEnumerable<(int address_id, string hash)> addresses);
        IEnumerable<OpenAddress> GetAddressesInHash(string hash);
    }
}

