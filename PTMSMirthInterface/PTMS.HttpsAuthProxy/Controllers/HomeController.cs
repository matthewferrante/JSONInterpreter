using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using PTMS.Core;
using PTMS.Core.Api;

namespace PTMS.HttpsAuthProxy.Controllers {
    public class HomeController : Controller {
        public ActionResult Index() {
            const string patientId = "12344";
            const string authTemplate = "{0}:{1}";
            const string getReportsTemplate = "reportinbox/?nextgenPid={0}";
            string getReportTemplate = "reportinbox/?reportId={0}";

            // Password = Next#Gen$Demo=Inbox
            // UserName = b1582c80-1102-4da4-a059-6babefc446d4
            // Expected = YjE1ODJjODAtMTEwMi00ZGE0LWEwNTktNmJhYmVmYzQ0NmQ0Ok5leHQjR2VuJERlbW89SW5ib3g=

            var p = "Next#Gen$Demo=Inbox";
            var u = "0ed6c1a8-7af9-4578-a5e6-91eb26406a14";

            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(String.Format(authTemplate, u, p)));

            ViewBag.Title = "Home Page";
            var uri = new Uri(ApiConnector.API_ENDPOINT);

            var s = ApiConnector.GetResource(uri, String.Format(getReportsTemplate, patientId), "Basic", auth);

            ViewBag.ApiMessage = s.ApiStatus.ApiMessage;

            return View();
        }
    }
}
