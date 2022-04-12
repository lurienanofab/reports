using LNF;
using LNF.DataAccess;
using LNF.Impl.Repository.Reporting;
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
    }

    public class EmailPreferenceItemFactory
    {
        public IProvider Provider { get; }

        private EmailPreferenceItemFactory(IProvider provider)
        {
            Provider = provider;
        }

        public static EmailPreferenceItemFactory Create(IProvider provider)
        {
            return new EmailPreferenceItemFactory(provider);
        }

        public ISession Session => Provider.DataAccess.Session;

        public IEnumerable<EmailPreferenceItem> Select(int clientId)
        {
            var prefs = Session.Query<EmailPreference>().ToList();

            var cp = Session.Query<ClientEmailPreference>().Where(x => x.ClientID == clientId && x.DisableDate == null).ToList();

            var result = new List<EmailPreferenceItem>();

            foreach (var p in prefs)
            {
                var item = new EmailPreferenceItem
                {
                    EmailPreferenceID = p.EmailPreferenceID,
                    ReportName = p.ReportName,
                    Description = p.Description,
                    ClientID = clientId,
                    Active = cp.Any(x => x.EmailPreferenceID == p.EmailPreferenceID)
                };

                result.Add(item);
            }

            return result;
        }
    }
}