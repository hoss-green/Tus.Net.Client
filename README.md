[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/O5O8AGNLW)

[![](https://www.paypalobjects.com/en_US/i/btn/btn_donate_LG.gif)](https://www.paypal.com/donate/?hosted_button_id=D6P7EB584UGLC)

# Tus.Net.Client
A lightweight header for connecting to a server that supports the TUS protocol. This API has two layers, the first layer (included from version 0.0.1) are built using HttpClient and HttpRequestMessage/Response setup.

## Packages
Current Nuget packages are available here: https://www.nuget.org/packages/Tus.Net.Client/

## Usage
### Part 1/4: TusProtocol
You can use the TusProtocol class to access the following requests directly:

#### Core Functionality
* HEAD: Used to find how much of a file has been uploaded, it returns a header containing the number of bytes that have been uploaded
* PATCH: Used to upload a piece of a file, sequentially, by stating the start byte (used by getting the response from "HEAD") and the number of bytes to upload with this patch. It automatically adjusts for the last patch to the server.
* OPTIONS: Used to find out information about the server, for example, the optional methods that have been implemented and the TUS protocol version supported by the server.

#### Extension Functionality
* CREATION: Used to initiate an upload to a server, by sending the total amount of bytes that will be uploaded, and the name of a file. It will return a HttpResponseMessage containing the link of the upload as a header.
* TERMINATION: Used to delete a partial or full upload. It will return a HttpRequestMessage with a 204 Status Code if successful.


### Part 2/4: TusFile
The TusFile is the class that handles uploading of Files to a server using the TUS protocol

#### There is a constructor, which is used to create the Tus File, and there is one method:
* UploadAsync: Used to be start or resume an ongoing upload, add any server specific headers here.

#### There are three events:
``` C#
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
```


### Part 3/4: TusClient
You can use the wrapped TusClient to have a higher level of abstraction to a TUS server.
Supports the following methods:
``` C#
    //Used to create a URL endpoint to upload to
    CreateEndpointAsync(
            string serverEndPoint,
            FileInfo fileInfo,
            Dictionary<string, string> customHeaders,
            Dictionary<string, string> metadata)....
    
    //Gets the upload offset of a particular endpoint
    GetUploadProgressAsync(string endPoint, Dictionary<string, string> customHeaders)

    //Deletes a file using an endpoint (if supported)
    DeleteFileAsync(string endPoint, Dictionary<string, string> customHeaders)

```

### Part 4/4: TusOptions [New in v2.0.0]
You can now pass in your own HttpClient using the TusOptions, big thank you to https://github.com/drewswiredin for the contributions.
``` C#
    //Used to set options when configuring the API
    TusOptions tusOptions = new();
    tusOptions.TusVersion = "1.0.0" //Set the tus version here
    tusOptions.Logging = true //turn on logging
    tusOptions.HttpClient = HttpClient   

    //usage:
    TusProtocol tusProtocol = new(tusOptions);

```


# Info about TUS
You can find out more about the TUS protocol here: https://tus.io/
You can see the protocol here: https://tus.io/protocols/resumable-upload.html

# Tip!
Always happy to receive donations to help me support my work or buy pizza.

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/O5O8AGNLW)

[![](https://www.paypalobjects.com/en_US/i/btn/btn_donate_LG.gif)](https://www.paypal.com/donate/?hosted_button_id=D6P7EB584UGLC)

BSC: 0x44ed8E0c45b82d12cAF02f73294fa9AfED48eD80

ETH: 0x44ed8E0c45b82d12cAF02f73294fa9AfED48eD80

BTC: 1Jwk86Rx42aJqKymBaCwtM8nqBinxGqMpX

## License
MIT: Copyright (c) 2021 Hoss

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
