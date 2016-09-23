using LizzardmanRestClient.Abstractions;
using System;
using System.Collections.Generic;
using System.Net.Http;
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

        public async Task<HttpResponseMessage> RequestAsync(HttpMethod method, string url, Dictionary<string, string> data, CancellationToken token)
        {
            using (HttpMessageHandler handler = _handler)
            using (var httpclient = new HttpClient(handler))
            {
                var request = new HttpRequestMessage(method, url);

                if (data != null && (method == HttpMethod.Post || method == HttpMethod.Put))
                {
                    request.Content = new FormUrlEncodedContent(data);
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

        public async Task<HttpResponseMessage> GetAsync(string url, CancellationToken token)
        {
            return await RequestAsync(HttpMethod.Get, url, null, token);
        }

        public async Task<HttpResponseMessage> PostAsync(string url, Dictionary<string, string> data, CancellationToken token)
        {
            return await RequestAsync(HttpMethod.Post, url, data, token);
        }

        public async Task<HttpResponseMessage> PutAsync(string url, Dictionary<string, string> data, CancellationToken token)
        {
            return await RequestAsync(HttpMethod.Put, url, data, token);
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
