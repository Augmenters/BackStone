using System;
using System.Net;
using FakeItEasy;
using Library.DataAccess;
using Library.Models;
using Library.Models.Business;
using Library.Models.Yelp;
using Library.Repositories;
using Library.Repositories.Interfaces;
using NUnit.Framework;

namespace BackstoneUnitTests.RepositoryTests
{
	public class BusinessRepositoryTests
	{
        private IYelpDataAccess fakeYelpDataAccess;
        private IBusinessRepository businessRepository;

        [SetUp]
        public void Setup()
        {
            fakeYelpDataAccess = A.Fake<IYelpDataAccess>();
            businessRepository = new BusinessRepository(fakeYelpDataAccess);
        }

        [Test]
        public async Task BusinessSearch_ReturnsSuccess_WhenSuccessful()
        {
            //Arrange
            A.CallTo(() => fakeYelpDataAccess.BusinessQuery(A<Coordinate>.Ignored, A<double>.Ignored))
             .Returns(Task.FromResult(new DataResult<IEnumerable<YelpBusiness>>{
                 IsSuccessful = true,
                 Data = new List<YelpBusiness>()
             }));

            //Act
            var result = await businessRepository.GetPOIs(new Coordinate());

            //Assert
            A.CallTo(() => fakeYelpDataAccess.BusinessQuery(A<Coordinate>.Ignored, A<double>.Ignored)).MustHaveHappened();
            Assert.IsTrue(result.IsSuccessful);
        }

        [Test]
        public async Task BusinessSearch_ReturnsFailure_WhenUnauthorized()
        {
            //Arrange
            A.CallTo(() => fakeYelpDataAccess.BusinessQuery(A<Coordinate>.Ignored, A<double>.Ignored))
             .Returns(Task.FromResult(new DataResult<IEnumerable<YelpBusiness>>
             {
                 IsSuccessful = false,
                 Data = null,
                 ErrorId = HttpStatusCode.Unauthorized
             }));

            //Act
            var result = await businessRepository.GetPOIs(new Coordinate());

            //Assert
            A.CallTo(() => fakeYelpDataAccess.BusinessQuery(A<Coordinate>.Ignored, A<double>.Ignored)).MustHaveHappened();
            Assert.IsFalse(result.IsSuccessful);
            Assert.IsTrue(result.ErrorId == HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task BusinessSearch_ReturnsFailure_WhenNoBusinessesFound()
        {
            //Arrange
            A.CallTo(() => fakeYelpDataAccess.BusinessQuery(A<Coordinate>.Ignored, A<double>.Ignored))
             .Returns(Task.FromResult(new DataResult<IEnumerable<YelpBusiness>>
             {
                 IsSuccessful = false,
                 Data = null,
                 ErrorId = HttpStatusCode.NotFound
             }));

            //Act
            var result = await businessRepository.GetPOIs(new Coordinate());

            //Assert
            A.CallTo(() => fakeYelpDataAccess.BusinessQuery(A<Coordinate>.Ignored, A<double>.Ignored)).MustHaveHappened();
            Assert.IsFalse(result.IsSuccessful);
            Assert.IsTrue(result.ErrorId == HttpStatusCode.NotFound);
        }

        [Test]
        public async Task BusinessSearch_MapsYelpToPOI_WhenSuccessful()
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

            A.CallTo(() => fakeYelpDataAccess.BusinessQuery(A<Coordinate>.Ignored, A<double>.Ignored))
             .Returns(Task.FromResult(new DataResult<IEnumerable<YelpBusiness>>
             {
                 IsSuccessful = true,
                 Data = new List<YelpBusiness>() { returnedBusiness }
             }));

            //Act
            var result = await businessRepository.GetPOIs(new Coordinate());

            //Assert
            A.CallTo(() => fakeYelpDataAccess.BusinessQuery(A<Coordinate>.Ignored, A<double>.Ignored)).MustHaveHappened();
            Assert.IsTrue(result.IsSuccessful);
            Assert.IsTrue(result.Data != null);
            Assert.IsTrue(result.Data.First().Validate());
        }
    }
}

