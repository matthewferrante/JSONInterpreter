using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace PTMS.HttpsAuthProxy.Models {
    public class ApiUser : IdentityUser {
        [Required]
        public String PracticeId { get; set; }
        [Required]
        public String Passcode { get; set; }
        [Required]
        public String EncryptionKey { get; set; } 

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApiUser> manager, string authenticationType) {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApiDbContext : IdentityDbContext<ApiUser> {
        public ApiDbContext() : base("PTMS.Identity.Connection", throwIfV1Schema: false) { }
        public static ApiDbContext Create() { return new ApiDbContext(); }
    }

    public class ApplicationUser : IdentityUser {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType) {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser> {
        public ApplicationDbContext() : base("PTMS.AdminIdentity.Connection", throwIfV1Schema: false) { }
        public static ApplicationDbContext Create() { return new ApplicationDbContext(); }
    }
}