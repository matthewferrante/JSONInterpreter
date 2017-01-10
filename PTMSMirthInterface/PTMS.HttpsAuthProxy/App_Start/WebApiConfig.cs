using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Serialization;
using PTMS.Core;

namespace PTMS.HttpsAuthProxy {
    public static class WebApiConfig {
        public static void Register(HttpConfiguration config) {
            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(name: "ReportsApi", routeTemplate: Constants.API_CURRENT_VERSION + "/{patientId}/Reports", defaults: new { controller = "Practice", action = "GetReports" }, constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Get) });
            config.Routes.MapHttpRoute(name: "ReportApi", routeTemplate: Constants.API_CURRENT_VERSION + "/{patientId}/Report/{reportId}", defaults: new { controller = "Practice", action = "GetReport" }, constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Get) });
            config.Routes.MapHttpRoute(name: "PracticeApi", routeTemplate: Constants.API_CURRENT_VERSION + "/Practice", defaults: new { controller = "Practice", action = "GetPractice" }, constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Get)});
            config.Routes.MapHttpRoute(name: "EncryptionKey", routeTemplate: Constants.API_CURRENT_VERSION + "/EncryptionKey", defaults: new { controller = "Practice", action = "GetEncryptionKey" }, constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Get) });
            config.Routes.MapHttpRoute(name: "PracticeMessageApi", routeTemplate: Constants.API_CURRENT_VERSION + "/File", defaults: new { controller = "Practice", action = "PostFile" }, constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Post) });
            config.Routes.MapHttpRoute(name: "RemoveReportApi", routeTemplate: Constants.API_CURRENT_VERSION + "/Report/{reportId}", defaults: new { controller = "Practice", action = "DeleteReport" }, constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Delete) });
            config.Routes.MapHttpRoute(name: "DashBoardVersion", routeTemplate: Constants.API_CURRENT_VERSION + "/Dashboard/Version", defaults: new { controller = "Dashboard", action = "GetVersion" }, constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Get) });


            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: Constants.API_ROUTE_TEMPLATE,
                defaults: new { id = RouteParameter.Optional }
            );
            //"api/{controller}/{id}"
        }
    }
}
