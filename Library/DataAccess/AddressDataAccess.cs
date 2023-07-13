using System;
using System.Net;
using Library.DataAccess.Interfaces;
using Library.Models;
using Library.Repositories.Utilities;
using Library.Repositories.Utilities.Interfaces;
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

        public Result SaveAddressHash((int id, string hash) address)
		{
            try
            {
                using (var connection = NpgsqlDataSource.Create(settings.BackstoneDB))
                using (var command = connection.CreateCommand("mw_update_address_hash"))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
					command.Parameters.AddWithValue("@address_id", NpgsqlTypes.NpgsqlDbType.Bigint, address.id);
                    command.Parameters.AddWithValue("@hash", NpgsqlTypes.NpgsqlDbType.Varchar, address.hash);

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

