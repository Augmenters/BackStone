using System;
using Library.Models;
using Library.Models.Business;
using Library.Models.Yelp;

namespace Library.Repositories.Interfaces
{
	public interface IBusinessRepository
	{
        Task<DataResult<IEnumerable<POI>>> GetPOIs(Coordinate coordinate);
        Task<DataResult<BusinessReviewsResponse>> GetReviews(string businessId);
    }
}

