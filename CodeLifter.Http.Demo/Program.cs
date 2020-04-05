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
            RestApiClient Client = new RestApiClient(BaseURI, true, true);

            HttpRequest request = new HttpRequest("");
            Ip result = await Client.Get<Ip>(request);
        }
    }
}
