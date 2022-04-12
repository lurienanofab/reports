using LNF.Reporting;
using System.Collections.Generic;

namespace Reports.Models
{
    public class EmailPreferenceModel
    {
        public int ClientID { get; set; }
        public string DisplayName { get; set; }
        public IEnumerable<IReportingClient> AvailableClients { get; set; }
        public IEnumerable<EmailPreferenceItem> AvailableItems { get; set; }
        public IEnumerable<int> SelectedItems { get; set; }
        public bool CanSelectUser { get; set; }
        public string Command { get; set; }
        
        public EmailPreferenceModel()
        {
            AvailableClients = new List<ReportingClientItem>();
            AvailableItems = new List<EmailPreferenceItem>();
            SelectedItems = new List<int>();
        }
    }
}