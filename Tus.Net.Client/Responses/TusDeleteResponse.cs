using System.Net.Http;

namespace Tus.Net.Client.Responses
{
    public class TusDeleteResponse : TusClientResponse
    {
        public readonly bool Deleted;

        public TusDeleteResponse(HttpResponseMessage responseMessage, bool deleted)
            : base(responseMessage)
        {
            this.Deleted = deleted;
        }
    }
}