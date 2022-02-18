using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Tus.Net.Client.Responses;

namespace Tus.Net.Client
{
    public static class TusClient
    {
        #region CreateEndpoint

        /// <summary>
        /// Used to create an endpoint in preparation for file upload
        /// </summary>
        /// <param name="serverEndPoint">The server location to request a new file end point</param>
        /// <param name="fileInfo">The FileInfo object representing the file</param>
        /// <param name="customHeaders">Any custom headers, such as authorization for this end point</param>
        /// <param name="metadata">metadata, such as filename, filetype, will be extracted from fileInfo but can be overwritten</param>
        /// <param name="tusOptions">Options to set the HttpClient, Logging and TUSVersion</param>
        /// <returns>A TusCreatedResponse if successful</returns>
        public static async Task<TusCreatedResponse> CreateEndpointAsync(
            string serverEndPoint,
            FileInfo fileInfo,
            Dictionary<string, string> customHeaders,
            Dictionary<string, string> metadata,
            TusOptions tusOptions = null)
        {
            return await CreateEndpointAsync(serverEndPoint, fileInfo.Length, fileInfo.Name, fileInfo.Extension, customHeaders, metadata, tusOptions);
        }

        /// <summary>
        /// Used to create an endpoint in preparation for file upload
        /// </summary>
        /// <param name="serverEndPoint">The server location to request a new file end point</param>
        /// <param name="fileSize">The size of the file in bytes</param>
        /// <param name="fileName">The name of the file</param>
        /// <param name="fileType">The extension/type of the file</param>
        /// <param name="customHeaders">Any custom headers, such as authorization for this end point</param>
        /// <param name="metadata">metadata, such as filename, filetype, will be extracted from fileInfo but can be overwritten</param>
        /// <param name="tusOptions">Options to set the HttpClient, Logging and TUSVersion</param>
        /// <returns>A TusCreatedResponse if successful</returns>
        public static async Task<TusCreatedResponse> CreateEndpointAsync(
            string serverEndPoint,
            long fileSize,
            string fileName,
            string fileType,
            Dictionary<string, string> customHeaders,
            Dictionary<string, string> metadata,
            TusOptions tusOptions = null)
            
        {
            try
            {
                TusProtocol protocol = new(tusOptions);
                HttpResponseMessage responseMessage = await protocol.CreateAsync(serverEndPoint, fileSize, fileName, fileType, customHeaders, metadata);
                Uri uri = responseMessage.Headers.Location;
                return new(responseMessage, responseMessage.StatusCode == HttpStatusCode.Created, uri?.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        #endregion

        #region Progress

        /// <summary>
        /// Returns the number of bytes that have been uploaded based upon the TUS head request
        /// </summary>
        /// <param name="endPoint">The location of the file to delete</param>
        /// <param name="customHeaders">Any additional headers specific to this implementation, such as Authorization</param>
        /// <param name="tusOptions">Options to set the HttpClient, Logging and TUSVersion</param>
        /// <returns>A Tus Head response containing the current number of bytes uploaded for a specific tus file</returns>
        public static async Task<TusHeadResponse> GetUploadProgressAsync(
            string endPoint, 
            Dictionary<string, string> customHeaders, 
            TusOptions tusOptions = null)
        {
            try
            {
                TusProtocol protocol = new(tusOptions);
                HttpResponseMessage responseMessage = await protocol.HeadAsync(endPoint, customHeaders);
                long? uploadOffset = null;
                if (responseMessage.Headers.Contains(TusHeaders.UploadOffset) &&
                    long.TryParse(responseMessage.Headers.GetValues(TusHeaders.UploadOffset).FirstOrDefault(), out long uploadOffsetHeaderValue))
                {
                    uploadOffset = uploadOffsetHeaderValue;
                }

                return new(responseMessage, uploadOffset);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        #endregion

        #region Delete
        /// <summary>
        /// Deletes a file using the TUS protocol
        /// </summary>
        /// <param name="endPoint">The location of the file to be deleted</param>
        /// <param name="customHeaders">Any additional headers specific to this implementation, such as Authorization</param>
        /// <returns></returns>
        public static async Task<TusDeleteResponse> DeleteFileAsync(
            string endPoint, 
            Dictionary<string, string> customHeaders, 
            TusOptions tusOptions = null)
        {
            TusProtocol protocol = new(tusOptions);
            HttpResponseMessage responseMessage = await protocol.DeleteAsync(endPoint, customHeaders);
            bool deleted = responseMessage.StatusCode is HttpStatusCode.NoContent or HttpStatusCode.NotFound or HttpStatusCode.Gone;
            return new(responseMessage, deleted);
        }
        #endregion
    }
}