using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Tus.Net.Client.Responses
{
    public abstract class TusClientResponse
    {
        
        public HttpStatusCode StatusCode { get; }
        public HttpResponseMessage HttpResponseMessage { get; }
        public long? ContentLength { get; }
        public long? UploadLength { get; }
        public long? UploadOffset { get; }
        public string TusResumable { get; }
        public string Location { get; }
        public string TusVersion { get; }
        public string TusExtension { get; }
        public long? TusMaxSize { get; }
        
        public TusClientResponse(HttpResponseMessage responseMessage)
        {
            this.HttpResponseMessage =
                responseMessage ??
                throw new TypeInitializationException(
                    "TusClientResult",
                    new("Tried to initialise TusClientResult with null HttpResponseMessage"));
            
            this.StatusCode = responseMessage.StatusCode;
            this.Location = responseMessage.Headers.Location != null ? responseMessage.Headers.Location.ToString() : null;
            this.ContentLength = responseMessage.Content.Headers.ContentLength;
            this.UploadOffset = GetLongHeaderValue(responseMessage, TusHeaders.UploadOffset);
            this.UploadLength = GetLongHeaderValue(responseMessage, TusHeaders.UploadLength);
            this.TusResumable = GetStringHeaderValue(responseMessage, TusHeaders.TusResumable);
            this.TusVersion = GetStringHeaderValue(responseMessage, TusHeaders.TusVersion);
            this.TusExtension = GetStringHeaderValue(responseMessage, TusHeaders.TusExtension);
            this.TusMaxSize = GetLongHeaderValue(responseMessage, TusHeaders.TusMaxSize);
        }
        
        private static string GetStringHeaderValue(HttpResponseMessage responseMessage, string headerName)
        {
            if (responseMessage.Headers.Contains(headerName)
                && responseMessage.Headers.TryGetValues(headerName, out IEnumerable<string> headerValues))
            {
                return headerValues.FirstOrDefault();
            }

            return null;
        }

        private static long? GetLongHeaderValue(HttpResponseMessage responseMessage, string headerName)
        {
            string result = GetStringHeaderValue(responseMessage, headerName);
            if (long.TryParse(result, out long value))
            {
                return value;
            }

            return null;
        }

        private static int? GetIntegerHeaderValue(HttpResponseMessage responseMessage, string headerName)
        {
            long? result = GetLongHeaderValue(responseMessage, headerName);
            return result.HasValue ? (int)result.Value : null;
        }
    }
}