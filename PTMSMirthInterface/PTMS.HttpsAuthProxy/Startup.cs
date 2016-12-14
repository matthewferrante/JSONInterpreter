using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(PTMS.HttpsAuthProxy.Startup))]

namespace PTMS.HttpsAuthProxy {
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
