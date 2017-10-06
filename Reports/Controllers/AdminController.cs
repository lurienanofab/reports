using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LNF.Cache;
using LNF.Models.Data;
using Reports.Models;
using LNF.CommonTools;

namespace Reports.Controllers
{
    public class AdminController : Controller
    {
        [HttpGet, Route("admin/email")]
        public ActionResult Email()
        {
            if (!CacheManager.Current.CurrentUser.HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Developer))
                return RedirectToAction("Index", "Home");

            return View();
        }

        [Route("admin/email/ajax/manager-usage-summary/recipients")]
        public ActionResult GetManagerUsageSummaryEmailRecipients(string group, DateTime period, bool remote = false)
        {
            var currentUserClientId = CacheManager.Current.CurrentUser.ClientID;
            var emails = GetManagerUsageSummaryEmails(group, period, currentUserClientId, remote);
            var result = emails.Select(x => new { Name = x.RecipientName, Email = x.RecipientEmail });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Route("admin/email/ajax/manager-usage-summary/send")]
        public ActionResult SendManagerUsageSummaryEmails(string group, DateTime period, string message, string ccaddr, bool debug, bool remote = false)
        {
            var currentUserClientId = CacheManager.Current.CurrentUser.ClientID;
            var emails = GetManagerUsageSummaryEmails(group, period, currentUserClientId, remote);
            var count = EmailManager.SendManagerSummaryReport(currentUserClientId, emails, message, ccaddr, debug);
            var result = new { message = string.Format("Manager Usage Summary emails sent: {0}", count) };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Route("admin/templates")]
        public ActionResult Templates()
        {
            return View();
        }

        private IEnumerable<EmailMessage> GetManagerUsageSummaryEmails(string group, DateTime period, int currentUserClientId, bool remote)
        {
            var key = string.Format("manager-usage-summary-emails-{0}-{1:yyyyMMdd}{2}", group, period, remote ? "-remote" : string.Empty);

            IEnumerable<EmailMessage> emails;

            if (Session[key] != null)
            {
                emails = (IEnumerable<EmailMessage>)Session[key];
            }
            else
            {
                var managers = EmailManager.GetManagers(currentUserClientId, group, period, remote);
                emails = EmailManager.CreateManagerUsageSummaryEmails(currentUserClientId, period, managers, remote).ToList();
                Session[key] = emails;
            }

            return emails;
        }
    }
}