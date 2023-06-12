using System;
namespace Library.Repositories.Utilities
{
    public static class GeoHashPrecisionMap
    {
        // (distance, precision level) in KM
        public static readonly Dictionary<double, int> geoHashPrecisions = new()
        {
            {5000, 2 },
            {625, 3 },
            {156, 4 },
            {19.5, 5 },
            {4.9, 6 },
            {0.61, 7 },
            {0.152, 8 },
            {0.019, 9 }
        };

        public static int Map(double radius) => radius < 5000 ? geoHashPrecisions.Where(x => x.Key < radius).Last().Value : 1;

        public static double Get(int level)
        {
            if (level < 2)
                level = 2;

            if (level > 9)
                level = 9;

            return geoHashPrecisions.First(x => x.Value == level).Key;
        }
    }
}

