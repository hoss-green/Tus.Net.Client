using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Tus.Net.Client
{
    /// <summary>
    /// This is layer on top of the Http Client that's sole purpose is to be able to check if your 
    /// </summary>
    internal class TusHttpClient : HttpClient
    {
        private readonly bool _logRequests;

        public TusHttpClient(bool logRequests)
        {
            this._logRequests = logRequests;
        }

        public new HttpResponseMessage Send(HttpRequestMessage request)
        {
            if (this._logRequests)
            {
                Debug.WriteLine(request.ToString());
            }
            return base.Send(request, new CancellationToken());
        }

        public override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (this._logRequests)
            {
                Debug.WriteLine(request.ToString());
            }
            return base.Send(request, cancellationToken);
        }

        public new async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            if (this._logRequests)
            {
                Debug.WriteLine(request.ToString());
            }
            return await base.SendAsync(request, new CancellationToken());
        }

        public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (this._logRequests)
            {
                Debug.WriteLine(request.ToString());
            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}