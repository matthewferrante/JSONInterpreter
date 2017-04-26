using System;

namespace PTMS.Core {
    public static class Constants {
        public const String API_CURRENT_VERSION = "v1";
        public const String API_ROUTE_TEMPLATE = API_CURRENT_VERSION + "/{controller}/{id}";
        public const String USER_NAME = "userName",
                            PASSWORD = "password",
                            SERVICE_NAME = "PTMSClientService",
                            DASHBOARD_NAME = "PTMS Dashboard",
                            CONTROLLER_MANAGER = "Controller.Manager",
                            SERVICE_EXE = "PTMSClientService.exe",
                            SERVICE_PATH_KEY = "ServiceRelativePath",
                            ENCRYPTION_SALT = "jjwofjeijwofjqhuwq38vv9",
                            APP_SETTING_LOGFILE = "LogFileName",
                            SETTING_API_URL = "ApiEndpointUrl",
                            SETTING_DASHBOARD_VERSION = "DashboardVersion",
                            SETTING_USERNAME = "ProviderUserName",
                            SETTING_PASSWORD = "ProviderPassword",
                            SETTING_PRACTICE_ID = "PracticeId",
                            SETTING_FILE_SAVE_PATH = "IncomingFileSavePath",
                            SETTING_INCOMING_DIRECTORY = "IncomingDirectory",
                            SETTING_OUTGOING_DIRECTORY = "OutgoingDirectory",
                            SETTING_PROCESSED_DIRECTORY = "ProcessedDirectory",
                            SETTING_DELETED_DIRECTORY = "DeletedDirectory",
                            SETTING_UPLOAD_DIRECTORY = "UploadDirectory",
                            SETTING_MANIFEST_DIRECTORY = "ManifestDirectory",
                            INTERVAL_REPORT = "ReportIntervalMinutes",
                            INTERVAL_UPDATE = "UpdateIntervalMinutes",
                            INTERVAL_UPLOAD = "UploadIntervalMinutes"
                            ;


        public const String TEMPLATE_EXCEPTION = "***************************************** EXCEPTION ********************************************\n" +
                                                 "Location: {0}\n" +
                                                 "Exception: {1}\n" +
                                                 "************************************************************************************************\n";
    }
}
