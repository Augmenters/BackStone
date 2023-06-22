using System;
using System.Net;
using Library.DataAccess;
using Library.Models;
using Library.Models.Business;
using Library.Models.Yelp;
using Library.Repositories.Interfaces;
using Library.Repositories.Utilities.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Library.Repositories
{
	public class BusinessRepository : IBusinessRepository
	{
        private readonly IYelpDataAccess yelpDataAccess;
        private readonly IGridRepository gridRepository;
        private readonly ICacheHelper cache;

		public BusinessRepository(IYelpDataAccess yelpDataAccess,
                                  IGridRepository gridRepository,
                                  ICacheHelper cache)
		{
            this.yelpDataAccess = yelpDataAccess;
            this.gridRepository = gridRepository;
            this.cache = cache;
		}

        public async Task<DataResult<IEnumerable<POI>>> GetPOIs(Coordinate coordinate)
        {
            var POIList = new List<POI>();
            var gridBoxes = gridRepository.GenerateGrid(coordinate);

            await Parallel.ForEachAsync(gridBoxes, async (box, token) =>
            {
                if (cache.TryGetValue<IEnumerable<POI>>(box.GeoHash, out var cachedPOIs))
                {
                    POIList.AddRange(cachedPOIs!);
                }
                else
                {
                    var result = await yelpDataAccess.BusinessQuery(box.Center);

                    if (result.ErrorId == HttpStatusCode.NotFound)
                        cache.Set(box.GeoHash, new List<POI>());

                    if (!result.IsSuccessful || result.Data == null)
                        return;

                    var converted = MapYelpToPOI(result.Data);
                    var valid = RemovePOIsOutsideOfBox(box.GeoHash, converted).ToList();
                    cache.Set(box.GeoHash, valid);

                    POIList.AddRange(valid);
                }
            });

            return POIList.Any()
                 ? new DataResult<IEnumerable<POI>> { IsSuccessful = true, Data = POIList }
                 : new DataResult<IEnumerable<POI>> { IsSuccessful = false, ErrorId = HttpStatusCode.NotFound, ErrorMessage = "No POIs returned from search" };
        }

        public async Task<DataResult<BusinessReviewsResponse>> GetReviews(string businessId)
        {
            if (cache.TryGetValue<BusinessReviewsResponse>(businessId, out var reviews))
            {
                return new DataResult<BusinessReviewsResponse> { IsSuccessful = true, Data = reviews };
            }
            else
            {
                var result = await yelpDataAccess.GetReviews(businessId);

                if (result.IsSuccessful && result.Data != null)
                    cache.Set(businessId, result.Data);

                return result;
            }
        }

        private IEnumerable<POI> RemovePOIsOutsideOfBox(string hash, IEnumerable<POI> pois)
        {
            return pois.Where(poi => gridRepository.CheckCoordinateInHash(hash, poi.Coordinates));
        }

        private IEnumerable<POI> MapYelpToPOI(IEnumerable<YelpBusiness> businesses)
        {
            var POIs = new List<POI>();
            foreach (var business in businesses)
            {
                var poi = new POI()
                {
                    Id = business.Id,
                    Phone = business.DisplayPhone,
                    Address = new Address()
                    {
                        Line1 = business.Location.Address1,
                        Line2 = business.Location.Address2,
                        City = business.Location.City,
                        State = business.Location.State,
                        Zip = business.Location.ZipCode
                    },
                    BusinessName = business.Name,
                    Coordinates = new Coordinate()
                    {
                        Latitude = business.Coordinate.Latitude,
                        Longitude = business.Coordinate.Longitude
                    },
                    Info = business.Url
                };

                if (business.Hours != null)
                {
                    var openHours = new List<YelpHour>();

                    foreach (var hour in business.Hours)
                        openHours.AddRange(hour.Open);

                    poi.Hours = openHours;
                }
                
                POIs.Add(poi);
            }
            return POIs;
        }
    }
}

