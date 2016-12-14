using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using PTMS.Core;
using PTMS.Core.Api;
using PTMS.Core.Utilities;
using PTMS.HttpsAuthProxy.Filters;

namespace PTMS.HttpsAuthProxy.Controllers {
    [OutputCache(VaryByParam = "*", Duration = 0, NoStore = true)] // will be applied to all actions in Controller, unless those actions override with their own decoration
    public class PracticeController : ApiController {
        private readonly Uri uri = new Uri(ApiConnector.API_ENDPOINT);
        private const string getReportsTemplate = "reportinbox/?nextgenPid={0}";
        private const string getReportTemplate = "reportinbox/?reportId={0}";
        private const string getPracticeTemplate = "reportinbox/?nextgenImhId={0}";
        private const string putDeleteTemplate = "{{\"Command\":\"DeleteReport\",\"ReportId\":\"{0}\"}}";

        [IdentityBasicAuthentication]
        [System.Web.Http.Authorize]
        public IHttpActionResult GetReports(string patientId) {
            var c = RequestContext.Principal.Identity as ClaimsIdentity;
            var auth = Auth.CreateAuth(c);
            var resp = ApiConnector.GetResource(uri, String.Format(getReportsTemplate, patientId), "Basic", auth);

            return Ok(resp);
        }

        [IdentityBasicAuthentication]
        [System.Web.Http.Authorize]
        public IHttpActionResult GetReport(string patientId, string reportId) {
            var c = RequestContext.Principal.Identity as ClaimsIdentity;
            var auth = Auth.CreateAuth(c);
            var resp = ApiConnector.GetResource(uri, String.Format(getReportTemplate, reportId), "Basic", auth);

            return Ok(resp);
        }

        [IdentityBasicAuthentication]
        [System.Web.Http.Authorize]
        public IHttpActionResult GetPractice(string practiceId) {
            var c = RequestContext.Principal.Identity as ClaimsIdentity;
            var auth = Auth.CreateAuth(c);
            var resp = ApiConnector.GetResource(uri, String.Format(getPracticeTemplate, practiceId), "Basic", auth);

            return Ok(resp);
        }

        [IdentityBasicAuthentication]
        [System.Web.Http.Authorize]
        public IHttpActionResult DeleteReport(string reportId) {
            var c = RequestContext.Principal.Identity as ClaimsIdentity;
            var s = String.Format(putDeleteTemplate, reportId);
            var auth = Auth.CreateAuth(c);

            ApiConnector.PutResource(uri, "reportinbox", "Basic", auth, s);

            return Ok();
        }
        
        [System.Web.Http.HttpPost]
        [IdentityBasicAuthentication]
        [System.Web.Http.Authorize]
        public async Task<IHttpActionResult> PostFile(string practiceId) {
            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            var provider = new MultipartMemoryStreamProvider();
            
            await Request.Content.ReadAsMultipartAsync(provider);

            String savePath = Path.GetFullPath(ConfigurationManager.AppSettings[Constants.SETTING_FILE_SAVE_PATH]);

            FileSystem.AssertDirectoryExists(savePath + practiceId);

            foreach (var file in provider.Contents) {
                var filename = file.Headers.ContentDisposition.FileName.Trim('\"');
                var buffer = await file.ReadAsByteArrayAsync();
                
                File.WriteAllBytes(savePath+practiceId+ "\\" + filename, buffer);
            }

            return Ok();
        }
    }
}
