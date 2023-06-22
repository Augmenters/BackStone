using System;
using System.Net;
using FakeItEasy;
using Geohash;
using Library.DataAccess;
using Library.Models;
using Library.Models.Business;
using Library.Models.Yelp;
using Library.Repositories;
using Library.Repositories.Interfaces;
using Library.Repositories.Utilities.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using NUnit.Framework;

namespace BackstoneUnitTests.RepositoryTests
{
    public class BusinessRepositoryTests
    {
        private IYelpDataAccess fakeYelpDataAccess;
        private IGridRepository fakeGridRepository;
        private ICacheHelper fakeCache;
        private IBusinessRepository businessRepository;

        [SetUp]
        public void Setup()
        {
            fakeYelpDataAccess = A.Fake<IYelpDataAccess>();
            fakeGridRepository = A.Fake<IGridRepository>();
            fakeCache = A.Fake<ICacheHelper>();
            businessRepository = new BusinessRepository(fakeYelpDataAccess, fakeGridRepository, fakeCache);
        }

        [Test]
        [Category("Search")]
        public async Task BusinessSearch_ReturnsFailure_WhenNoBusinessesFound()
        {
            //Arrange
            A.CallTo(() => fakeGridRepository.GenerateGrid(A<Coordinate>.Ignored))
             .Returns(new List<GridBox>()
             {
                 new GridBox(string.Empty, new Coordinate())
             });

            IEnumerable<POI>? ignored = null;

            A.CallTo(() => fakeCache.TryGetValue<IEnumerable<POI>>(A<string>.Ignored, out ignored))
             .Returns(false);

            A.CallTo(() => fakeYelpDataAccess.BusinessQuery(A<Coordinate>.Ignored))
             .Returns(Task.FromResult(new DataResult<IEnumerable<YelpBusiness>>
             {
                 IsSuccessful = false,
                 Data = null,
                 ErrorId = HttpStatusCode.NotFound
             }));

            //Act
            var result = await businessRepository.GetPOIs(new Coordinate());

            //Assert
            A.CallTo(() => fakeYelpDataAccess.BusinessQuery(A<Coordinate>.Ignored)).MustHaveHappened();
            Assert.IsFalse(result.IsSuccessful);
            Assert.IsTrue(result.ErrorId == HttpStatusCode.NotFound);
        }

        [Test]
        [Category("Search")]
        public async Task BusinessSearch_MapsYelpToPOI_WhenSuccessful_Cached()
        {
            //Arrange
            var returnedPOI = new POI()
            {
                Id = "id",
                BusinessName = "name",
                Rating = 1,
                ReviewCount = 1,
                Phone = "123",
                Price = "$",
                Coordinates = new Coordinate()
                {
                    Latitude = 1,
                    Longitude = 1
                },
                Address = new Address()
                {
                    Line1 = "add1",
                    Line2 = "add2",
                    City = "city",
                    State = "state",
                    Zip = "65201"
                },
                Info = "url"
            };

            A.CallTo(() => fakeGridRepository.GenerateGrid(A<Coordinate>.Ignored))
             .Returns(new List<GridBox>()
             {
                 new GridBox(string.Empty, new Coordinate())
             });

            IEnumerable<POI>? ignored = null;

            A.CallTo(() => fakeCache.TryGetValue<IEnumerable<POI>>(A<string>.Ignored, out ignored))
             .Returns(true)
             .AssignsOutAndRefParameters(new List<POI>
             {
                returnedPOI
             });

            //Act
            var result = await businessRepository.GetPOIs(new Coordinate());

            //Assert
            A.CallTo(() => fakeYelpDataAccess.BusinessQuery(A<Coordinate>.Ignored)).MustNotHaveHappened();
            Assert.IsTrue(result.IsSuccessful);
            Assert.IsTrue(result.Data != null);
            Assert.IsTrue(result.Data.First().Validate());
        }

