using System;
using System.Net;

namespace Library.Models
{
	public class Result
	{
		public string ErrorMessage { get; set; }
		public bool IsSuccessful { get; set; }
		public HttpStatusCode ErrorId { get; set; }
    }

	public sealed class DataResult<T> : Result
	{
		public T Data { get; set; }
	}
}

