using System;
using System.Net;
using Library.DataAccess.Interfaces;
using Library.Models;
using Library.Repositories.Utilities;
using Library.Repositories.Utilities.Interfaces;
using Newtonsoft.Json;
using Npgsql;

namespace Library.DataAccess
{
	public class AddressDataAccess : IAddressDataAccess
	{
		private readonly ISettings settings;

		public AddressDataAccess(ISettings settings)
		{
			this.settings = settings;
		}

		public IEnumerable<(int id, Coordinate coordinate)> GetUnhashedAddresses()
		{
            try
            {
                var addresses = new List<(int, Coordinate)>();
                using (var connection = NpgsqlDataSource.Create(settings.BackstoneDB))
                using (var command = connection.CreateCommand("SELECT * from gt_unhashed_addresses()"))
                using(var reader = command.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        var coordinate = new Coordinate()
                        {
                            Latitude = double.Parse(reader["latitude"]?.ToString()),
                            Longitude = double.Parse(reader["longitude"]?.ToString())
                        };

                        var id = int.Parse(reader["id"]?.ToString());

                        addresses.Add((id, coordinate));
                    }
                }

                return addresses;
            }
            catch (Exception ex)
            {
                ex.Log("Failed to get unhashed addresses");
                return null;
            }
        }

        public IEnumerable<OpenAddress> GetAddressesInHash(string hash)
        {
            try
            {
                var addresses = new List<OpenAddress>();
                using (var connection = NpgsqlDataSource.Create(settings.BackstoneDB))
                using (var command = connection.CreateCommand($"SELECT * from gt_addresses_in_gridhash('{hash}')"))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var address = new OpenAddress()
                        {
                            Number = int.Parse(reader["number"]?.ToString()),
                            Street = reader["street"]?.ToString(),
                            Unit = reader["unit"]?.ToString(),
                            City = reader["city"]?.ToString(),
                            State = reader["state"]?.ToString(),
                            Zip = reader["zipcode"]?.ToString(),
                            Longitude = reader["longitude"]?.ToString(),
                            Latitude = reader["latitude"]?.ToString()
                        };

                        addresses.Add(address);
                    }
                }

                return addresses;
            }
            catch (Exception ex)
            {
                ex.Log("Failed to get addresses in hash");
                return null;
            }
        }

        public Result SaveAddressHashes(IEnumerable<(int address_id, string hash)> addresses)
		{
            try
            {
                var json = JsonConvert.SerializeObject(addresses.Select(x => new { address_id = x.address_id, hash = x.hash }));
                using (var connection = NpgsqlDataSource.Create(settings.BackstoneDB))
                using (var command = connection.CreateCommand("mw_update_address_hash"))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
					command.Parameters.AddWithValue(NpgsqlTypes.NpgsqlDbType.Json, json);

					command.ExecuteNonQuery();
                }

                return new Result { IsSuccessful = true };
            }
            catch (Exception ex)
            {
                ex.Log("Failed to update address hash");
				return new Result() { IsSuccessful = false, ErrorMessage = ex.Message };
            }
        }

	}
}

