using System;
using System.Net;
using System.Net.Http;

namespace Tus.Net.Client.TusEventArgs
{
    public class TusErrorEventArgs : TusEventArgs
    {
        public readonly Exception Exception;

        public TusErrorEventArgs(TusFile tusFile, string message,  HttpResponseMessage httpResponseMessage, HttpStatusCode? statusCode, Exception exception) 
            : base(tusFile, message, httpResponseMessage,  statusCode)
        {
            this.Exception = exception;
        }
    }
}