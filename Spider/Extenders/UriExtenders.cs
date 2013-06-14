using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Spider
{
    public static class UriExtenders
    {
        public static async Task<HttpResponseMessage> GetResponse(this Uri uri)
        {
            const string USERAGENT =
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_2) AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1309.0 Safari/537.17";

            var client = new HttpClient();

            var message = new HttpRequestMessage(HttpMethod.Get, uri);

            message.Headers.Add("User-Agent", USERAGENT);

            return await client.SendAsync(message);
        }
    }
}
