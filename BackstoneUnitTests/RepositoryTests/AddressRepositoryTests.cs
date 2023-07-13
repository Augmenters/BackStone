using System;
using System.Net;
using FakeItEasy;
using Library.DataAccess.Interfaces;
using Library.Models;
using Library.Models.Business;
using Library.Models.Yelp;
using Library.Repositories;
using Library.Repositories.Interfaces;
using Library.Repositories.Utilities.Interfaces;
using NUnit.Framework;

namespace UnitTests.RepositoryTests
{
	public class AddressRepositoryTests
	{
		private IAddressDataAccess fakeAddressDataAccess;
		private IGridRepository fakeGridRepository;
		private IAddressRepository addressRepository;

        [SetUp]
        public void Setup()
        {
            fakeAddressDataAccess = A.Fake<IAddressDataAccess>();
            fakeGridRepository = A.Fake<IGridRepository>();
            addressRepository = new AddressRepository(fakeAddressDataAccess, fakeGridRepository);
        }

        [Test]
        [Category("address")]
        public void GetUnhashedAddresses_SavesAddressHashes_WhenAvailable()
        {
            //Arrange
            var addressId = 1;
            var hash = "a hash";

            A.CallTo(() => fakeAddressDataAccess.GetUnhashedAddresses())
             .Returns(new List<(int id, Coordinate coordinate)>()
             {
                 (addressId, new Coordinate(){ Latitude = 1, Longitude = 1 })
             });

            A.CallTo(() => fakeGridRepository.GenerateHash(A<Coordinate>.Ignored))
             .Returns(hash);

            A.CallTo(() => fakeAddressDataAccess.SaveAddressHash(A<(int, string)>.Ignored))
             .Returns(new Result() { IsSuccessful = true });

            //Act
            var result = addressRepository.HashUnhashedAddresses();

            //Assert
            Assert.IsTrue(result.IsSuccessful);
            A.CallTo(() => fakeAddressDataAccess.SaveAddressHash(A<(int, string)>.Ignored))
             .MustHaveHappened();
        }

        [Test]
        [Category("address")]
        public void GetUnhashedAddresse_ReturnsNotFound_WhenNoneAvailable()
        {
            //Arrange
            A.CallTo(() => fakeAddressDataAccess.GetUnhashedAddresses())
             .Returns(null);

            //Act
            var result = addressRepository.HashUnhashedAddresses();

            //Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.IsTrue(result.ErrorId == HttpStatusCode.NotFound);
            A.CallTo(() => fakeGridRepository.GenerateHash(A<Coordinate>.Ignored))
             .MustNotHaveHappened();
            A.CallTo(() => fakeAddressDataAccess.SaveAddressHash(A<(int, string)>.Ignored))
             .MustNotHaveHappened();
        }
    }
}

