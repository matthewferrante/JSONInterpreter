using System.ServiceProcess;
using PTMS.Core;

namespace PTMSClientService {
    partial class ProjectInstaller {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.ServiceProcess.ServiceProcessInstaller spi;
        private System.ServiceProcess.ServiceInstaller si;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent() {
            this.spi = new ServiceProcessInstaller();
            this.si = new ServiceInstaller();
            // 
            // spi
            // 
            this.spi.Account = ServiceAccount.LocalSystem;
            this.spi.Password = null;
            this.spi.Username = null;
            // 
            // si
            // 
            this.si.ServiceName = Constants.SERVICE_NAME;
            this.si.StartType = ServiceStartMode.Automatic;
            this.si.Description = "Prime Time Medical Software Client Service for Secure File Transfer";
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.spi,
            this.si});

        }

        #endregion
    }
}