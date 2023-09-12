using System;
using Library.DataAccess.Interfaces;
using Library.Models.Responses;
using Library.Repositories.Interfaces;

namespace Library.Repositories
{
	public class CrimeRepository : ICrimeRepository
	{
		private readonly ICrimeDataAccess dataAccess;
		private readonly IGridRepository gridRepository;

		public CrimeRepository(ICrimeDataAccess dataAccess, IGridRepository gridRepository)
		{
			this.dataAccess = dataAccess;
			this.gridRepository = gridRepository;
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
	}
}

