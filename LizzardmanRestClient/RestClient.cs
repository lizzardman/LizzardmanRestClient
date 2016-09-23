using LizzardmanRestClient.Abstractions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LizzardmanRestClient
{
    public class RestClient : IRestClient
    {
        readonly HttpMessageHandler _handler;

        public RestClient()
        {

        }

        public RestClient(HttpMessageHandler handler)
        {
            _handler = handler;
        }


        public async Task<HttpResponseMessage> RequestAsync(HttpMethod method, string url, HttpContent content, CancellationToken token)
        {
            using (HttpMessageHandler handler = _handler)
            using (var httpclient = new HttpClient(handler))
            {
                var request = new HttpRequestMessage(method, url);

                if (content != null && (method == HttpMethod.Post || method == HttpMethod.Put))
                {
                    request.Content = content;
                }

                HttpResponseMessage response = null;

                try
                {
                    response = await httpclient.SendAsync(request, token);
                    token.ThrowIfCancellationRequested();
                }
                catch (Exception ex)
                {
                    throw new RestClientException()
                    {
                        InnerException = ex,
                        Message = ex.Message,
                        StatusCode = 0,
                        IsNetwork = true
                    };
                }

                if (response.IsSuccessStatusCode)
                {
                    return response;
                }
                else
                {
                    token.ThrowIfCancellationRequested();
                    throw new RestClientException()
                    {
                        Message = await response.Content.ReadAsStringAsync(),
                        StatusCode = response.StatusCode,
                        IsNetwork = false
                    };
                }
            }
        }

        public async Task<HttpResponseMessage> RequestJsonAsync(HttpMethod method, string url, object data, CancellationToken token)
        {
            var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            return await RequestAsync(method, url, content, token);
        }

        public async Task<HttpResponseMessage> RequestFormAsync(HttpMethod method, string url, Dictionary<string, string> data, CancellationToken token)
        {
            var content = new FormUrlEncodedContent(data);
            return await RequestAsync(method, url, content, token);
        }

        public async Task<HttpResponseMessage> GetAsync(string url, CancellationToken token)
        {
            return await RequestFormAsync(HttpMethod.Get, url, null, token);
        }

        public async Task<HttpResponseMessage> PostAsync(string url, Dictionary<string, string> data, CancellationToken token)
        {
            return await RequestFormAsync(HttpMethod.Post, url, data, token);
        }

        public async Task<HttpResponseMessage> PutAsync(string url, Dictionary<string, string> data, CancellationToken token)
        {
            return await RequestFormAsync(HttpMethod.Put, url, data, token);
        }

        public async Task<HttpResponseMessage> PostJsonAsync(string url, object data, CancellationToken token)
        {
            return await RequestJsonAsync(HttpMethod.Post, url, data, token);
        }

        public async Task<HttpResponseMessage> PutJsonAsync(string url, object data, CancellationToken token)
        {
            return await RequestJsonAsync(HttpMethod.Put, url, data, token);
        }

        public async Task<T> GetAsync<T>(string url, CancellationToken token)
        {
            var response = await GetAsync(url, token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            var stringcontent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            T result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(stringcontent);
            return result;
        }
    }
}
