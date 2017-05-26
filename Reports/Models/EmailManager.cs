using LNF;
using LNF.Cache;
using LNF.CommonTools;
using LNF.Email;
using LNF.Models.Reporting;
using LNF.Models.Reporting.Individual;
using LNF.Reporting;
using LNF.Repository;
using LNF.Repository.Reporting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Reports.Models
{
    public static class EmailManager
    {
        public static int SendManagerSummaryReport(DateTime period, bool includeRemote)
        {
            // Get all active managers and check for any that do not
            // already have a preference. They will be opted in by default.

            var activeManagers = ClientItemUtility.SelectActiveManagers(period);

            return SendManagerSummaryReport(period, activeManagers, includeRemote);
        }

        public static int SendManagerSummaryReport(DateTime period, IEnumerable<ClientItem> managers, bool includeRemote)
        {
            int managerSummaryReportEmailPreferenceId = 1;

            var prefs = DA.Current.Query<ClientEmailPreference>().Where(x => x.EmailPreferenceID == managerSummaryReportEmailPreferenceId).ToList();

            var sendTo = new List<ManagerSummaryReportEmailArgs>();

            foreach (var mgr in managers)
            {
                // check if any pref exists
                var exists = prefs.Any(x => x.ClientID == mgr.ClientID);

                if (!exists)
                {
                    // add opt-in pref by default
                    var p = new ClientEmailPreference()
                    {
                        EmailPreferenceID = managerSummaryReportEmailPreferenceId,
                        ClientID = mgr.ClientID,
                        EnableDate = DateTime.Now,
                        DisableDate = null
                    };

                    DA.Current.Insert(p);

                    var model = ReportGenerator.CreateManagerUsageSummary(period, mgr, includeRemote);

                    if (model.AccountItems.Count() > 0)
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

                        var model = ReportGenerator.CreateManagerUsageSummary(period, mgr, includeRemote);

                        if (model.AccountItems.Count() > 0)
                        {
                            sendTo.Add(ManagerSummaryReportEmailArgs.Create(p, model, mgr));
                        }
                    }
                }
            }

            int count = 0;

            string debugEmail = ConfigurationManager.AppSettings["DebugEmail"];

            foreach (var args in sendTo)
            {
                if (args.Model.AccountItems.Count() > 0)
                {
                    string toAddr = string.IsNullOrEmpty(debugEmail) ? args.Manager.Email : debugEmail;

                    var sendResult = Providers.Email.SendMessage(new SendMessageArgs()
                    {
                        ClientID = CacheManager.Current.CurrentUser.ClientID,
                        Caller = "Reports.Models.EmailManager.SendManagerSummaryReport",
                        Subject = string.Format("LNF Manager Usage Summary Report for {0:MMM yyyy}", period),
                        Body = GetManagerSummaryReportBody(args),
                        From = "lnf-system@umich.edu",
                        DisplayName = "LNF System",
                        To = new[] { toAddr },
                        IsHtml = true
                    });

                    if (sendResult.Success)
                        count += 1;
                }
            }

            return count;
        }

        private static string GetManagerSummaryReportBody(ManagerSummaryReportEmailArgs args)
        {
            string setting = ConfigurationManager.AppSettings["TemplateDirec"];

            var model = args.Model;
            var client = args.Manager;
            var width = args.Model.ShowSubsidyColumn ? 900 : 600;

            var unsubscribeUrl = Providers.Context.Current.GetAbsolutePath(string.Format("~/unsubscribe/{0}", Encryption.SHA256(args.Preference.ClientEmailPreferenceID.ToString())));

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

        private static string CreateManagerUsageSummaryTable(IEnumerable<ManagerUsageSummaryItem> items, string header, bool showSubsidyColumn)
        {
            string result = string.Empty;

            result += "<table border=\"1\" width=\"900\" cellpadding=\"5\" bordercolor=\"#cccccc\" style=\"margin-top: 20px; border-collapse: collapse;\">";
            result += "<thead>";
            result += "<tr bgcolor=\"#eeeeee\">";
            result += string.Format("<th>{0}</th>", header);
            if (showSubsidyColumn)
            {
                result += "<th width=\"150\">Net Charges</th>";
                result += "<th width=\"150\">Usage Charges</th>";
                result += "<th width=\"150\">Subsidy</th>";
            }
            else
            {
                result += "<th>Usage Charges</th>";
            }
            result += "</tr>";
            result += "</thead>";
            result += "<tbody>";
            foreach (var item in items)
            {
                result += "<tr>";
                result += string.Format("<td>{0}</td>", item.Name);
                if (showSubsidyColumn)
                {
                    result += string.Format("<td align=\"right\">{0:C}</td>", item.NetCharge);
                    result += string.Format("<td align=\"right\">{0:C}</td>", item.UsageCharge);
                    result += string.Format("<td align=\"right\">{0:C}</td>", item.Subsidy);
                }
                else
                {
                    result += string.Format("<td align=\"right\">{0:C}</td>", item.UsageCharge);
                }
                result += "</tr>";
            }
            result += "</tbody>";
            result += "<tfoot>";
            result += "<tr bgcolor=\"#eeeeee\">";
            result += "<td><b>Total</b></td>";
            if (showSubsidyColumn)
            {
                result += string.Format("<td align=\"right\"><b>{0:C}</b></td>", items.Sum(x => x.NetCharge));
                result += string.Format("<td align=\"right\"><b>{0:C}</b></td>", items.Sum(x => x.UsageCharge));
                result += string.Format("<td align=\"right\"><b>{0:C}</b></td>", items.Sum(x => x.Subsidy));
            }
            else
            {
                result += string.Format("<td align=\"right\"><b>{0:C}</b></td>", items.Sum(x => x.UsageCharge));
            }
            result += "</tr>";
            result += "</tfoot>";
            result += "</table>";

            return result;
        }
    }

    public struct ManagerSummaryReportEmailArgs
    {
        public static ManagerSummaryReportEmailArgs Create(ClientEmailPreference pref, ManagerUsageSummary model, ClientItem manager)
        {
            var result = new ManagerSummaryReportEmailArgs();
            result.Preference = pref;
            result.Model = model;
            result.Manager = manager;
            return result;
        }

        public ClientEmailPreference Preference { get; private set; }
        public ManagerUsageSummary Model { get; private set; }
        public ClientItem Manager { get; private set; }
    }
}