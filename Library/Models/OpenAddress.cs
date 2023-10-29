using System;
namespace Library.Models
{
	public sealed class OpenAddress
	{
		public int Number { get; set; }
		public string Street { get; set; }
		public string Unit { get; set; }
		public string City { get; set; }
		public string State { get; set; }
		public string Zip { get; set; }
		public string Longitude { get; set; }
		public string Latitude { get; set; }
	}
}

