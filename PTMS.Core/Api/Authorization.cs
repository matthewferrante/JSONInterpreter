using System;
using System.Text;

namespace PTMS.Core.Api {
    public static class Authorization {
        public static string CreateBasicAuth(string username, string password) {
            return Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));
        }
    }
}
