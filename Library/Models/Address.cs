using System;
namespace Library.Models
{
    public sealed class Address
    {
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }

        public bool Validate()
        {
            return !string.IsNullOrWhiteSpace(Line1)
                && !string.IsNullOrWhiteSpace(City)
                && !string.IsNullOrWhiteSpace(State)
                && !string.IsNullOrWhiteSpace(Zip);
        }
    }
}

