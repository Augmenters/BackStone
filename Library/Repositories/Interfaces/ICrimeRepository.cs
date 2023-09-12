using System;
using Library.Models.Responses;

namespace Library.Repositories.Interfaces
{
	public interface ICrimeRepository
	{
        IEnumerable<CrimeResponse> GetCrimes(int timeSlotId);
    }
}

