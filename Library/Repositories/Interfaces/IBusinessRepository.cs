using System;
using Library.Models;
using Library.Models.Business;

namespace Library.Repositories.Interfaces
{
	public interface IBusinessRepository
	{
        Task<DataResult<IEnumerable<POI>>> GetPOIs(Coordinate coordinate);
    }
}

