namespace CodeLifter.Http
{
    using CodeLifter.Http.Caching;
    using CodeLifter.Http.Serialization;
    using CodeLifter.Logging;
    using CodeLifter.Logging.Loggers;
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
        protected ILogRunner LogRunner;
        protected IDeserializer Deserializer;


        /// <summary>
        /// CONSTRUCTOR - Includes two very basic loggers
        /// </summary>
        /// <param name="baseUrl"></param>
        public HttpClient(string baseUrl)
        {
            Cache = new InMemoryCache();

            LogRunner = new LogRunner();
            LogRunner.AddLogger(new ConsoleLogger());
            LogRunner.AddLogger(new DebugLogger());

            Deserializer = new JsonSerializer();
            InitSelf(baseUrl);
        }

        /// <summary>
        /// Constructor that includes no default loggers. Call AddLogger on the instance to add loggers.
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="deserializer"></param>
        /// <param name="logger"></param>
        /// <param name="baseUrl"></param>
        public HttpClient(ICacheService cache, IDeserializer deserializer, string baseUrl)
        {
            Cache = cache;
            LogRunner = new LogRunner();
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

        public void AddLogger(ILogger log)
        {
            LogRunner.AddLogger(log);
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


//public void LogError(Uri BaseUrl, IRestRequest request, IRestResponse response)
//{
//    //Get the values of the parameters passed to the API
//    string parameters = string.Join(", ", request.Parameters.Select(x => x.Name.ToString() + "=" + ((x.Value == null) ? "NULL" : x.Value)).ToArray());

//    //Set up the information message with the URL, the status code, and the parameters.
//    string info = "Request to " + BaseUrl.AbsoluteUri + request.Resource + " failed with status code " + response.StatusCode + ", parameters: "
//    + parameters + ", and content: " + response.Content;

//    //Acquire the actual exception
//    Exception ex;
//    if (response != null && response.ErrorException != null)
//    {
//        ex = response.ErrorException;
//    }
//    else
//    {
//        ex = new Exception(info);
//        info = string.Empty;
//    }

//    //Log the exception and info message
//    LogError(info);
//}

        private void TimeoutCheck(IRestRequest request, IRestResponse response)
        {
            if (response.StatusCode == 0)
            {
                LogRunner.LogMessage($"TIMEOUT {BaseUrl.ToString()}/{request.Resource}", LogLevels.Error);
            }
        }

        public async Task<T> Get<T>(IRestRequest request, string cacheKey = null)
        {
            LogRunner.LogMessage($"{BaseUrl}/{request.Resource}");

            Stopwatch restTimer = new Stopwatch();
            restTimer.Start();
            IRestResponse<T> response = await ExecuteAsync<T>(request);
            restTimer.Stop();
            LogRunner.LogMessage($"REQUEST COMPLETED in {restTimer.ElapsedMilliseconds} ms");

            if (response.Data == null) LogRunner.LogMessage($"the data returned from {BaseUrl}/{request.Resource} is of an invalid format. ", LogLevels.Error);

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
                LogRunner.LogMessage($"{BaseUrl} - {response.ErrorMessage}", LogLevels.Error);
                return default(T);
            }
        }

        public async Task<T> GetFromCache<T>(IRestRequest request, string cacheKey) where T : class
        {
            Stopwatch cacheTimer = new Stopwatch();
            cacheTimer.Start();
            var item = Cache.Get<T>(cacheKey);
            cacheTimer.Stop();

            if (item == null)
            {
                LogRunner.LogMessage($"CACHE MISS in {cacheTimer.ElapsedMilliseconds} ms", LogLevels.Info);
                item = await Get<T>(request, cacheKey);
            }
            else
            {
                LogRunner.LogMessage($"CACHE HIT in {cacheTimer.ElapsedMilliseconds} ms", LogLevels.Info);
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
                LogRunner.LogMessage($"{BaseUrl}/{request.Resource} - {response.ErrorMessage}", LogLevels.Error);
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

        public void FlushCache()
        {
            Cache.Flush();
        }
    }
}
