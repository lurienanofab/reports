using LNF;
using LNF.Data;
using LNF.Web;
using Reports.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Reports.Controllers
{
    public class AdminController : ReportsController
    {
        public EmailManager EmailManager { get; }

        public AdminController(IProvider provider) : base(provider)
        {
            EmailManager = new EmailManager(provider);
        }

        [HttpGet, Route("admin/email")]
        public ActionResult Email()
        {
            if (!CurrentUser.HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Developer))
                return RedirectToAction("Index", "Home");

            ViewBag.CurrentUserEmail = CurrentUser.Email;

            return View();
        }

        [Route("admin/email/ajax/manager-usage-summary/recipients")]
        public ActionResult GetManagerUsageSummaryEmailRecipients(string group, DateTime period, bool remote = false)
        {
            var currentUserClientId = CurrentUser.ClientID;
            var emails = GetManagerUsageSummaryEmails(group, period, remote);
            var result = emails.Select(x => new { Name = x.RecipientName, Email = x.RecipientEmail });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Route("admin/email/ajax/manager-usage-summary/send")]
        public ActionResult SendManagerUsageSummaryEmails(string group, DateTime period, string message, string ccaddr, bool debug, bool remote = false)
        {
            var currentUserClientId = CurrentUser.ClientID;
            var emails = GetManagerUsageSummaryEmails(group, period, remote);
            var count = EmailManager.SendManagerSummaryReport(currentUserClientId, emails, message, ccaddr, debug);
            var result = new { message = string.Format("Manager Usage Summary emails sent: {0}", count) };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Route("admin/templates")]
        public ActionResult Templates()
        {
            return View();
        }

        private IEnumerable<EmailMessage> GetManagerUsageSummaryEmails(string group, DateTime period, bool remote)
        {
            var key = string.Format("manager-usage-summary-emails-{0}-{1:yyyyMMdd}{2}", group, period, remote ? "-remote" : string.Empty);

            IEnumerable<EmailMessage> emails;

            if (Session[key] != null)
            {
                emails = (IEnumerable<EmailMessage>)Session[key];
            }
            else
            {
                var managers = EmailManager.GetManagers(group, period);
                emails = EmailManager.CreateManagerUsageSummaryEmails(period, managers, remote).ToList();
                Session[key] = emails;
            }

            return emails;
        }
    }
}