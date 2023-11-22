﻿using System;
using Library.Models;
using Library.Models.Business;
using Library.Models.Yelp;

namespace Library.DataAccess.Interfaces
{
	public interface IYelpDataAccess
	{
        Task<DataResult<IEnumerable<YelpBusiness>>> BusinessQuery(Coordinate coordinate, int offset = 0);
        Task<DataResult<BusinessReviewsResponse>> GetReviews(string businessId);
    }
}

