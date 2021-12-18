using System.Net;
using System.Net.Http;

namespace Tus.Net.Client.TusEventArgs
{
    public class TusSuccessEventArgs : TusEventArgs
    {
        public TusSuccessEventArgs(TusFile tusFile, string message, HttpResponseMessage httpResponseMessage, HttpStatusCode? statusCode) 
            : base(tusFile, message, httpResponseMessage, statusCode)
        {
        }
    }
}