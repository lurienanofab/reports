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

        [HttpPost, Route("admin/email")]
        public ActionResult Email(string command, bool remote = false)
        {
            if (!CacheManager.Current.CurrentUser.HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Developer))
                return RedirectToAction("Index", "Home");

            if (command == "send-manager-summary-report")
            {
                DateTime priorPeriod = DateTime.Now.FirstOfMonth().AddMonths(-1);

                int count = EmailManager.SendManagerSummaryReport(priorPeriod, remote);
                ViewBag.Message = string.Format("The Manager Summary Report for the period <strong>{0:MMM yyyy}</strong> has been sent to <strong>{1} users</strong>.", priorPeriod, count);
            }

            return View();
        }
    }
}