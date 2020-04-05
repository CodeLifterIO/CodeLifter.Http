namespace CodeLifter.Http
{
    public class HttpRequest : RestSharp.RestRequest
    {
        public HttpRequest(string baseUri) : base(baseUri)
        {
        }
    }
}
