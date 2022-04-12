using LNF;
using LNF.CommonTools;
using LNF.Mail;
using LNF.Reporting;
using LNF.Reporting.Individual;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Reports.Models
{
    public class EmailManager
    {
        protected IProvider Provider { get; }

        public EmailManager(IProvider provider)
        {
            Provider = provider;
        }

        public IEnumerable<IReportingClient> GetManagers(string group, DateTime period)
        {
            var activeManagers = Provider.Reporting.ClientItem.SelectActiveManagers(period);

            IEnumerable<IReportingClient> filtered;

            switch (group)
            {
                case "all-internal":
                    filtered = activeManagers.Where(x => x.IsInternal);
                    break;
                case "all-external":
                    filtered = activeManagers.Where(x => !x.IsInternal);
                    break;
                case "all-internal-external":
                    filtered = activeManagers;
                    break;
                case "technical-internal":
                    filtered = activeManagers.Where(x => x.IsManager && !x.IsFinManager && x.IsInternal);
                    break;
                case "technical-external":
                    filtered = activeManagers.Where(x => x.IsManager && !x.IsFinManager && !x.IsInternal);
                    break;
                case "technical-internal-external":
                    filtered = activeManagers.Where(x => x.IsManager && !x.IsFinManager);
                    break;
                case "financial-internal":
                    filtered = activeManagers.Where(x => !x.IsManager && x.IsFinManager && x.IsInternal);
                    break;
                case "financial-external":
                    filtered = activeManagers.Where(x => !x.IsManager && x.IsFinManager && !x.IsInternal);
                    break;
                case "financial-internal-external":
                    filtered = activeManagers.Where(x => !x.IsManager && x.IsFinManager);
                    break;
                case "technical-financial-internal":
                    filtered = activeManagers.Where(x => x.IsManager && x.IsFinManager && x.IsInternal);
                    break;
                case "technical-financial-external":
                    filtered = activeManagers.Where(x => x.IsManager && x.IsFinManager && !x.IsInternal);
                    break;
                case "technical-financial-internal-external":
                    filtered = activeManagers.Where(x => x.IsManager && x.IsFinManager);
                    break;
                default:
                    throw new NotImplementedException("Unknown recipient group.");
            }

            return filtered;
        }

        public IEnumerable<EmailMessage> CreateManagerUsageSummaryEmails(DateTime period, IEnumerable<IReportingClient> managers, bool includeRemote)
        {
            int managerSummaryReportEmailPreferenceId = 1;

            var prefs = Provider.Reporting.ClientEmailPreference.GetClientEmailPreferences(managerSummaryReportEmailPreferenceId);

            var sendTo = new List<ManagerSummaryReportEmailArgs>();

            var generator = new ReportGenerator(Provider);

            foreach (var mgr in managers)
            {
                // check if any pref exists
                var exists = prefs.Any(x => x.ClientID == mgr.ClientID);

                if (!exists)
                {
                    // add opt-in pref by default
                    var p = Provider.Reporting.ClientEmailPreference.AddClientEmailPreference(managerSummaryReportEmailPreferenceId, mgr.ClientID);

                    var items = generator.GetManagerUsageSummaryItems(mgr.ClientID, period, includeRemote);

                    var model = generator.CreateManagerUsageSummary(period, mgr, items);

                    if (model.Accounts.Count() > 0)
                    {
                        sendTo.Add(ManagerSummaryReportEmailArgs.Create(p, model, mgr));
                    }
                }
                else
                {
                    // check if enabled
                    var p = prefs.FirstOrDefault(x => x.ClientID == mgr.ClientID && x.DisableDate == null);

                    if (p != null)
                    {
                        // client has not opted out

                        var items = generator.GetManagerUsageSummaryItems(mgr.ClientID, period, includeRemote);

                        var model = generator.CreateManagerUsageSummary(period, mgr, items);

                        if (model.Accounts.Count() > 0)
                        {
                            sendTo.Add(ManagerSummaryReportEmailArgs.Create(p, model, mgr));
                        }
                    }
                }
            }

            string senderEmail = ConfigurationManager.AppSettings["ManagerUsageSummarySenderEmail"]; //lnf-system@umich.edu
            string senderName = ConfigurationManager.AppSettings["ManagerUsageSummarySenderName"]; //LNF System
            string subject = ConfigurationManager.AppSettings["ManagerUsageSummarySubject"]; //LNF Manager Usage Summary Report for {0:MMM yyyy}

            List<EmailMessage> result = new List<EmailMessage>();

            foreach (var args in sendTo)
            {
                if (args.Model.Accounts.Count() > 0)
                {
                    result.Add(new EmailMessage()
                    {
                        RecipientEmail = args.Manager.Email,
                        RecipientName = args.Manager.LName + ", " + args.Manager.FName,
                        SenderEmail = senderEmail,
                        SenderName = senderName,
                        Subject = string.Format(subject, period),
                        Body = GetManagerSummaryReportBody(args)
                    });
                }
            }

            return result;
        }

        public int SendManagerSummaryReport(int currentUserClientId, DateTime period, string message, string ccaddr, bool debug, bool includeRemote)
        {
            // Get all active managers and check for any that do not
            // already have a preference. They will be opted in by default.

            var activeManagers = GetManagers("all-internal-external", period);
            return SendManagerSummaryReport(currentUserClientId, period, activeManagers, message, ccaddr, debug, includeRemote);
        }

        public int SendManagerSummaryReport(int currentUserClientId, DateTime period, IEnumerable<IReportingClient> managers, string message, string ccaddr, bool debug, bool includeRemote)
        {
            var emails = CreateManagerUsageSummaryEmails(period, managers, includeRemote);
            return SendManagerSummaryReport(currentUserClientId, emails, message, ccaddr, debug);
        }

        public int SendManagerSummaryReport(int currentUserClientId, IEnumerable<EmailMessage> emails, string message, string ccaddr, bool debug)
        {
            int count = 0;

            string debugEmail = ConfigurationManager.AppSettings["DebugEmail"];

            string[] cc = !string.IsNullOrEmpty(ccaddr) ? new[] { ccaddr } : null;

            foreach (var e in emails)
            {
                string toAddr = debug && !string.IsNullOrEmpty(debugEmail) ? debugEmail : e.RecipientEmail;

                string body;

                if (string.IsNullOrEmpty(message))
                    body = e.Body.Replace("%message", string.Empty);
                else
                    body = e.Body.Replace("%message", message + "<br/><br/>");

                var sendMessageArgs = new SendMessageArgs()
                {
                    ClientID = currentUserClientId,
                    Caller = "Reports.Models.EmailManager.SendManagerSummaryReport",
                    Subject = e.Subject,
                    Body = body,
                    From = e.SenderEmail,
                    DisplayName = e.SenderName,
                    To = new[] { toAddr },
                    Cc = cc,
                    IsHtml = true
                };

                Provider.Mail.SendMessage(sendMessageArgs);

                count += 1;
            }

            return count;
        }

        private string GetManagerSummaryReportBody(ManagerSummaryReportEmailArgs args)
        {
            var model = args.Model;
            var client = args.Manager;
            var width = args.Model.ShowSubsidyColumn ? 1000 : 800;

            var unsubscribeUrl = VirtualPathUtility.ToAbsolute(string.Format("~/unsubscribe/{0}", Encryption.SHA256.EncryptText(args.Preference.ClientEmailPreferenceID.ToString())));

            string result = TemplateManager.ManagerUsageSummaryEmailTemplate(new { client, model, width, unsubscribeUrl });

            return result;

            /*
            Dear ***FirstName_LastName***,

            Below is a summary of your group's usage charges in the LNF during the month of  ****Period****. The numbers include all
            LNF charges (room, tool and store). Please note that, while they are estimates that may still change, this message intends
            to give you an early reasonable indication of usage charges, before the final numbers are posted/invoiced. 
            
            ************************************************
            **************** TABLES **********************
            ************************************************

            As a reminder, more details about lab usage for each user listed above can be found in the LNF Online Services (http://ssel-sched.eecs.umich.edu/sselonline). Instructions are available at http://lnf-wiki.eecs.umich.edu/wiki/Online_Usage_Report

            If any change in the list of lab users or accounts is needed for future usage, or if you have any question, please contact LNF-billing@umich.edu.

            Best regards,
            Sandrine
            */

            //string message = TemplateManager.ManagerUsageReportBodyHeaderTemplate(new { fname = args.Model.LName, lname = args.Model.FName, period = args.Model.Period });
            //string message = "";

            ////string message = setting
            ////    .Replace("{Period}", args.Model.Period.ToString("MMM yyyy"))
            ////    .Replace("{DisplayName}", args.Model.DisplayName);

            //string result = string.Format("<p>{0}</p><hr/>", message);

            //result += "<br/><b>Charges by Account</b>";
            //result += CreateManagerUsageSummaryTable(args.Model.AccountItems, "Account", args.Model.ShowSubsidyColumn);

            //result += "<br/><br/><b>Charges by Lab User</b>";
            //result += CreateManagerUsageSummaryTable(args.Model.ClientItems, "Lab User", args.Model.ShowSubsidyColumn);

            //string footer = "";



            //result += footer;

            //return result;
        }
    }

    public struct ManagerSummaryReportEmailArgs
    {
        public static ManagerSummaryReportEmailArgs Create(IClientEmailPreference pref, ManagerUsageSummary model, IReportingClient manager)
        {
            var result = new ManagerSummaryReportEmailArgs
            {
                Preference = pref,
                Model = model,
                Manager = manager
            };

            return result;
        }

        public IClientEmailPreference Preference { get; private set; }
        public ManagerUsageSummary Model { get; private set; }
        public IReportingClient Manager { get; private set; }
    }
}