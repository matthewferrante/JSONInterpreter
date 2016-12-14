using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace PTMS.HttpsAuthProxy.Filters {
    public class IdentityBasicAuthenticationAttribute : BasicAuthenticationAttribute {
        protected override async Task<IPrincipal> AuthenticateAsync(string userName, string password, CancellationToken cancellationToken, ApiUserManager aum) {
            cancellationToken.ThrowIfCancellationRequested();

            var u = await aum.FindAsync(userName, password);

            if (u == null) {
                return null;
            }

            // Create a ClaimsIdentity with all the claims for this user.
            Claim nameClaim = new Claim(ClaimTypes.Name, userName);
            Claim practiceIdClaim = new Claim(ClaimTypes.Sid, u.PracticeId);
            Claim passCodeClaim = new Claim(ClaimTypes.UserData, u.Passcode);
            List<Claim> claims = new List<Claim> { nameClaim, practiceIdClaim, passCodeClaim };
            

            // important to set the identity this way, otherwise IsAuthenticated will be false
            // see: http://leastprivilege.com/2012/09/24/claimsidentity-isauthenticated-and-authenticationtype-in-net-4-5/
            ClaimsIdentity identity = new ClaimsIdentity(claims, AuthenticationTypes.Basic);

            var principal = new ClaimsPrincipal(identity);
            return principal;
        }
    }
}