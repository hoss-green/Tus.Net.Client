using System.Net.Http;

namespace Tus.Net.Client
{
    public class TusOptions
    {
        /// <summary>
        /// The version of the protocol, defaults to 1.0.0
        /// </summary>
        public string TusVersion { get; set; } = "1.0.0";
        
        /// <summary>
        /// Turn on logging of http requests
        /// </summary>
        public bool LogRequests { get; set; } = false;
        
        /// <summary>
        /// Optional HttpClient to use for sending requests
        /// </summary>
        public HttpClient HttpClient { get; set; } = null;
    }
}