        [Test]
        [Category("Search")]
        public async Task BusinessSearch_MapsYelpToPOI_WhenSuccessful_NotCached()
        {
            //Arrange
            var returnedBusiness = new YelpBusiness()
            {
                Id = "id",
                Name = "name",
                Rating = 1,
                ReviewCount = 1,
                DisplayPhone = "123",
                Price = "$",
                Coordinate = new YelpCoordinate()
                {
                    Latitude = 1,
                    Longitude = 1
                },
                Location = new YelpAddress()
                {
                    Address1 = "add1",
                    Address2 = "add2",
                    City = "city",
                    State = "state",
                    ZipCode = "65201",
                    Country = ""
                },
                Url = "url"
            };

            A.CallTo(() => fakeGridRepository.GenerateGrid(A<Coordinate>.Ignored))
             .Returns(new List<GridBox>()
             {
                 new GridBox(string.Empty, new Coordinate())
             });

            A.CallTo(() => fakeGridRepository.CheckCoordinateInHash(A<string>.Ignored, A<Coordinate>.Ignored))
             .Returns(true);

            IEnumerable<POI>? ignored = null;

            A.CallTo(() => fakeCache.TryGetValue<IEnumerable<POI>>(A<string>.Ignored, out ignored))
             .Returns(false);

            A.CallTo(() => fakeYelpDataAccess.BusinessQuery(A<Coordinate>.Ignored))
             .Returns(Task.FromResult(new DataResult<IEnumerable<YelpBusiness>>
             {
                 IsSuccessful = true,
                 Data = new List<YelpBusiness>() { returnedBusiness }
             }));

            //Act
            var result = await businessRepository.GetPOIs(new Coordinate());

            //Assert
            A.CallTo(() => fakeYelpDataAccess.BusinessQuery(A<Coordinate>.Ignored)).MustHaveHappened();
            Assert.IsTrue(result.IsSuccessful);
            Assert.IsTrue(result.Data != null);
            Assert.IsTrue(result.Data.First().Validate());
        }

        [Test]
        [Category("GetReviews")]
        public async Task GetReviews_ReturnsReviews_NoCache()
        {
            //Arrange
            BusinessReviewsResponse? ignored = null;

            A.CallTo(() => fakeCache.TryGetValue<BusinessReviewsResponse>(A<string>.Ignored, out ignored))
             .Returns(false);

            A.CallTo(() => fakeYelpDataAccess.GetReviews(A<string>.Ignored))
             .Returns(Task.FromResult(new DataResult<BusinessReviewsResponse>
             {
                 IsSuccessful = true,
                 Data = new BusinessReviewsResponse
                 {
                     Reviews = new List<YelpReview>()
                     {
                         new YelpReview()
                     }
                 }
             }));

            //Act
            var result = await businessRepository.GetReviews("");

            //Assert
            Assert.IsTrue(result.IsSuccessful && result.Data != null);
            A.CallTo(() => fakeYelpDataAccess.GetReviews(A<string>.Ignored)).MustHaveHappened();
        }

        [Test]
        [Category("GetReviews")]
        public async Task GetReviews_ReturnsReviews_Cache()
        {
            //Arrange
            var returnedReview = new BusinessReviewsResponse
            {
                Reviews = new List<YelpReview>()
                {
                    new YelpReview()
                }
            };

            BusinessReviewsResponse? ignored = null;

            A.CallTo(() => fakeCache.TryGetValue<BusinessReviewsResponse>(A<string>.Ignored, out ignored))
             .Returns(true)
             .AssignsOutAndRefParameters(returnedReview);

            //Act
            var result = await businessRepository.GetReviews("");

            //Assert
            Assert.IsTrue(result.IsSuccessful && result.Data != null);
            A.CallTo(() => fakeYelpDataAccess.GetReviews(A<string>.Ignored)).MustNotHaveHappened();
        }
    }
}

