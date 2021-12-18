using System.Net;
using System.Net.Http;

namespace Tus.Net.Client.TusEventArgs
{
    public class TusEventArgs
    {
        public readonly TusFile TusFile; 
        public readonly HttpStatusCode? StatusCode;
        public readonly string Message;
        public readonly HttpResponseMessage HttpResponseMessage;

        public TusEventArgs(TusFile tusFile, string message, HttpResponseMessage httpResponseMessage, HttpStatusCode? statusCode)
        {
            this.Message = message;
            this.HttpResponseMessage = httpResponseMessage;
            this.StatusCode = statusCode;
        }
    }
}