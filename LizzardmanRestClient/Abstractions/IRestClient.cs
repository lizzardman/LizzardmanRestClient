using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LizzardmanRestClient.Abstractions
{
    public interface IRestClient
    {
        Task<HttpResponseMessage> RequestAsync(HttpMethod method, string url, Dictionary<string, string> data, CancellationToken token);
        Task<HttpResponseMessage> GetAsync(string url, CancellationToken token);
        Task<HttpResponseMessage> PostAsync(string url, Dictionary<string, string> data, CancellationToken token);
        Task<HttpResponseMessage> PutAsync(string url, Dictionary<string, string> data, CancellationToken token);
        Task<T> GetAsync<T>(string url, CancellationToken token);
    }
}
