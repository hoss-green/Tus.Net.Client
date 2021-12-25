using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Tus.Net.Client.TusEventArgs;

namespace Tus.Net.Client
{
    /// <summary>
    /// Responsible for managing tus file uploads, for creating and uploading tus files 
    /// </summary>
    public class TusFile : IDisposable
    {
        /// <summary>
        /// The stream of the file
        /// </summary>
        public FileStream FileStream { get; private set; }
        
        /// <summary>
        /// The name of this file
        /// </summary>
        public string FileName { get; private set; }
        
        /// <summary>
        /// The size of the file, in bytes
        /// </summary>
        public long FileSize { get; private set; }
        
        /// <summary>
        /// The type of the file to be uploaded
        /// </summary>
        public string FileType { get; private set; }
        
        /// <summary>
        /// Where this file will be uploaded to
        /// </summary>
        public string EndPoint { get; private set; }
        
        /// <summary>
        /// Metadata such as filename and filetype
        /// </summary>
        public Dictionary<string, string> MetaData { get; private set; }
        
        /// <summary>
        /// An error that occurs during file uploading
        /// </summary>
        public EventHandler<TusErrorEventArgs> OnError { get; set; }
        
        /// <summary>
        /// An event which occurs when part of the file is successfully patched to the server
        /// </summary>
        public EventHandler<TusProgressEventArgs> OnProgress { get; set; }
        
        /// <summary>
        /// An event which occurs when the file has completed uploading
        /// </summary>
        public EventHandler<TusSuccessEventArgs> OnSuccess { get; set; }

        /// <param name="fileName">The exact name/path of the file</param>
        /// <param name="endPoint">The location where you want to upload the file</param>
        /// <param name="metaData">filename, filetype and any other additional metadata</param>
        /// <param name="onError">An event which occurs when there is an error with the upload</param>
        /// <param name="onProgress">An event which occurs when part of the file is successfully patched to the server</param>
        /// <param name="onSuccess">An event which occurs when the file has completed uploading</param>
        /// <returns></returns>
        public TusFile(
            string fileName,
            string endPoint,
            Dictionary<string, string> metaData,
            EventHandler<TusErrorEventArgs> onError,
            EventHandler<TusProgressEventArgs> onProgress,
            EventHandler<TusSuccessEventArgs> onSuccess)
        {
            FileInfo fileInfo = new(fileName);
            FileStream fs = new(fileInfo.FullName, FileMode.Open, FileAccess.Read);
            Create(fs, fs.Name, fs.Length, fileInfo.Extension, endPoint, metaData, onError, onProgress, onSuccess);
        }

        /// <param name="fileInfo">The FileInfo object representing the file to be uploaded</param>
        /// <param name="endPoint">The location where you want to upload the file</param>
        /// <param name="metaData">filename, filetype and any other additional metadata</param>
        /// <param name="onError">An event which occurs when there is an error with the upload</param>
        /// <param name="onProgress">An event which occurs when part of the file is successfully patched to the server</param>
        /// <param name="onSuccess">An event which occurs when the file has completed uploading</param>
        public TusFile(
            FileInfo fileInfo,
            string endPoint,
            Dictionary<string, string> metaData,
            EventHandler<TusErrorEventArgs> onError,
            EventHandler<TusProgressEventArgs> onProgress,
            EventHandler<TusSuccessEventArgs> onSuccess)
        {
            FileStream fs = new(fileInfo.FullName, FileMode.Open, FileAccess.Read);
            Create(fs, fs.Name, fs.Length, fileInfo.Extension, endPoint, metaData, onError, onProgress, onSuccess);
        }

        /// <param name="fileStream">The stream from the file</param>
        /// <param name="fileName">The name of the file to be uploaded</param>
        /// <param name="fileType">The type of the file to be uploaded</param>
        /// <param name="endPoint">The location where you want to upload the file</param>
        /// <param name="metaData">filename, filetype and any other additional metadata</param>
        /// <param name="onError">An event which occurs when there is an error with the upload</param>
        /// <param name="onProgress">An event which occurs when part of the file is successfully patched to the server</param>
        /// <param name="onSuccess">An event which occurs when the file has completed uploading</param>
        /// <param name="fileSize"></param>
        public TusFile(
            FileStream fileStream,
            string fileName,
            long fileSize,
            string fileType,
            string endPoint,
            Dictionary<string, string> metaData,
            EventHandler<TusErrorEventArgs> onError,
            EventHandler<TusProgressEventArgs> onProgress,
            EventHandler<TusSuccessEventArgs> onSuccess)
        {
            Create(fileStream, fileName, fileSize, fileType, endPoint, metaData, onError, onProgress, onSuccess);
        }
        
        
        private void Create(
            FileStream fileStream,
            string fileName,
            long fileSize,
            string fileType,
            string endPoint,
            Dictionary<string, string> metaData,
            EventHandler<TusErrorEventArgs> onError,
            EventHandler<TusProgressEventArgs> onProgress,
            EventHandler<TusSuccessEventArgs> onSuccess)
        {
            this.FileStream = fileStream;
            this.EndPoint = endPoint;
            this.MetaData = metaData;
            this.OnError = onError;
            this.OnProgress = onProgress;
            this.OnSuccess = onSuccess;
            this.FileSize = fileSize;
            this.FileType = fileType;
            this.FileName = fileName;
        }


