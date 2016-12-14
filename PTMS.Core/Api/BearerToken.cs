using System;

namespace PTMS.Core {
    public class BearerToken {
        public String access_token { get; set; }
        public String token_type { get; set; }
        public String userName { get; set; }
        public double expires_in { get; set; }
    }
}