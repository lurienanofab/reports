using LNF.Repository;
using LNF.Repository.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Reports.Models
{
    public class EmailPreferenceItem
    {
        public int ClientEmailPreferenceID { get; set; }
        public int EmailPreferenceID { get; set; }
        public int ClientID { get; set; }
        public string ReportName { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }

        public static IEnumerable<EmailPreferenceItem> Select(int clientId)
        {
            var prefs = DA.Current.Query<EmailPreference>().ToList();

            var cp = DA.Current.Query<ClientEmailPreference>().Where(x => x.ClientID == clientId && x.DisableDate == null).ToList();

            var result = new List<EmailPreferenceItem>();

            foreach (var p in prefs)
            {
                var item = new EmailPreferenceItem();

                item.EmailPreferenceID = p.EmailPreferenceID;
                item.ReportName = p.ReportName;
                item.Description = p.Description;
                item.ClientID = clientId;
                item.Active = cp.Any(x => x.EmailPreferenceID == p.EmailPreferenceID);

                result.Add(item);
            }

            return result;
        }
    }
}