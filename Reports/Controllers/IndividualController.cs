using LNF;
using LNF.CommonTools;
using LNF.Data;
using System;
using System.Web.Mvc;

namespace Reports.Controllers
{
    public class IndividualController : ReportsController
    {
        public IndividualController(IProvider provider) : base(provider) { }

        [Route("individual")]
        public ActionResult Index()
        {
            return View();
        }

        [Route("individual/manager-usage-summary")]
        public ActionResult ManagerUsageSummary(int clientId = 0, DateTime? period = null)
        {
            bool isAdmin = CurrentUser.HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Developer);

            ViewBag.CanEmail = isAdmin;
            ViewBag.CanSelectUser = isAdmin;

            if (clientId == 0)
            {
                ViewBag.ClientID = CurrentUser.ClientID;
                ViewBag.DisplayName = CurrentUser.DisplayName;
            }
            else
            {
                var c = Provider.Reporting.ClientItem.CreateClientItem(clientId);
                ViewBag.ClientID = c.ClientID;
                ViewBag.DisplayName = Clients.GetDisplayName(c.LName, c.FName);
            }

            if (period == null)
                ViewBag.Period = DateTime.Now.FirstOfMonth().AddMonths(-1);
            else
                ViewBag.Period = period.Value;

            ViewBag.CurrentUserClientID = CurrentUser.ClientID;
            ViewBag.CurrentUserEmail = CurrentUser.Email;

            return View();
        }

        [Route("individual/user-usage-summary")]
        public ActionResult UserUsageSummary(int clientId = 0, DateTime? period = null)
        {
            bool isAdmin = CurrentUser.HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Developer);

            ViewBag.CanEmail = isAdmin;
            ViewBag.CanSelectUser = isAdmin;

            if (clientId == 0 || !isAdmin)
            {
                ViewBag.ClientID = CurrentUser.ClientID;
                ViewBag.DisplayName = CurrentUser.DisplayName;
            }
            else
            {
                var c = Provider.Reporting.ClientItem.CreateClientItem(clientId);
                ViewBag.ClientID = c.ClientID;
                ViewBag.DisplayName = Clients.GetDisplayName(c.LName, c.FName);
            }

            if (period == null)
                ViewBag.Period = DateTime.Now.FirstOfMonth().AddMonths(-1);
            else
                ViewBag.Period = period.Value;

            return View();
        }
    }
}