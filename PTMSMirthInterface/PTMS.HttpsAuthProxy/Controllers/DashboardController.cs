using System;
using System.Configuration;
using System.Net.Http;
using System.Web.Http;
using PTMS.Core;
using PTMS.HttpsAuthProxy.Filters;

namespace PTMS.HttpsAuthProxy.Controllers {
    public class DashboardController : ApiController {
        [IdentityBasicAuthentication]
        [Authorize]
        public IHttpActionResult GetVersion() {
            dynamic a = new { Version = ConfigurationManager.AppSettings[Constants.SETTING_DASHBOARD_VERSION]};

            return Ok(a);
        }
    }
}
