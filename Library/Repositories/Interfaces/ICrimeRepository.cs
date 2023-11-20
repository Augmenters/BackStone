using System;
using Library.Models.Crime;
using Library.Models.Responses;

namespace Library.Repositories.Interfaces
{
	public interface ICrimeRepository
	{
        IEnumerable<CrimeResponse> GetCrimes(int timeSlotId);
        IEnumerable<CrimeTimeResponse> GetAllCrimes();
    }
}

