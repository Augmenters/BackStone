using System;
using Library.Models.Business;
using Library.Repositories.Utilities.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Library.Repositories.Utilities
{
	//using this as a wrapper so that IMemoryCache can be faked for testing
	public class CacheHelper : ICacheHelper
	{
		private readonly IMemoryCache cache;

		public CacheHelper(IMemoryCache cache)
		{
			this.cache = cache;
		}

		public bool TryGetValue<T>(string key, out T? value)
		{
			return cache.TryGetValue<T>(key, out value);
		}

        public void Set(string key, object data)
        {
            var options = new MemoryCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
            };

            cache.Set(key, data, options);
        }
    }
}

