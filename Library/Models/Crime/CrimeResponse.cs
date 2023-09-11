using System;
namespace Library.Models.Responses
{
    public sealed class CrimeResponse
    {
        public string GridHash { get; set; }
        public int CrimeCount { get; set; }
        public IEnumerable<Coordinate> Coordinates { get; set; }
    }
}

