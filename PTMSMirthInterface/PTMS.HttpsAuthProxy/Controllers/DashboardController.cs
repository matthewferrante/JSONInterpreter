﻿using System;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json.Linq;
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

        [IdentityBasicAuthentication]
        [Authorize]
        public IHttpActionResult GetUpdate() {
            var s = JObject.Parse(File.ReadAllText(ConfigurationManager.AppSettings[Constants.SETTING_MANIFEST]));

            return Ok(s);
        }
    }
}
