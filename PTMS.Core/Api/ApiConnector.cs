using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace PTMS.Core.Api {
    public class ApiConnector {
        public static String API_ENDPOINT = ConfigurationManager.AppSettings[Constants.SETTING_API_URL];
        private const String JSON_MEDIA_TYPE = "application/json";

        public static dynamic GetResource(Uri apiUri, string endPoint, string authType, string authString) {
            using (var client = ApiClient(apiUri, authType, authString)) {
                HttpResponseMessage response = client.GetAsync(endPoint).Result;

                if (response.IsSuccessStatusCode) {
                    dynamic x = response.Content.ReadAsAsync<ExpandoObject>().Result;
                    return x;
                }

                throw new Exception("Error retrieving resource");
            }
        }

        public static T PostResource<T>(Uri apiUri, string endPoint, object postObject, string authType, string authString) {
            using (var client = ApiClient(apiUri, authType, authString)) {
                // HTTP Post
                HttpResponseMessage response = client.PostAsJsonAsync(endPoint, postObject).Result;

                if (!response.IsSuccessStatusCode) {
                    string content = response.Content.ReadAsStringAsync().Result;

                    throw new Exception("Error adding item: " + response + "\nContent: " + content);
                }

                return response.Content.ReadAsAsync<T>().Result;
            }
        }

        public static HttpResponseMessage PostFile(Uri apiUri, string endPoint, string filePath, string authType, string authString) {
            using (var client = ApiClient(apiUri, authType, authString)) {
                using (var content = new MultipartFormDataContent()) {
                    var fileContent = new ByteArrayContent(File.ReadAllBytes(filePath));

                    fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") {
                        FileName = Path.GetFileName(filePath)
                    };

                    content.Add(fileContent);

                    return client.PostAsync(endPoint, content).Result;
                }
            }
        }

        public static dynamic PutResource(Uri apiUri, string endPoint, string authType, string authString, string putObject) {
            using (var client = ApiClient(apiUri, authType, authString)) {
                HttpResponseMessage response = client.PutAsync(endPoint, new StringContent(putObject, Encoding.UTF8, JSON_MEDIA_TYPE)).Result;

                if (response.IsSuccessStatusCode) {
                    dynamic x = response.Content.ReadAsAsync<ExpandoObject>().Result;
                    return x;
                }

                if (response.StatusCode == HttpStatusCode.NotFound) { throw new FileNotFoundException(); }

                throw new Exception("Error retrieving resource:" + response.ReasonPhrase);
            }
        }

        public static void DeleteResource(Uri apiUri, string endPoint, string authType, string authString) {
            using (var client = ApiClient(apiUri, authType, authString)) {
                HttpResponseMessage response = client.DeleteAsync(endPoint).Result;

                if (!response.IsSuccessStatusCode) {
                    throw new Exception("Error Deleting item:" + apiUri + ":" + endPoint);
                }
            }
        }

        private static HttpClient ApiClient(Uri apiUri, string authType, string authString) {
            var client = new HttpClient {BaseAddress = apiUri};

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(JSON_MEDIA_TYPE));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authType, authString);
            //ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            return client;                
        }
    }
}
