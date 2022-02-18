using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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

        internal TusHttpClient(TusOptions tusOptions)
        {
            this._tusOptions = tusOptions;
        }

        [Obsolete("Will remove all non-async methods in next major version")]
        public new HttpResponseMessage Send(HttpRequestMessage request)
        {
            if (this._tusOptions.LogRequests)
            {
                Debug.WriteLine(request.ToString());
            }
            return this._tusOptions.HttpClient.Send(request, new CancellationToken());
        }

        [Obsolete("Will remove all non-async methods in next major version")]
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