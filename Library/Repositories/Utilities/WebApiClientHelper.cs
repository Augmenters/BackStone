using System;
using Library.Models;
using Newtonsoft.Json;
using RestSharp;

namespace Library.Repositories.Utilities
{
    public class WebApiClientHelper : RestClient
    {
        public WebApiClientHelper(string baseUrl) : base(baseUrl) { }

        public async Task<DataResult<T>> GetAsync<T>(string resource, ICollection<KeyValuePair<string, string>> headers, IEnumerable<Parameter> parameters = null)
        {
            var request = new RestRequest(resource, method: Method.Get);
            request.AddHeaders(headers);

            if (parameters != null)
                request.AddOrUpdateParameters(parameters);

            var response = await ExecuteAsync(request);

            if (!response.IsSuccessStatusCode || string.IsNullOrEmpty(response.Content))
                return new DataResult<T> { IsSuccessful = false, ErrorId = response.StatusCode, ErrorMessage = response.ErrorMessage };

            var data = JsonConvert.DeserializeObject<T>(response.Content);

            return new DataResult<T> { IsSuccessful = true, Data = data};
        }
    }
}

