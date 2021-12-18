using System.Net.Http;

namespace Tus.Net.Client.Responses
{
    public class TusCreatedResponse : TusClientResponse
    {
        public readonly bool Success;
        public readonly string FileLocation;

        public TusCreatedResponse(HttpResponseMessage responseMessage, bool success, string fileLocation) : base(responseMessage)
        {
            this.Success = success;
            this.FileLocation = fileLocation;
        }
    }
}