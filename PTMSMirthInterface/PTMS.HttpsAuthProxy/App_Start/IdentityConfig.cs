using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using PTMS.HttpsAuthProxy.Models;

namespace PTMS.HttpsAuthProxy {
    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.

    public class ApiUserManager : UserManager<ApiUser> {
        public ApiUserManager(IUserStore<ApiUser> store)
            : base(store) {
        }

        public static ApiUserManager Create(IdentityFactoryOptions<ApiUserManager> options, IOwinContext context) {
            var manager = new ApiUserManager(new UserStore<ApiUser>(context.Get<ApiDbContext>()));
            
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApiUser>(manager) {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator {
                RequiredLength = 12,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            var dataProtectionProvider = options.DataProtectionProvider;

            if (dataProtectionProvider != null) {
                manager.UserTokenProvider = new DataProtectorTokenProvider<ApiUser>(dataProtectionProvider.Create("PTMS SecureIdentity"));
            }

            return manager;
        }
    }

    public class ApplicationUserManager : UserManager<ApplicationUser> {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store) {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context) {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));

            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager) {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator {
                RequiredLength = 12,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            var dataProtectionProvider = options.DataProtectionProvider;

            if (dataProtectionProvider != null) {
                manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("PTMS SecureAdminIdentity"));
            }

            return manager;
        }
    }
}
