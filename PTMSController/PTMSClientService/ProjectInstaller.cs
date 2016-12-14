using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace PTMSClientService {
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer {
        public ProjectInstaller() {
            InitializeComponent();
        }

        protected override void OnAfterInstall(IDictionary savedState) {
            base.OnAfterInstall(savedState);

            //The following code starts the services after it is installed.
            using (ServiceController sc = new ServiceController(si.ServiceName)) {
                sc.Start();
            }
        }
    }
}
