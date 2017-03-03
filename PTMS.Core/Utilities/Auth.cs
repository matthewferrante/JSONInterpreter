using System;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace PTMS.Core.Utilities {
    public static class Auth {
        //private const string authTemplate = "{0}:{1}";

        public static string CreateAuth(ClaimsIdentity c) {
            var pc = c.Claims.First(x => x.Type == ClaimTypes.UserData).Value;

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(pc));
        }
    }
}
