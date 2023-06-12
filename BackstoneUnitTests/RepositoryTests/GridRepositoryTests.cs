using System;
using FakeItEasy;
using Geohash;
using Library.DataAccess;
using Library.Models;
using Library.Repositories;
using Library.Repositories.Interfaces;
using Library.Repositories.Utilities;
using Library.Repositories.Utilities.Interfaces;
using NUnit.Framework;

namespace UnitTests.RepositoryTests
{
    public class GridRepositoryTests
    {
        private Geohasher geoHasher;
        private ISettings testSettings;
        private IGridRepository gridRepository;

        [SetUp]
        public void Setup()
        {
            this.testSettings = new TestSettings();
            this.geoHasher = new Geohasher();
            this.gridRepository = new GridRepository(testSettings, geoHasher);
        }

        [Test]
        public void VerifyHashContainsCoordinate_ReturnsSuccessWhenCoordinateInHash()
        {
            //Arrange
            var coordinate = new Coordinate()
            {
                Latitude = 38.95201,
                Longitude = -92.33701
            };

            var coordinateHash = gridRepository.GenerateHash(coordinate);

            //Act
            var result = gridRepository.VerifyHashContainsCoordinate(coordinateHash, coordinate);

            //Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void GenerateGrid_GeneratesGrid()
        {
            //Arrange
            var coordinate = new Coordinate()
            {
                Latitude = 38.95201,
                Longitude = -92.33701
            };

            var coordinateHash = gridRepository.GenerateHash(coordinate);
            var gridBoxRadius = GeoHashPrecisionMap.Get(testSettings.GridPrecision);

            //Act
            var grid = gridRepository.GenerateGrid(coordinate);

            //Assert
            Assert.IsTrue(grid != null);
            Assert.IsTrue(grid.Any());
            Assert.IsTrue(grid.Count() == 9, "Grid did not generate all boxes");
            Assert.IsTrue(grid.All(x => x.Radius == gridBoxRadius), "Grid radius was not set correctly");
            Assert.IsTrue(grid.Single(x => x.GeoHash == coordinateHash) != null, "Grid does not contain original hash");
        }
    }
}

