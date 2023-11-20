using System;
using Library.DataAccess.Interfaces;
using Library.Models.Business;
using Library.Models.Crime;
using Library.Models.Responses;
using Library.Repositories.Interfaces;
using Library.Repositories.Utilities.Interfaces;

namespace Library.Repositories
{
	public class CrimeRepository : ICrimeRepository
	{
		private readonly ICrimeDataAccess dataAccess;
		private readonly IGridRepository gridRepository;
        private readonly ICacheHelper cache;

        public CrimeRepository(ICrimeDataAccess dataAccess, IGridRepository gridRepository, ICacheHelper cacheHelper)
		{
			this.dataAccess = dataAccess;
			this.gridRepository = gridRepository;
			this.cache = cacheHelper;
		}

		public IEnumerable<CrimeResponse> GetCrimes(int timeSlotId)
		{
			var crimes = dataAccess.GetCrimes(timeSlotId);

			if (crimes == null)
				return null;

			foreach(var crime in crimes)
				crime.Coordinates = gridRepository.GetBoundingBoxCoordinates(crime.GridHash);

			return crimes;
		}

        public IEnumerable<CrimeTimeResponse> GetAllCrimes()
        {
            var allCrimes = new List<CrimeTimeResponse>();

            if (cache.TryGetValue<IEnumerable<CrimeTimeResponse>>("AllCrimes", out var cachedCrimes))
            {
                foreach (var crime in cachedCrimes!)
                {
                    allCrimes.Add(crime);
                }
            }
			else
			{
                var timeslots = dataAccess.GetTimeSlots();

                foreach (var timeslot in timeslots)
                {
                    var crimes = dataAccess.GetCrimes(timeslot.Id);

                    if (crimes == null)
                        continue;

                    foreach (var crime in crimes)
                        crime.Coordinates = gridRepository.GetBoundingBoxCoordinates(crime.GridHash);

                    allCrimes.Add(new CrimeTimeResponse()
                    {
                        Id = timeslot.Id,
                        Crimes = crimes
                    });
                }

                cache.Set("AllCrimes", allCrimes);
            }

            return allCrimes;
        }
    }
}

