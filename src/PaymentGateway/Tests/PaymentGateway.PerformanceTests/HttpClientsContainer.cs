using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PaymentGateway.Write.PerformanceTests
{
    public class HttpClientsContainer : IHttpClientsContainer
    {
        private readonly ConcurrentDictionary<string, SafeHttpClient> _cache = new ConcurrentDictionary<string, SafeHttpClient>();

        private volatile bool _disposed;

        public void Dispose()
        {
            foreach (var webApiClient in _cache.Values)
            {
                webApiClient.Dispose();
            }

            _cache.Clear();
            _disposed = true;
        }

        public ISafeHttpClient GetSafeHttpClient(string baseUri, params DelegatingHandler[] messageHandlers)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(typeof(HttpClientsContainer).Name);
            }

            var safeHttpClient = _cache.GetOrAdd(
                baseUri,
                x =>
                {
                    //var delegatingHandlers = new List<DelegatingHandler>
                    //{
                    //    new CustomOriginHeaderHandler()
                    //};
                    //delegatingHandlers.AddRange(messageHandlers);

                    //var httpClient = HttpClientFactory.Create(delegatingHandlers.ToArray());
                    var httpClient = new HttpClient();
                    httpClient.BaseAddress = new Uri(baseUri);

                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    return new SafeHttpClient(httpClient);
                });

            return safeHttpClient;
        }

		
    }


    public interface IHttpClientsContainer : IDisposable
    {
        ISafeHttpClient GetSafeHttpClient(string baseUri, params DelegatingHandler[] delegatingHandlers);
    }


    public interface ISafeHttpClient
    {
        string BaseUriString { get; }
        Task<HttpResponseMessage> PostAsync(string requestUri, StringContent content);
        Task<HttpResponseMessage> GetAsync(string requestUri);
    }


    public class SafeHttpClient : ISafeHttpClient, IDisposable
	{
		private readonly HttpClient _httpClient;

		internal SafeHttpClient(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		public string BaseUriString => _httpClient.BaseAddress.ToString();

	    public Task<HttpResponseMessage> PostAsync(string requestUri, StringContent content)
	    {
	        return _httpClient.PostAsync(requestUri, content);
	    }

	    public Task<HttpResponseMessage> GetAsync(string requestUri)
	    {
	        return _httpClient.GetAsync(requestUri);
	    }

	    /// <summary>
		/// Call this method only when you're done with http calls. Do not dispose it on each call.
		/// </summary>
		public void Dispose()
		{
			_httpClient?.Dispose();
		}

	}
}