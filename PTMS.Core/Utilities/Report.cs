using System;

namespace PTMS.Core.Utilities {
    public static class Report {
        public static bool IsAutoProcess(dynamic report) {
            return Convert.ToBoolean(report.Encounter.NextGen.AutoSendToChart);
        }
    }
}
