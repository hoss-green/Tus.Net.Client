using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Tus.Net.Client
{
    public sealed class TusProtocol
    {
        private readonly bool _logRequests;
        private readonly string _tusVersion;

        /// <summary>
        /// Instantiates this TUS connector
        /// </summary>
        /// <param name="tusVersion">The version of the protocol, defaults to 1.0.0</param>
        /// <param name="logRequests">Turn on logging of http requests</param>
        public TusProtocol(string tusVersion = "1.0.0", bool logRequests = false)
        {
            this._tusVersion = tusVersion;
            this._logRequests = logRequests;
        }

        /// <summary>
        /// Used to ascertain how much is left of the upload, and the start point
        /// </summary>
        /// <param name="url">The Url of the upload</param>
        /// <param name="customHeaders">Any custom headers, such as authentication/authorisation that need to be added for this particular API</param>
        /// <returns>Either 200 OK or 404 Not Found, 410 Gone or 403 Forbidden depending on circumstance</returns>
        public async Task<HttpResponseMessage> HeadAsync(string url, Dictionary<string, string> customHeaders = null)
        {
            try
            {
                TusHttpClient client = new(this._logRequests);
                HttpRequestMessage requestMessage = BuildRequest(customHeaders, HttpMethod.Head, url);
                requestMessage.Headers.Add(TusHeaders.TusResumable, this._tusVersion);
                HttpResponseMessage responseMessage = await client.SendAsync(requestMessage);
                return responseMessage;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        /// <summary>
        /// Create an upload Url and file location for patching requests to, not supported by all protocols.
        /// </summary>
        /// <param name="url">The Url of the upload</param>
        /// <param name="file">The FileInfo of the file to be uploaded</param>
        /// <param name="customHeaders">Any custom headers, such as authentication/authorisation that need to be added for this particular API</param>
        /// <param name="metadata"></param>
        /// <returns>201 successful if created</returns>
        public async Task<HttpResponseMessage> CreateAsync(
            string url,
            FileInfo file,
            Dictionary<string, string> customHeaders = null,
            Dictionary<string, string> metadata = null)
        {
            return await CreateAsync(url, file.Length, file.Name, file.Extension, customHeaders, metadata);
        }

        /// <summary>
        /// Create an upload Url and file location for patching requests to, not supported by all protocols.
        /// </summary>
        /// <param name="url">The Url of the upload</param>
        /// <param name="fileLength">The size of the file in bytes</param>
        /// <param name="fileName">The name of the file</param>
        /// <param name="fileExtension">The extension of the given file (.mov, .mp4)</param>
        /// <param name="customHeaders">Any custom headers, such as authentication/authorisation that need to be added for this particular API</param>
        /// <param name="metadata"></param>
        /// <returns>201 successful if created</returns>
        public async Task<HttpResponseMessage> CreateAsync(
            string url,
            long fileLength,
            string fileName,
            string fileExtension,
            Dictionary<string, string> customHeaders = null,
            Dictionary<string, string> metadata = null)
        {
            metadata ??= new();
            if (!metadata.ContainsKey("filename"))
            {
                metadata["filename"] = fileName;
            }

            if (!metadata.ContainsKey("filetype"))
            {
                metadata["filetype"] = fileExtension;
            }
            return await CreateAsync(url, fileLength, customHeaders, metadata);
        }
        
        /// <summary>
        /// Create an upload Url and file location for patching requests to, not supported by all protocols.
        /// </summary>
        /// <param name="url">The Url of the upload</param>
        /// <param name="fileLength">The size of the file in bytes</param>
        /// <param name="customHeaders">Any custom headers, such as authentication/authorisation that need to be added for this particular API</param>
        /// <param name="metadata"></param>
        /// <returns>201 successful if created</returns>
        public async Task<HttpResponseMessage> CreateAsync(
            string url,
            long fileLength,
            Dictionary<string, string> customHeaders = null,
            Dictionary<string, string> metadata = null)
        {
            TusHttpClient client = new(this._logRequests);
            HttpRequestMessage requestMessage = BuildRequest(customHeaders, HttpMethod.Post, url);

            requestMessage.Content = new StringContent("");
            requestMessage.Content.Headers.Add(TusHeaders.ContentLength, "0");
            requestMessage.Headers.Add(TusHeaders.UploadLength, fileLength.ToString());

            metadata ??= new();

            List<string> metadataStrings = new();
            foreach (KeyValuePair<string, string> meta in metadata)
            {
                string k = meta.Key.Replace(" ", "").Replace(",", "");
                string v = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(meta.Value));
                metadataStrings.Add($"{k} {v}");
            }

            requestMessage.Headers.Add(TusHeaders.UploadMetadata, string.Join(", ", metadataStrings.ToArray()).Trim());
            requestMessage.Headers.Add(TusHeaders.TusResumable, this._tusVersion);

            HttpResponseMessage responseMessage = await client.SendAsync(requestMessage);
            return responseMessage;
        }


        /// <summary>
        /// Used to patch part of a file that has been created or stopped during an upload
        /// </summary>
        /// <param name="url">The Url of the file to patch</param>
        /// <param name="file">The FileInfo of the file to be uploaded</param>
        /// <param name="offset">Where to start the upload from</param>
        /// <param name="chunkSize">The maximum size of this upload patch, will be automatically reduced for the last patch</param>
        /// <param name="customHeaders">Any custom headers, such as authentication/authorisation that need to be added for this particular API</param>
        /// <returns>204 no content if successful, 409 if offsets mismatch, 404 if resource doesn't exist</returns>
        public async Task<HttpResponseMessage> PatchAsync(string url, FileInfo file, long offset, int chunkSize,
            Dictionary<string, string> customHeaders = null)
        {
            await using FileStream fs = new(file.FullName, FileMode.Open, FileAccess.Read);
            return await PatchAsync(url, fs, offset, chunkSize, customHeaders);
        }

        
        /// <summary>
        /// Used to patch part of a file that has been created or stopped during an upload
        /// </summary>
        /// <param name="url">The Url of the file to patch</param>
        /// <param name="fs">The filestream to patch to this upload</param>
        /// <param name="offset">Where to start the upload from</param>
        /// <param name="chunkSize">The maximum size of this upload patch, will be automatically reduced for the last patch</param>
        /// <param name="customHeaders">Any custom headers, such as authentication/authorisation that need to be added for this particular API</param>
        /// <returns>204 no content if successful, 409 if offsets mismatch, 404 if resource doesn't exist</returns>
        public async Task<HttpResponseMessage> PatchAsync(string url, Stream fs, long offset, int chunkSize, Dictionary<string, string> customHeaders = null)
        {
            try
            {
                TusHttpClient client = new(this._logRequests);
                System.Security.Cryptography.SHA1 sha = new System.Security.Cryptography.SHA1Managed();

                fs.Seek(offset, SeekOrigin.Begin);
                byte[] buffer = new byte[chunkSize];
                int bytesRead = await fs.ReadAsync(buffer, 0, chunkSize);

                // resize the buffer to match what is left (should always be less than the buffer size)
                Array.Resize(ref buffer, bytesRead);

                byte[] sha1Hash = sha.ComputeHash(buffer);
                HttpRequestMessage requestMessage = BuildRequest(customHeaders, HttpMethod.Patch, url);
                requestMessage.Headers.Add(TusHeaders.TusResumable, this._tusVersion);
                requestMessage.Headers.Add(TusHeaders.UploadOffset, $"{offset}");
                requestMessage.Headers.Add(TusHeaders.UploadChecksum, "sha1 " + Convert.ToBase64String(sha1Hash));
                requestMessage.Content = new ByteArrayContent(buffer);
                requestMessage.Content.Headers.Add("Content-Length", bytesRead.ToString());
                requestMessage.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/offset+octet-stream");
                return await client.SendAsync(requestMessage);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        

        /// <summary>
        /// An OPTIONS request MAY be used to gather information about the Server’s current configuration.
        /// </summary>
        /// <param name="url">The server URL for where to check the Options of the server</param>
        /// <param name="customHeaders">Any custom headers, such as authentication/authorisation that need to be added for this particular API</param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> OptionsAsync(string url, Dictionary<string, string> customHeaders = null)
        {
            TusHttpClient client = new(this._logRequests);
            HttpRequestMessage requestMessage = BuildRequest(customHeaders, HttpMethod.Options, url);
            return await client.SendAsync(requestMessage);
        }

        /// <summary>
        /// This request for an existing upload the Server SHOULD free associated resources 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="customHeaders"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> DeleteAsync(string url, Dictionary<string, string> customHeaders = null)
        {
            TusHttpClient client = new(this._logRequests);
            HttpRequestMessage requestMessage = BuildRequest(customHeaders, HttpMethod.Delete, url);
            requestMessage.Headers.Add(TusHeaders.TusResumable, this._tusVersion);

            return await client.SendAsync(requestMessage);
        }
        
        
        /// <summary>
        /// Creates a HttpRequestMessage and adds the custom headers
        /// </summary>
        /// <param name="customHeaders"></param>
        /// <param name="method"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        static HttpRequestMessage BuildRequest(Dictionary<string, string> customHeaders, HttpMethod method, string url)
        {
            HttpRequestMessage requestMessage = new(method, url);
            if (customHeaders == null || !customHeaders.Any())
            {
                return requestMessage;
            }
            
            foreach (KeyValuePair<string, string> customHeader in customHeaders)
            {
                requestMessage.Headers.Add(customHeader.Key, customHeader.Value);
            }
            return requestMessage;
        }
    }
}