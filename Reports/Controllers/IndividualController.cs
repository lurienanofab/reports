using LNF.Cache;
using LNF.CommonTools;
using LNF.Models.Data;
using LNF.Reporting;
using System;
using System.Web.Mvc;

namespace Reports.Controllers
{
    public class IndividualController : Controller
    {
        [Route("individual")]
        public ActionResult Index()
        {
            return View();
        }

        [Route("individual/manager-usage-summary")]
        public ActionResult ManagerUsageSummary(int clientId = 0, DateTime? period = null)
        {
            bool isAdmin = CacheManager.Current.CurrentUser.HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Developer);

            ViewBag.CanEmail = isAdmin;
            ViewBag.CanSelectUser = isAdmin;

            if (clientId == 0)
            { 
                ViewBag.ClientID = CacheManager.Current.CurrentUser.ClientID;
                ViewBag.DisplayName = CacheManager.Current.CurrentUser.DisplayName;
            }
            else
            { 
                var c = ClientItemUtility.CreateClientItem(clientId);
                ViewBag.ClientID = c.ClientID;
                ViewBag.DisplayName = ClientItem.GetDisplayName(c.LName, c.FName);
            }

            if (period == null)
                ViewBag.Period = DateTime.Now.FirstOfMonth().AddMonths(-1);
            else
                ViewBag.Period = period.Value;

            return View();
        }

        [Route("individual/user-usage-summary")]
        public ActionResult UserUsageSummary(int clientId = 0, DateTime? period = null)
        {
            bool isAdmin = CacheManager.Current.CurrentUser.HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Developer);

            ViewBag.CanEmail = isAdmin;
            ViewBag.CanSelectUser = isAdmin;

            if (clientId == 0 || !isAdmin)
            { 
                ViewBag.ClientID = CacheManager.Current.CurrentUser.ClientID;
                ViewBag.DisplayName = CacheManager.Current.CurrentUser.DisplayName;
            }
            else
            {
                var c = ClientItemUtility.CreateClientItem(clientId);
                ViewBag.ClientID = c.ClientID;
                ViewBag.DisplayName = ClientItem.GetDisplayName(c.LName, c.FName);
            }

            if (period == null)
                ViewBag.Period = DateTime.Now.FirstOfMonth().AddMonths(-1);
            else
                ViewBag.Period = period.Value;

            return View();
        }
    }
}