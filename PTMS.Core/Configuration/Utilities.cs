using System;
using System.Configuration;
using PTMS.Core.Api;

namespace PTMS.Core.Configuration {
    public static class Utilities {
        public static ApiCredentials GetCredentials() {
            return new ApiCredentials() {
                Username = ConfigurationManager.AppSettings[Constants.SETTING_USERNAME],
                Password = ConfigurationManager.AppSettings[Constants.SETTING_PASSWORD],
                ApiUri = new Uri(ConfigurationManager.AppSettings[Constants.SETTING_API_URL])
            };
        }

        public static string GetSetting(string setting) {
            return ConfigurationManager.AppSettings[setting];
        }
    }
}
