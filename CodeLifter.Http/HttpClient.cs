namespace CodeLifter.Http
{
    using CodeLifter.Http.Caching;
    using CodeLifter.Http.Logging;
    using CodeLifter.Http.Serialization;
    using RestSharp;
    using RestSharp.Deserializers;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    public class HttpClient : RestSharp.RestClient, IHttpClient
    {
        private IDictionary<string, string> CustomHeaders = new Dictionary<string, string>();

        protected ICacheService Cache;
        protected ILogger Logger;
        protected IDeserializer Deserializer;

        public HttpClient(string baseUrl, bool logDebug = false, bool logToConsole = true)
        {
            Cache = new InMemoryCache();
            Logger = new Logger(logDebug, logToConsole);
            Deserializer = new JsonSerializer();
            InitSelf(baseUrl);
        }

        public HttpClient(ICacheService cache, IDeserializer deserializer, ILogger logger, string baseUrl, bool logDebug = false, bool logToConsole = true)
        {
            Cache = cache;
            Logger = new Logger(logDebug, logToConsole);
            Deserializer = deserializer;
            InitSelf(baseUrl);
        }

        private void InitSelf(string baseUrl)
        {
            AddHandler("application/json", Deserializer);
            AddHandler("text/json", Deserializer);
            AddHandler("text/x-json", Deserializer);
            BaseUrl = new Uri(baseUrl);
        }

        public void AddHeader(string name, string value)
        {
            RemoveHeader(name);
            CustomHeaders.Add(name, value);
        }

        public void RemoveHeader(string name)
        {
            CustomHeaders.Remove(name);
        }

        public void ClearHeaders()
        {
            CustomHeaders.Clear();
        }

        private void TimeoutCheck(IRestRequest request, IRestResponse response)
        {
            if (response.StatusCode == 0)
            {
                Logger.LogError(BaseUrl.ToString(), request.Resource);
            }
        }


        public async Task<T> Get<T>(IRestRequest request, string cacheKey = null)
        {
            Logger.LogMessage($"{BaseUrl}/{request.Resource}");

            IRestResponse<T> response = await ExecuteAsync<T>(request);

            Logger.LogMessage(response.Content);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                if(!string.IsNullOrWhiteSpace(cacheKey))
                {
                    Cache.Set(cacheKey, response.Data);
                }
                return response.Data;
            }
            else
            {
                Logger.LogError(BaseUrl.ToString(), request.Resource);
                return default(T);
            }
        }

        public async Task<T> GetFromCache<T>(IRestRequest request, string cacheKey) where T : class
        {
            var item = Cache.Get<T>(cacheKey);

            if (item == null)
            {
                item = await Get<T>(request, cacheKey);
            }
            return item;
        }

        public async Task<T> Post<T>(IRestRequest request)
        {
            return await CreateOrEdit<T>(request);
        }

        public async Task<T> Put<T>(IRestRequest request)
        {
            return await CreateOrEdit<T>(request);
        }

        public async Task<T> Post<T>(IRestRequest request, object data, params string[] includedProperties)
        {
            return await CreateOrEdit<T>(request, data, includedProperties);
        }

        public async Task<T> Put<T>(IRestRequest request, object data, params string[] includedProperties)
        {
            return await CreateOrEdit<T>(request, data, includedProperties);
        }

        /// <summary>
        /// Executes a non-cached (POST/PUT), but type returning command
        /// </summary>
        /// <returns>The or edit.</returns>
        /// <param name="request">Request.</param>
        /// <typeparam name="T">expected return type</typeparam>
        private async Task<T> CreateOrEdit<T>(IRestRequest request)
        {
            IRestResponse<T> response = await ExecuteAsync<T>(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Data;
            }
            else
            {
                Logger.LogError(BaseUrl.ToString(), request.Resource);
                return default(T);
            }
        }

        /// <summary>
        /// Adds data and whitelisted properties if needed, then calls the simpler CreateOrEdit
        /// </summary>
        /// <returns>The or edit.</returns>
        /// <param name="request">Request.</param>
        /// <param name="data">Data.</param>
        /// <param name="includedProperties">Included properties.</param>
        /// <typeparam name="T">Expected return type</typeparam>
        private async Task<T> CreateOrEdit<T>(IRestRequest request, object data, params string[] includedProperties)
        {
            if (null == includedProperties || includedProperties.Count<string>() == 0)
            {
                request.AddObject(data);
            }
            else
            {
                request.AddObject(data, includedProperties);
            }
            return await CreateOrEdit<T>(request);
        }

        public override async Task<IRestResponse<T>> ExecuteTaskAsync<T>(IRestRequest request)
        {
            foreach(KeyValuePair<string, string> header in CustomHeaders)
            {
                request.AddHeader(header.Key, header.Value);
            }

            IRestResponse<T> response = await base.ExecuteAsync<T>(request);

            TimeoutCheck(request, response);
            return response;
        }
    }
}
