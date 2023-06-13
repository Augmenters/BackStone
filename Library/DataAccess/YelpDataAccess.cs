using System;
using Library.Models.Business;
using Library.Models.Yelp;
using Newtonsoft.Json;
using System.Runtime;
using Library.Models;
using Library.Repositories.Utilities;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using GraphQL;
using System.Net.Http.Headers;
using System.Collections.ObjectModel;
using System.Net;

namespace Library.DataAccess
{
	public class YelpDataAccess : IYelpDataAccess
	{
        private readonly ISettings settings;

        public YelpDataAccess(ISettings settings)
        {
            this.settings = settings;
        }

        public async Task<DataResult<IEnumerable<YelpBusiness>>> BusinessQuery(Coordinate coordinate)
        {
            try
            {
                using (var client = new GraphQLHttpClient(settings.YelpGraphQLUrl, new NewtonsoftJsonSerializer()))
                {
                    client.HttpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + settings.YelpApiKey);

                    var request = BuildRequest(coordinate, settings.SearchRadius);

                    var result = await client.SendQueryAsync<BusinessQueryResponse>(request);

                    if(result.Errors?.Any() ?? false)
                        return new DataResult<IEnumerable<YelpBusiness>> { IsSuccessful = false, ErrorMessage = result.Errors?.First().Message ?? "search failed" };

                    return result.Data?.Search?.Businesses != null
                         ? new DataResult<IEnumerable<YelpBusiness>> { IsSuccessful = true, Data = result.Data.Search.Businesses }
                         : new DataResult<IEnumerable<YelpBusiness>> { IsSuccessful = false, ErrorId = HttpStatusCode.NotFound };
                }

            }
            catch (Exception ex)
            {
                ex.Data["coordinate"] = JsonConvert.SerializeObject(coordinate);
                return new DataResult<IEnumerable<YelpBusiness>> { IsSuccessful = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<DataResult<BusinessReviewsResponse>> GetReviews(string businessId)
        {
            try
            {
                var resource = $"/businesses/{businessId}/reviews";

                var headers = new Collection<KeyValuePair<string, string>>()
                {
                    new ("accept", "application/json"),
                    new ("Authorization", $"Bearer {settings.YelpApiKey}")
                };

                using (var client = new WebApiClientHelper(settings.YelpApiBaseUrl))
                {
                    var result = await client.GetAsync<BusinessReviewsResponse>(resource, headers);
                    return result;
                }

            }
            catch (Exception ex)
            {
                ex.Data["Business Id"] = businessId;
                return new DataResult<BusinessReviewsResponse> { IsSuccessful = false, ErrorMessage = ex.Message };
            }
        }

        private GraphQLRequest BuildRequest(Coordinate coordinate, double radius)
        {
            return new GraphQLRequest()
            {
                Query = @"query BusinessSearch($latitude: Float!, $longitude: Float!, $radius: Float!)
						{ 
							search(latitude: $latitude, longitude: $longitude, radius: $radius)
							{ 
								total
								business
								{ 
									id 
									coordinates
									{ 
										latitude 
										longitude 
									} 
									display_phone 
									name 
									rating 
									review_count 
									url 
									price 
									location
									{ 
										address1 
										city 
										state 
									} 
									hours
									{ 
										open
										{ 
											start
											end 
											day 
										} 
									} 
								} 
							}
						}",
                OperationName = "BusinessSearch",
                Variables = new
                {
                    latitude = Convert.ToSingle(coordinate.Latitude),
                    longitude = Convert.ToSingle(coordinate.Longitude),
                    radius = Convert.ToSingle(radius)
                },
            };
        }
    }


}


