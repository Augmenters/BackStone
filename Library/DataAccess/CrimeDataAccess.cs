using System;
using Library.DataAccess.Interfaces;
using Library.Models.Crime;
using Library.Models.Responses;
using Library.Repositories.Utilities;
using Library.Repositories.Utilities.Interfaces;
using Npgsql;

namespace Library.DataAccess
{
	public class CrimeDataAccess : ICrimeDataAccess
	{
        private readonly ISettings settings;

		public CrimeDataAccess(ISettings settings)
		{
            this.settings = settings;
		}

        public IEnumerable<CrimeResponse> GetCrimes(int timeSlotId)
        {
            try
            {
                var crimes = new List<CrimeResponse>();
                using (var connection = NpgsqlDataSource.Create(settings.BackstoneDB))
                using (var command = connection.CreateCommand($"SELECT * from gt_crime_by_timeslot_id({timeSlotId})"))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var crime = new CrimeResponse()
                        {
                            GridHash = reader["grid_hash"].ToString(),
                            CrimeCount = int.Parse(reader["crime_count"]?.ToString())
                        };

                        crimes.Add(crime);
                    }
                }

                return crimes;
            }
            catch (Exception ex)
            {
                ex.Log("Failed to get crimes");
                return null;
            }
        }

        public IEnumerable<TimeSlot> GetTimeSlots()
        {
            try
            {
                var timeSlots = new List<TimeSlot>();
                using (var connection = NpgsqlDataSource.Create(settings.BackstoneDB))
                using (var command = connection.CreateCommand($"SELECT * from gt_time_slots()"))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var timeSlot = new TimeSlot()
                        {
                            Id = int.Parse(reader["id"]?.ToString()),
                            TimeOfDay = int.Parse(reader["time_of_day"]?.ToString()),
                            DayOfWeek = reader["day_of_week"]?.ToString()
                        };

                        timeSlots.Add(timeSlot);
                    }
                }

                return timeSlots;
            }
            catch (Exception ex)
            {
                ex.Log("Failed to get time slots");
                return null;
            }
        }
    }
}

