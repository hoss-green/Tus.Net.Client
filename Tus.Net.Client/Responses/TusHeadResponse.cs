using System.Net.Http;

namespace Tus.Net.Client.Responses
{
    public class TusHeadResponse : TusClientResponse
    {
        public readonly long? CurrentUploadBytePosition;
        public TusHeadResponse(HttpResponseMessage responseMessage, long? currentUploadBytePosition) : base(responseMessage)
        {
            this.CurrentUploadBytePosition = currentUploadBytePosition;
            
        }
    }
}