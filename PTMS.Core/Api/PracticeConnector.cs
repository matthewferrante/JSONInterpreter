using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PTMS.Core.Api;
using PTMS.Core.Crypto;
using PTMS.Core.Logging;
using PTMS.Core.Utilities;

namespace PTMS.Core.Api {
    public static class PracticeConnector {
        private const String AUTH_TYPE = "Basic";

        public static dynamic GetAvailableReports(Uri apiEndPoint, string auth) {
            var s = ApiConnector.GetResource(apiEndPoint, "Practice", AUTH_TYPE, auth);

            return s.ReportInbox.ReportIds;
        }

        public static dynamic GetReport(Uri apiEndpoint, string reportId, string auth) {
            string patientId = ExtractPatientId(reportId);

            return ApiConnector.GetResource(apiEndpoint, patientId + "/Report/" + reportId, AUTH_TYPE, auth);
        }

        public static String GetEncryptionKey(Uri apiEndpoint, string auth) {
            return ApiConnector.GetResource(apiEndpoint, "EncryptionKey", AUTH_TYPE, auth).Key;
        }

        public static async Task<bool> SendFile(Uri apiEndpoint, string filePath, string auth) {
            var hrm = ApiConnector.PostFile(apiEndpoint, "File", filePath, AUTH_TYPE, auth);

            if (hrm.StatusCode == HttpStatusCode.OK) {
                return true;
            }

            throw new Exception(String.Format("API = {0}, exception = {1}, status = {2}", apiEndpoint, await hrm.Content.ReadAsStringAsync(), hrm.StatusCode));
        }

        /// <summary>
        /// Delete Report from the API
        /// </summary>
        /// <param name="apiEndpoint">Location of the API</param>
        /// <param name="reportId">Report ID to delete</param>
        /// <param name="auth">The authorization credentials to pass to the API to authenticate and authorize the ability to delete</param>
        /// <returns></returns>
        public static bool DeleteReport(Uri apiEndpoint, string reportId, string auth) {
            try {
                ApiConnector.DeleteResource(apiEndpoint, "Report/" + reportId, AUTH_TYPE, auth);
            } catch {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Downlaod reports to the local disk
        /// </summary>
        /// <param name="creds">Api Credentials to connect to the API</param>
        /// <param name="log">Logger to log the action</param>
        /// <param name="writeDirectory">Where to write the reports once downloaded</param>
        /// <param name="processedDirectory">Where to write the report if it has the flag to be auto-processed once downloaded</param>
        /// <param name="passPhrase">Encryption key to be used</param>
        public static void DownloadReports(ApiCredentials creds, Logger log, string writeDirectory, string processedDirectory, string passPhrase) {
            log.Log("**** Downloading Reports ******");
            var reports = GetAvailableReports(creds.ApiUri, creds.AuthToken);

            foreach (string reportId in reports) {
                log.Log(String.Format("Found Report: {0}", reportId));

                var s = GetReport(creds.ApiUri, reportId, creds.AuthToken);
                var reportdata = JObject.Parse(s.ReportInbox.ReportInfo.ReportData);
                var outputPath = Path.Combine(writeDirectory, reportId);
                var autoPath = Path.Combine(processedDirectory, reportId);
                string json = JsonConvert.SerializeObject(reportdata);

                try {
                    if (Report.IsAutoProcess(reportdata)) {  
                        File.WriteAllText(autoPath, json);

                        VerifyAndDelete(autoPath, log, creds, reportId);
                    } else {
                        File.WriteAllText(outputPath, StringCipher.Encrypt(json, passPhrase));

                        VerifyAndDelete(outputPath, log, creds, reportId);
                    }
                } catch (Exception ex) {
                    log.LogException(String.Format("Could not Read Report for Auto Processing: {0}", reportId), ex.ToString());
                }
            }
        }

        private static bool VerifyDownload(string path) {
            return File.Exists(path);
        }

        private static void VerifyAndDelete(string path, Logger log, ApiCredentials creds, string reportId) {
            if (VerifyDownload(path)) {
                log.Log(String.Format("Downloaded Report {0}", reportId));

                try {
                    log.Log(String.Format("Removing Report from API: {0}", reportId));
                    DeleteReport(creds.ApiUri, reportId, creds.AuthToken);
                } catch (Exception ex) {
                    log.LogException(String.Format("Could not Delete Report: {0}", reportId), ex.ToString());
                }
            } else {
                log.Log(String.Format("Error in download, could not verify file was written. Output path = {0}", path));
            }                        
        }

        private static string GetPractice(Uri apiEndPoint, string auth) {
            return ApiConnector.GetResource(apiEndPoint, "Practice", AUTH_TYPE, auth);
        }
        private static string ExtractPatientId(string reportId) {
            var idx = reportId.IndexOf("{PID_", StringComparison.Ordinal)+5;

            return reportId.Substring(idx, reportId.IndexOf("}", reportId.IndexOf("{PID_")) - idx);

        }
    }
}