        /// <summary>
        /// The main function of this file, starts and resumes files based upon the fileName/endPoint given
        /// </summary>
        /// <param name="customHttpHeaders">Any additional headers, authorization etc.</param>
        /// <returns>true if successful</returns>
        public async Task<bool> UploadAsync(Dictionary<string, string> customHttpHeaders = null)
        {
            return await UploadAsync(customHttpHeaders);
        }

        /// <summary>
        /// The main function of this file, starts and resumes files based upon the fileName/endPoint given
        /// </summary>
        /// <param name="customHttpHeaders">Any additional headers, authorization etc.</param>
        /// <returns>true if successful</returns>
        [Obsolete("Will be removed in next 1.x.0 update, please use UploadAsync() instead")]
        public async Task<bool> Upload(Dictionary<string, string> customHttpHeaders = null)
        {
            HttpResponseMessage responseMessage = null;
            try
            {
                TusProtocol protocol = new();
                int chunkSize = (int)Math.Ceiling(5 * 1024.0 * 1024.0); //5mb
                responseMessage = await protocol.HeadAsync(this.EndPoint, customHttpHeaders);
                if (!responseMessage.IsSuccessStatusCode || !responseMessage.Headers.Contains(TusHeaders.UploadOffset))
                {
                    this.OnError?.Invoke(this, 
                        new(
                            this,
                            responseMessage.ReasonPhrase, 
                            responseMessage,
                            responseMessage.StatusCode,
                            null));
                    return false;
                }

                long uploadOffset = GetUploadOffset(responseMessage);
                //this is where we resume uploading from
                this.OnProgress?.Invoke(
                    this,
                    new(this, "Upload in progress", responseMessage, responseMessage?.StatusCode, uploadOffset, this.FileSize));

                while (uploadOffset < this.FileSize)
                {
                    HttpResponseMessage patchResponse = await protocol.PatchAsync(this.EndPoint, this.FileStream, uploadOffset, chunkSize, customHttpHeaders);
                    switch (patchResponse.StatusCode)
                    {
                        case HttpStatusCode.Conflict:
                            throw new("Tried to upload the file using the wrong start point, please try again later");
                        case HttpStatusCode.UnsupportedMediaType:
                            throw new("Media type not supported for file: " + this.FileName);
                        case HttpStatusCode.NotFound:
                            throw new($"Upload link not found at: {this.EndPoint}");
                        default:
                            if (!patchResponse.IsSuccessStatusCode)
                            {
                                throw new($"Upload error: {patchResponse.StatusCode} | Reason: {patchResponse.ReasonPhrase}");
                            }

                            break;
                    }

                    uploadOffset = GetUploadOffset(patchResponse);
                    this.OnProgress?.Invoke(
                        this,
                        new(this, "Upload in progress", responseMessage, responseMessage?.StatusCode, uploadOffset, this.FileSize));
                }

                this.OnSuccess?.Invoke(
                    this,
                    new(this, "Upload 100% Complete", responseMessage, responseMessage?.StatusCode));
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                this.OnError?.Invoke(
                    this,
                    new(this, "Failed to finish Upload", responseMessage, responseMessage?.StatusCode, e));
                return false;
            }
        }

        /// <summary>
        /// Extracts the UploadOffset from the headers
        /// </summary>
        private long GetUploadOffset(HttpResponseMessage responseMessage)
        {
            if (responseMessage.Headers.TryGetValues(TusHeaders.UploadOffset, out IEnumerable<string> values))
            {
                if (long.TryParse(values.FirstOrDefault(), out long uploadOffset))
                {
                    return uploadOffset;
                }
            }

            throw new("Cannot parse upload offset");
        }

        /// <summary>
        /// Extracts the first value from a header
        /// </summary>
        private string GetFirstHeaderValue(HttpResponseMessage responseMessage, string tusHeader)
        {
            return responseMessage.Headers.TryGetValues(tusHeader, out IEnumerable<string> values) ? values.FirstOrDefault() : null;
        }

        public void Dispose()
        {
            this.FileStream?.Dispose();
        }
    }
}