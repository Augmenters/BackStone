using System;
namespace Library.Models.Crime
{
    public sealed class TimeSlot
    {
        public int DayOfWeek { get; set; }
        public int StartHour { get; set; }
        public int EndHour { get; set; }
        public int Id { get; set; }
    }
}

