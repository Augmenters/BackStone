using System;
using Library.Models.Business;

namespace Library.Models
{
    public sealed class GridBox
    {
        public string GeoHash { get; private set; }
        public Coordinate Center { get; private set; }
        public IEnumerable<POI> POIs { get; private set; }

        public GridBox(string hash, Coordinate center)
        {
            this.GeoHash = hash;
            this.Center = center;
            this.POIs = new List<POI>();
        }
    }
}

