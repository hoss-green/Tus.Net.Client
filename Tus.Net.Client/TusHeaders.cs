using System;

namespace Tus.Net.Client
{
    public static class TusHeaders
    {
        public const string TusResumable = "Tus-Resumable";
        public const string TusVersion = "Tus-Version";
        public const string TusExtension = "Tus-Extension";
        public const string TusMaxSize = "Tus-Max-Size";
        public const string ContentType = "Content-Type";
        public const string ContentLength = "Content-Length";
        public const string UploadLength = "Upload-Length";
        public const string UploadOffset = "Upload-Offset";
        public const string UploadChecksum = "Upload-Checksum";
        public const string UploadMetadata = "Upload-Metadata";
        
        [Obsolete("Will be removed in the next version")]
        public const string Authorization = "Authorization";
        
        [Obsolete("Will be removed in the next version")]
        public const string Host = "Host";
    }
}