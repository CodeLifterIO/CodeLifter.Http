# CodeLifter.Http

```
using System;
using System.Threading.Tasks;

namespace CodeLifter.Http.Demo
{
    class Ip
    {
        public string ip { get; set; }
    }


    class Program
    {
        static void Main(string[] args)
        {
            Task t = UseHttpGet();
            t.Wait();
        }

        private static async Task UseHttpGet()
        {
            string BaseURI = "https://api.ipify.org?format=json";
            HttpClient Client = new HttpClient(BaseURI, true);

            Client.FlushCache();

            HttpRequest request = new HttpRequest("");
            Ip cacheThisdResult = await Client.GetFromCache<Ip>(request, "this_is_a_test_key");
            Ip skippedCache = await Client.Get<Ip>(request);
            Ip resultFromCache = await Client.GetFromCache<Ip>(request, "this_is_a_test_key");
        }
    }
}

```