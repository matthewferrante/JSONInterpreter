using System;

namespace PTMS.Core.Api {
    public class ApiCredentials {
        public string Username { get; set; }
        public string Password { get; set; }
        public Uri ApiUri { get; set; }

        public string AuthToken  {
            get {
                return Authorization.CreateBasicAuth(Username, Password);
            }
        }
    }
}
