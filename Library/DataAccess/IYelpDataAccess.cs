using System;
using Library.Models;
using Library.Models.Business;
using Library.Models.Yelp;

namespace Library.DataAccess
{
	public interface IYelpDataAccess
	{
        Task<DataResult<IEnumerable<YelpBusiness>>> BusinessQuery(Coordinate coordinate);
        Task<DataResult<BusinessReviewsResponse>> GetReviews(string businessId);
    }
}

