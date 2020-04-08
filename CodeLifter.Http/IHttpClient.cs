using System;
using System.Threading.Tasks;
using CodeLifter.Logging.Loggers;
using RestSharp;

namespace CodeLifter.Http
{
    public interface IHttpClient
    {
        void AddHeader(string name, string value);
        void RemoveHeader(string name);
        void ClearHeaders();
        void FlushCache();
        void AddLogger(ILogger log);


        Task<T> Get<T>(IRestRequest request, string cacheKey = null);
        Task<T> GetFromCache<T>(IRestRequest request, string cacheKey) where T : class;

        Task<T> Post<T>(IRestRequest request);
        Task<T> Post<T>(IRestRequest request, object data, params string[] includedProperties);

        Task<T> Put<T>(IRestRequest request);
        Task<T> Put<T>(IRestRequest request, object data, params string[] includedProperties);

        ///TODO - Complete Delete Functionality
        //Task<IRestResponse> Delete(IRestRequest request);
    }
}
