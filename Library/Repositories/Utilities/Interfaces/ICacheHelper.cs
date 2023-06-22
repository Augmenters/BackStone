using System;
namespace Library.Repositories.Utilities.Interfaces
{
	public interface ICacheHelper
	{
        bool TryGetValue<T>(string key, out T? value);
        void Set(string key, object data);
    }
}

