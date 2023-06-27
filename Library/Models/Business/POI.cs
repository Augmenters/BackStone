using System;
using Library.Models.Yelp;

namespace Library.Models.Business
{
    public sealed class POI
    {
        public string Id { get; set; }
        public string BusinessName { get; set; }
        public string Phone { get; set; }
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        public string Price { get; set; }
        public Coordinate Coordinates { get; set; }
        public Address Address { get; set; }
        public string Info { get; set; }
        public IEnumerable<YelpHour> Hours { get; set; }
        public IEnumerable<YelpCategory> Categories { get; set; }

        public bool Validate()
        {
            return !string.IsNullOrWhiteSpace(Id)
                && !string.IsNullOrWhiteSpace(BusinessName)
                && !string.IsNullOrWhiteSpace(Phone)
                && !string.IsNullOrWhiteSpace(Info)
                && Address.Validate();
        }
    }
}

