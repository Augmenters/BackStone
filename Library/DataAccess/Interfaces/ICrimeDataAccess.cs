using System;
using Library.Models.Crime;
using Library.Models.Responses;

namespace Library.DataAccess.Interfaces
{
	public interface ICrimeDataAccess
	{
        IEnumerable<CrimeResponse> GetCrimes(int timeSlotId);
        IEnumerable<TimeSlot> GetTimeSlots();
    }
}

