using System;
using Library.Models.Business;

namespace Library.Models
{
    public class GridBox
    {
        public string GeoHash { get; set; }
        public Coordinate Center { get; set; } //Don't know if we'll use this
        public double Radius { get; set; } // or this
        public IEnumerable<POI> POIs { get; set; }
    }
}

