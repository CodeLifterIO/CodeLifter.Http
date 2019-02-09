﻿using System;

namespace CodeLifter.Http.Caching
{
    public class InMemoryCache : ICacheService
    {
        public T Get<T>(string cacheKey) where T : class
        {
            //return MemoryCache.Default.Get(cacheKey) as T;
            return null;
        }
        public void Set(string cacheKey, object item, int minutes = 10)
        {
            if (item != null)
            {
                //MemoryCache.Default.Add(cacheKey, item, DateTime.Now.AddMinutes(minutes));
            }
        }
    }
}