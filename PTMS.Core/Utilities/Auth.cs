using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PTMS.Core.Utilities {
    public static class Auth {
        private const string authTemplate = "{0}:{1}";

        public static string CreateAuth(ClaimsIdentity c) {
            var pid = c.Claims.First(x => x.Type == ClaimTypes.Sid).Value;
            var pc = c.Claims.First(x => x.Type == ClaimTypes.UserData).Value;

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(String.Format(authTemplate, pid, pc)));
        }
    }
}
