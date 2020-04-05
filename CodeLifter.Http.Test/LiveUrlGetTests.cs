using System;
using Xunit;

namespace CodeLifter.Http.Test
{
    class Ip
    {
        public string ip { get; set; }
    }


    public class LiveUrlGetTests
    {
        [Fact]
        public async void TestAValidUrlRetrunsJsonAsAType()
        {
            string BaseURI = "https://api.ipify.org?format=json";
            RestApiClient Client = new RestApiClient(BaseURI, true, true);

            HttpRequest request = new HttpRequest("");
            Ip result = await Client.Get<Ip>(request);

            Assert.NotNull(result);
            Assert.IsType<Ip>(result);
        }

        [Fact]
        public async void TestAnInvalidUrlReturnsNull()
        {
            string BaseURI = "https://this.is.not.a.real.url.com";
            RestApiClient Client = new RestApiClient(BaseURI, true, true);
            Client.Timeout = 5000;

            HttpRequest request = new HttpRequest("");
            Ip result = await Client.Get<Ip>(request);

            Assert.Null(result);
        }
    }
}
