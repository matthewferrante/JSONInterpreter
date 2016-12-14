using System;

namespace PTMS.Core.Api {
    public static class DashboardConnector {
        private const String AUTH_TYPE = "Basic";

        public static dynamic GetVersion(Uri apiEndPoint, string auth) {
            return ApiConnector.GetResource(apiEndPoint, "Dashboard/Version", AUTH_TYPE, auth);
        }
    }
}
