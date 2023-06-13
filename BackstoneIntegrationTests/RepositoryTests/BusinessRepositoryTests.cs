using System;
using Library.DataAccess;
using Library.Models;
using Library.Repositories.Utilities;
using NUnit.Framework;

namespace IntegrationTests.RepositoryTests
{
    namespace BackstoneUnitTests.RepositoryTests
    {
        public class BusinessRepositoryTests
        {
            private IYelpDataAccess yelpDataAccess;

            [SetUp]
            public void Setup()
            {
                yelpDataAccess = new YelpDataAccess(new TestSettings());
                Environment.SetEnvironmentVariable("YelpApiKey", ""); //Insert your key here or comment this line out if set locally
            }

            [Test]
            public async Task BusinessSearch_ReturnsSuccessAndData()
            {
                //Arrange
                var coordinate = new Coordinate()
                {
                    Latitude = 38.95201,
                    Longitude = -92.33701
                };

                //Act
                var result = await yelpDataAccess.BusinessQuery(coordinate);

                //Assert
                Assert.IsTrue(result.IsSuccessful);
                Assert.IsTrue(result.Data?.Any());
            }

            [Test]
            public async Task GetReviews_ReturnsSuccessAndData()
            {
                //ID of barred owl and butcher 
                var capturedBusinessId = "ro7BauyrL1NbNkUl0mV3mg";
                
                //Act
                var result = await yelpDataAccess.GetReviews(capturedBusinessId);

                //Assert
                Assert.IsTrue(result.IsSuccessful);
                Assert.IsTrue(result.Data!.Reviews.Any());
            }
        }
    }
}

