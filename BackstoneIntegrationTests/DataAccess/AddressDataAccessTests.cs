using System;
using System.Net;
using Library.DataAccess;
using Library.DataAccess.Interfaces;
using Library.Models;
using Library.Repositories.Utilities;
using Library.Repositories.Utilities.Interfaces;
using Npgsql;
using NUnit.Framework;

namespace IntegrationTests.RepositoryTests
{
	public class AddressDataAccessTests
	{
        private IAddressDataAccess dataAccess;
        private ISettings settings;

        [SetUp]
        public void Setup()
        {
            settings = new TestSettings();
            dataAccess = new AddressDataAccess(settings);

            //env file is not passed in because were not running these in a container so these need to be set
            Environment.SetEnvironmentVariable("POSTGRES_USER", "backstone_user");
            Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", "password"); 
            Environment.SetEnvironmentVariable("POSTGRES_DB", "base_db"); 
        }

        [Test]
        public void GetUnhashedAddresses_GetsUnhashedAddresses()
        {
            //Arrange
            var addressId = 1;
            RemoveAddressHash(addressId);

            //Act
            var result = dataAccess.GetUnhashedAddresses();

            //Assert
            Assert.IsTrue(result.Any());
            Assert.IsTrue(result.First().id != 0);
            Assert.IsTrue(result.First().coordinate.Latitude != 0);
            Assert.IsTrue(result.First().coordinate.Longitude != 0);
        }

        [Test]
        public void UpdateAddressHash_UpdatesAddressHash()
        {
            //Arrange
            var addressId = 1;
            var newHash = "new hash";
            RemoveAddressHash(addressId);

            //Act
            var result = dataAccess.SaveAddressHashes(new[] { (1, newHash) });

            //Assert
            var hashFromDb = GetAddressHash(addressId);

            Assert.IsTrue(result.IsSuccessful);
            Assert.IsTrue(newHash == hashFromDb);
        }

        private string GetAddressHash(int addressId)
        {
            var commandText = @$"select geohash from addresses where ""id"" = {addressId}";

            using (var connection = NpgsqlDataSource.Create(settings.BackstoneDB))
            using (var command = connection.CreateCommand(commandText))
            using (var reader = command.ExecuteReader())
                if (reader.Read())
                    return reader["geohash"].ToString();

            return null;
        }

        private void RemoveAddressHash(int addressId)
        {
            var commandText = @$"update addresses set geohash = null where ""id"" = {addressId}";

            using (var connection = NpgsqlDataSource.Create(settings.BackstoneDB))
            using (var command = connection.CreateCommand(commandText))
                command.ExecuteNonQuery();
        }
    }
}

