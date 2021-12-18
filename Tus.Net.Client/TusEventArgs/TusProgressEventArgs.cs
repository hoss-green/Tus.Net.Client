using System.Net;
using System.Net.Http;

namespace Tus.Net.Client.TusEventArgs
{
    public class TusProgressEventArgs : TusEventArgs
    {
        public readonly long UploadedBytes;
        public readonly long TotalBytes;
        public readonly float Percentage;

        public TusProgressEventArgs(
            TusFile tusFile, 
            string message, 
            HttpResponseMessage httpResponseMessage, 
            HttpStatusCode? statusCode, 
            long uploadedBytes, 
            long totalBytes) 
            : base(tusFile, message, httpResponseMessage, statusCode)
        {
            this.UploadedBytes = uploadedBytes;
            this.TotalBytes = totalBytes;
            this.Percentage = (float)this.UploadedBytes / (float)this.TotalBytes;
        }
    }
}