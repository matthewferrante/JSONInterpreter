using System;

namespace PTMS.Core.Api {
    public static class DashboardConnector {
        private const String AUTH_TYPE = "Basic";

        public static dynamic GetVersion(Uri apiEndPoint, string auth) {
            return ApiConnector.GetResource(apiEndPoint, "Dashboard/Version", AUTH_TYPE, auth);
        }

        public static dynamic GetUpdateManifest(Uri apiEndPoint, string version, string auth) {
            return ApiConnector.GetResource(apiEndPoint, "Dashboard/Update/" + version, AUTH_TYPE, auth);
        }

    }
}
