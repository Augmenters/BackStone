using System;
using System.Collections.Concurrent;
using System.Net;
using Library.DataAccess;
using Library.DataAccess.Interfaces;
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
        private readonly IAddressDataAccess addressDataAccess;
        private readonly ICacheHelper cache;
        private readonly ISettings settings;

		public BusinessRepository(IYelpDataAccess yelpDataAccess,
                                  IGridRepository gridRepository,
                                  IAddressDataAccess addressDataAccess,
                                  ICacheHelper cache,
                                  ISettings settings)
		{
            this.yelpDataAccess = yelpDataAccess;
            this.gridRepository = gridRepository;
            this.addressDataAccess = addressDataAccess;
            this.cache = cache;
            this.settings = settings;
		}

        public async Task<DataResult<IEnumerable<POI>>> GetPOIs(Coordinate coordinate)
        {
            var POIList = new ConcurrentBag<POI>();
            var gridBoxes = gridRepository.GenerateGrid(coordinate);

            await Parallel.ForEachAsync(gridBoxes, async (box, token) =>
            {
                if (cache.TryGetValue<IEnumerable<POI>>(box.GeoHash, out var cachedPOIs))
                {
                    foreach (var poi in cachedPOIs!)
                    {
                        POIList.Add(poi);
                    }
                }
                else
                {
                    var offset = 0;
                    var totalResultCount = 1;
                    DataResult<IEnumerable<YelpBusiness>> result = new DataResult<IEnumerable<YelpBusiness>>() { IsSuccessful = false };

                    var allPois = new List<YelpBusiness>();
                    while (offset < totalResultCount)
                    {
                        result = await yelpDataAccess.BusinessQuery(box.Center, offset);

                        if (result.ErrorId == HttpStatusCode.NotFound)
                            cache.Set(box.GeoHash, new List<POI>());

                        if (!result.IsSuccessful || result.Data == null)
                            return;

                        totalResultCount = result.Total;

                        allPois.AddRange(result.Data);
                        offset += settings.Limit;
                    }

                    var converted = MapYelpToPOI(allPois);
                    var valid = RemovePOIsOutsideOfBox(box.GeoHash, converted).ToList();
                    var allAddresses = FillOpenAddressSlots(box.GeoHash, valid);
                    cache.Set(box.GeoHash, allAddresses);

                    foreach (var poi in allAddresses)
                    {
                        POIList.Add(poi);
                    }
                }
            });

            return POIList.Any()
                 ? new DataResult<IEnumerable<POI>> { IsSuccessful = true, Data = POIList }
                 : new DataResult<IEnumerable<POI>> { IsSuccessful = false, ErrorId = HttpStatusCode.NotFound, ErrorMessage = "No POIs returned from search" };
        }

        private IEnumerable<POI> FillOpenAddressSlots(string hash, List<POI> poisInHash)
        {
            var openAddresses = addressDataAccess.GetAddressesInHash(hash)?.ToList();

            if (!openAddresses?.Any() ?? false)
                return poisInHash;

            foreach(var poi in poisInHash)
            {
                var match = openAddresses.FirstOrDefault(x =>
                {
                    return poi.Address.Line1.ToLower().Contains(x.Number.ToString())
                        && poi.Address.Line1.ToLower().Contains(x.Street)
                        && poi.Address.City.ToLower().Contains(x.City);
                });

                if(match != null)
                    openAddresses.Remove(match);
            }

            foreach(var address in openAddresses)
            {
                var line1 = string.Join(' ', address.Number, address.Street, address.Unit);
                poisInHash.Add(new POI()
                {
                    Address = new Address()
                    {
                        Line1 = line1,
                        City = address.City,
                        State = address.State,
                        Zip = address.Zip
                    },
                    BusinessName = string.Join(' ', "Open Address:", line1),
                    Coordinates = new Coordinate()
                    {
                        Latitude = double.Parse(address.Latitude),
                        Longitude = double.Parse(address.Longitude)
                    }
                }) ;
            }

            return poisInHash;
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
                    Rating = business.Rating,
                    ReviewCount = business.ReviewCount,
                    Price = business.Price,
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

                poi.Categories = business.Categories != null
                               ? business.Categories
                               : new List<YelpCategory>();

                POIs.Add(poi);
            }
            return POIs;
        }
    }
}

