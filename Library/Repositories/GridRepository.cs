using System;
using Geohash;
using Library.Models;
using Library.Repositories.Interfaces;
using Library.Repositories.Utilities;
using Library.Repositories.Utilities.Interfaces;

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

        public IEnumerable<Coordinate> GetBoundingBoxCoordinates(string geohash)
        {
            var boundingBox = geoHasher.GetBoundingBox(geohash);

            // Returned coordinates must be in this order to render correctly in map view
            // 2 -- 3
            // |    |
            // 1 -- 4

            return new List<Coordinate>()
            { 
                new Coordinate() { Latitude = boundingBox.MinLat, Longitude = boundingBox.MinLng }, //1
                new Coordinate() { Latitude = boundingBox.MinLat, Longitude = boundingBox.MaxLng }, //2
                new Coordinate() { Latitude = boundingBox.MaxLat, Longitude = boundingBox.MaxLng }, //3
                new Coordinate() { Latitude = boundingBox.MaxLat, Longitude = boundingBox.MinLng }  //4
            };
        }
    }
}

