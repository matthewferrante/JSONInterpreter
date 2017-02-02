using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PTMS.Core.Utilities {
    public class Fetch {
        #region Initialiation
        /// <summary>
        /// Initializes a new instance of the <see cref="Fetch"/> class.
        /// </summary>
        public Fetch() {
            Headers = new WebHeaderCollection();
            Retries = 5;
            Timeout = 60000;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Get the headers.
        /// </summary>
        /// <value>The headers.</value>
        public WebHeaderCollection Headers { get; private set; }

        /// <summary>
        /// Gets the response.
        /// </summary>
        public HttpWebResponse Response { get; private set; }

        /// <summary>
        /// Get/Set the Authorization Credentials
        /// </summary>
        public NetworkCredential Credential { get; set; }

        /// <summary>
        /// Gets the response data.
        /// </summary>
        public byte[] ResponseData { get; private set; }

        /// <summary>
        /// Gets or sets the number of retries.
        /// </summary>
        /// <value>The number of retries.</value>
        public int Retries { get; set; }

        /// <summary>
        /// Gets or sets the timeout.
        /// </summary>
        /// <value>The timeout.</value>
        public int Timeout { get; set; }

        /// <summary>
        /// Gets or sets the retry sleep in milliseconds.
        /// </summary>
        /// <value>The retry sleep.</value>
        public int RetrySleep { get; set; }

        /// <summary>
        /// Returns successfulness of the most recent Fetch
        /// </summary>
        /// <value><c>true</c> if successful; otherwise, <c>false</c>.</value>
        public bool Success { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// Gets the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public void Load(string url) {
            for (int retry = 0; retry < Retries; retry++) {
                try {
                    var req = HttpWebRequest.Create(url) as HttpWebRequest;
                    req.AllowAutoRedirect = true;
                    // ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true; // Turn off certificate checking
                    
                    if (Credential != null) {
                        req.Credentials = Credential;                        
                    }
                    req.Headers = Headers;
                    req.Timeout = Timeout;

                    Response = req.GetResponse() as HttpWebResponse;

                    switch (Response.StatusCode) {
                        case HttpStatusCode.Found:
                            // This is a redirect, just ignore.
                            break;
                        case HttpStatusCode.OK:
                            using (var sr = Response.GetResponseStream()) {
                                using (var ms = new MemoryStream()) {
                                    for (int b; (b = sr.ReadByte()) != -1;) {
                                        ms.WriteByte((byte)b);
                                    }
                                        
                                    ResponseData = ms.ToArray();
                                }
                            }
                            break;
                    }
                    Success = true;
                    break;
                } catch (WebException ex) {
                    Response = ex.Response as HttpWebResponse;

                    if (ex.Status == WebExceptionStatus.Timeout) {
                        Thread.Sleep(RetrySleep);
                        continue;
                    }

                    break;
                }
            }
        }


        public static byte[] Get(string url) {
            var f = new Fetch();
            f.Load(url);
            return f.ResponseData;
        }

        public string GetString() {
            var encoder = string.IsNullOrEmpty(Response.ContentEncoding) ? Encoding.UTF8 : Encoding.GetEncoding(Response.ContentEncoding);
            
            if (ResponseData == null)
                return string.Empty;
            
            return encoder.GetString(ResponseData);
        }
        #endregion
    }
}
