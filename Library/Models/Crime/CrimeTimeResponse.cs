using System;
using Library.Models.Responses;

namespace Library.Models.Crime
{
	public class CrimeTimeResponse
	{
		public int Id { get; set; }

		public IEnumerable<CrimeResponse> Crimes { get; set; }
	}
}

