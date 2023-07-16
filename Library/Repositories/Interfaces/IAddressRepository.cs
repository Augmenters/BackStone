using System;
using Library.Models;

namespace Library.Repositories.Interfaces
{
	public interface IAddressRepository
	{
        Result HashUnhashedAddresses();
    }
}

