using System;
using Library.Models;

namespace Library.DataAccess.Interfaces
{
	public interface IAddressDataAccess
	{
        IEnumerable<(int id, Coordinate coordinate)> GetUnhashedAddresses();
        Result SaveAddressHash((int id, string hash) address);
    }
}

