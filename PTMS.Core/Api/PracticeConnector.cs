﻿using System;
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

            throw new Exception(await hrm.Content.ReadAsStringAsync());
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
        /// <param name="practiceId">The practiceId to download reports from</param>
        /// <param name="log">Logger to log the action</param>
        /// <param name="writeDirectory">Where to write the logs once downloaded</param>
        public static void DownloadReports(ApiCredentials creds, Logger log, string writeDirectory, string passPhrase) {
            log.Log("**** Downloading Reports ******");
            var reports = GetAvailableReports(creds.ApiUri, creds.AuthToken);

            foreach (string reportId in reports) {
                log.Log(String.Format("Found Report: {0}", reportId));

                var s = GetReport(creds.ApiUri, reportId, creds.AuthToken);
                var reportdata = JObject.Parse(s.ReportInbox.ReportInfo.ReportData);

                string json = JsonConvert.SerializeObject(reportdata);
                File.WriteAllText( Path.Combine(writeDirectory,reportId), StringCipher.Encrypt(json, passPhrase));

                log.Log(String.Format("Downloaded Report {0}", reportId));
                log.Log(String.Format("Removing Report from API: {0}", reportId));

                try {
                    DeleteReport(creds.ApiUri, reportId, creds.AuthToken);
                } catch (Exception ex) {
                    log.LogException(String.Format("Could not Delete Report: {0}", reportId), ex.ToString());
                }
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
