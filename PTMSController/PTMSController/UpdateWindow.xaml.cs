using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PTMS.Core.Api;
using PTMS.Core.Configuration;
using PTMS.Core.Utilities;

namespace PTMSController {
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window {
        protected Version ServerVersion;
        protected Version Version;
        private dynamic _creds;
        

        public UpdateWindow() {
            InitializeComponent();

            _creds = Utilities.GetCredentials();
            ServerVersion = new Version(DashboardConnector.GetVersion(_creds.ApiUri, _creds.AuthToken).Version);
            Version = Assembly.GetExecutingAssembly().GetName().Version;

            lblVersion.Content = Version;

            if (ServerVersion.CompareTo(Version) > 0) {
                lblServerVersion.Content = String.Format("The lastest verision is {0}", ServerVersion);
                btnUpdate.Visibility = Visibility.Visible;
            }
            
        }

        private void Update_Click(object sender, RoutedEventArgs e) {
            try {
                    // Do update
                    var m = DashboardConnector.GetUpdateManifest(_creds.ApiUri, ServerVersion.ToString(), _creds.AuthToken);

                    Updater u = new Updater(m.Version);

                    u.Check(_creds.ApiUri, m);
            } catch (Exception ex) {
                MessageBox.Show("Unable to check for updates.  Please try again shortly.", "Update", MessageBoxButton.OK);
            }
        }
    }
}
