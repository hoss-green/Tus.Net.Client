using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Tus.Net.Client
{
    /// <summary>
    /// This is layer on top of the Http Client
    /// </summary>
    internal class TusHttpClient : HttpClient
    {
        private readonly TusOptions _tusOptions;
        // private readonly bool _logRequests;
        // private readonly HttpClient _httpClient;

        public TusHttpClient(TusOptions tusOptions)
        {
            this._tusOptions = tusOptions ?? new TusOptions();
            this._tusOptions.HttpClient = tusOptions?.HttpClient ?? HttpClientFactory.Create();
            
            // if (this._tusOptions.HttpClient == null)
            // {
            //     this._httpClient = HttpClientFactory.Create();
            // }
        }

        public new HttpResponseMessage Send(HttpRequestMessage request)
        {
            if (this._tusOptions.LogRequests)
            {
                Debug.WriteLine(request.ToString());
            }
            return this._tusOptions.HttpClient.Send(request, new CancellationToken());
        }

        public override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (this._tusOptions.LogRequests)
            {
                Debug.WriteLine(request.ToString());
            }
            return this._tusOptions.HttpClient.Send(request, cancellationToken);
        }

        public new async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            if (this._tusOptions.LogRequests)
            {
                Debug.WriteLine(request.ToString());
            }
            return await this._tusOptions.HttpClient.SendAsync(request, new CancellationToken());
        }

        public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (this._tusOptions.LogRequests)
            {
                Debug.WriteLine(request.ToString());
            }
            return this._tusOptions.HttpClient.SendAsync(request, cancellationToken);
        }
    }
}