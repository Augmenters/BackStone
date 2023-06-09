using System;
using Library.DataAccess;
using Library.Models;
using Library.Models.Business;
using Library.Models.Yelp;
using Library.Repositories.Interfaces;

namespace Library.Repositories
{
	public class BusinessRepository : IBusinessRepository
	{
        private readonly IYelpDataAccess yelpDataAccess;

		public BusinessRepository(IYelpDataAccess yelpDataAccess)
		{
            this.yelpDataAccess = yelpDataAccess;
		}

        public async Task<DataResult<IEnumerable<POI>>> GetPOIs(Coordinate coordinate)
        {
            var radius = 2500; // this will be based off of grid size when we get there

            var result = await yelpDataAccess.BusinessQuery(coordinate, radius);

            if (!result.IsSuccessful || result.Data == null)
                return new DataResult<IEnumerable<POI>> { IsSuccessful = false, ErrorId = result.ErrorId, ErrorMessage = result.ErrorMessage };

            var POIs = MapYelpToPOI(result.Data);

            return new DataResult<IEnumerable<POI>> { IsSuccessful = true, Data = POIs };
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

