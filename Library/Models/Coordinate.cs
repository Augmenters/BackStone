using System;
namespace Library.Models
{
    public sealed class Coordinate
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public bool Validate()
        {
            return Latitude is > -90 or < 90
                && Longitude is > -180 or < 180;
        }
    }
}

