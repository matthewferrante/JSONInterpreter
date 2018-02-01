using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Newtonsoft.Json.Linq;
using PTMSController.Models;
using System.IO;
using System.Security;
using Newtonsoft.Json;
using PTMS.Core;
using PTMS.Core.Api;
using PTMS.Core.Configuration;
using PTMS.Core.Crypto;
using PTMS.Core.Utilities;

namespace PTMSController {
    /// <summary>
    /// Interaction logic for ReviewWindow.xaml
    /// </summary>
    public partial class ReviewWindow : Window {
        private ObservableCollection<FindingRow> _findingRows = new ObservableCollection<FindingRow>();
        private dynamic _findings;
        private string _processedDir;
        private string _fileName;
        private String _key;
        private readonly PracticeControllerManager pcm;

        public ReviewWindow(ReviewRow rr) {

            InitializeComponent();
            _processedDir = FileSystem.BuildAbsolutePath(Utilities.GetSetting(Constants.SETTING_PROCESSED_DIRECTORY));
            _fileName = rr.FileName;

            var creds = Utilities.GetCredentials();
            _key = PracticeConnector.GetEncryptionKey(creds.ApiUri, creds.AuthToken);

            dynamic r = JObject.Parse(StringCipher.Decrypt(File.ReadAllText(_fileName), _key));  // This may cause problems.  Full path being read from variable?

            tbFirstName.Text = r.Patient.FirstName;
            tbLastName.Text = r.Patient.LastName;
            tbPatientId.Text = r.Patient.PatientId;
            tbDOB.Text = String.Format("{0}/{1}/{2}",r.Patient.DateOfBirth.Month,r.Patient.DateOfBirth.Day,r.Patient.DateOfBirth.Year);

            Title = String.Format("Review for {0},{1}", r.Patient.LastName, r.Patient.FirstName);
            _findings = r.Encounter.NextGen.Findings;

            foreach (var finding in _findings) {
                var fr = new FindingRow() {Category = finding.Category, Answer = finding.PatientResponse, Finding = finding.DisplayText};
                fr.IncludedChanged += UpdateSavedFindings;
                 _findingRows.Add(fr);
            }

            tbTotalFindings.Text = _findingRows.Count.ToString();
            tbSavedFindings.Text = _findingRows.Count.ToString();
            dgFindings.ItemsSource = _findingRows;
            pcm = PracticeControllerManager.Current;
        }

        private void Save_Click(object sender, RoutedEventArgs e) {
            JArray sf = new JArray();

            dynamic r = JObject.Parse(StringCipher.Decrypt(File.ReadAllText(_fileName), _key));

            foreach (var finding in _findings) {
                string dt = finding.DisplayText;
                if (_findingRows.AsQueryable().Any(x => x.IsIncluded && x.Finding.Equals(dt))) {
                    sf.Add(finding);
                }
            }

            r.Encounter.NextGen.Findings = sf;

            File.WriteAllText(Path.Combine(_processedDir, Path.GetFileName(_fileName)), r.ToString());
            pcm.DeleteIncomingQuestionnaire(_fileName);

            Close();
        }

        private bool _AllChecked = true;
        public bool AllChecked {
            get { return _AllChecked; }
            set {
                _AllChecked = value;
                _findingRows.ToList().ForEach(x => x.IsIncluded = value);
                
                dgFindings.Items.Refresh();
            }
        }

        private void UpdateSavedFindings(object sender, EventArgs e) {
            Dispatcher.Invoke(delegate { tbSavedFindings.Text = _findingRows.Count(x => x.IsIncluded).ToString(); });
        }

        private void CbxAll_OnChecked(object sender, RoutedEventArgs e) {
            var cb = sender as CheckBox;

            AllChecked = cb.IsChecked.HasValue ? cb.IsChecked.Value : false;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) {
            Close();
        }

    }
}
