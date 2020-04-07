using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace CodeLifter.Http.Caching
{
    public class InMemoryCache : ICacheService
    {
        public void Flush()
        {
            List<string> cacheKeys = MemoryCache.Default.Select(kvp => kvp.Key).ToList();
            foreach (string cacheKey in cacheKeys)
            {
                MemoryCache.Default.Remove(cacheKey);
            }
        }

        public T Get<T>(string cacheKey) where T : class
        {
            return MemoryCache.Default.Get(cacheKey) as T;
        }
        public void Set(string cacheKey, object item, int minutes = 0)
        {
            if (item != null)
            {
                MemoryCache.Default.Add(cacheKey, item, DateTime.Now.AddMinutes(minutes));
            }
        }
    }
}
