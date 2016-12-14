using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using PTMSController.Annotations;

namespace PTMSController.Models {
    public class FindingRow {
        private bool _isIncluded = true;

        public string Category { get; set; }
        public string Finding { get; set; }
        public string Answer { get; set; }
        public bool IsIncluded {
            get { return _isIncluded; }
            set {
                _isIncluded = value;
                OnIncludedChanged(null);
            }
        }

        protected virtual void OnIncludedChanged(EventArgs e) {
            EventHandler<EventArgs> handler = IncludedChanged;

            if (handler != null) {
                handler(this, e);
            }
        }

        public event EventHandler<EventArgs> IncludedChanged;

        public FindingRow() {}
    }
}
