using System;
using Geohash;
using Library.Models;
using Library.Repositories.Interfaces;
using Library.Repositories.Utilities;

namespace Library.Repositories
{
	public class GridRepository : IGridRepository
	{
        private readonly ISettings settings;
        private readonly Geohasher geoHasher;

        public GridRepository(ISettings settings, Geohasher geoHasher)
        {
            this.settings = settings;
            this.geoHasher = geoHasher;
        }

        public string GenerateHash(Coordinate coordinate)
        {
            return geoHasher.Encode(coordinate.Latitude, coordinate.Longitude, settings.GridPrecision);
        }

        public Coordinate DecodeHash(string hash)
        {
            var coordinate = geoHasher.Decode(hash);
            return new Coordinate() { Latitude = coordinate.latitude, Longitude = coordinate.longitude };
        }

        public bool CheckCoordinateInHash(string hash, Coordinate coordinate)
        {
            var newHash = geoHasher.Encode(coordinate.Latitude, coordinate.Longitude, settings.GridPrecision);
            return newHash.Contains(hash);
        }

        public IEnumerable<GridBox> GenerateGrid(Coordinate coordinate)
        {
            var hash = GenerateHash(coordinate);
            var neighbors = geoHasher.GetNeighbors(hash).Select(x=>x.Value);

            return neighbors.Append(hash).Select(x => new GridBox(x, DecodeHash(x)));
        }
    }
}

