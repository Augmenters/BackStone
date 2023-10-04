using System;
namespace Library.Models.Crime
{
    public sealed class TimeSlot
    {
        public string DayOfWeek { get; set; }
        public int TimeOfDay { get; set; }
        public int Id { get; set; }
    }
}

