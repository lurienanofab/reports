using LNF.Cache;
using LNF.CommonTools;
using LNF.Models.Data;
using LNF.Repository;
using LNF.Repository.Data;
using LNF.Repository.Reporting;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Reports.Controllers
{
    public class HomeController : Controller
    {
        [Route("")]
        public ActionResult Index()
        {
            ViewBag.IsAdmin = CacheManager.Current.CurrentUser.HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Developer);
            return View();
        }

        [Route("unsubscribe/{hash}")]
        public ActionResult Unsubscribe(string hash)
        {
            var prefs = DA.Current.Query<ClientEmailPreference>().Where(x => x.DisableDate == null).ToList();
            var p = prefs.FirstOrDefault(x => hash == Encryption.SHA256(x.ClientEmailPreferenceID.ToString()));

            string message = string.Empty;
            string errmsg = string.Empty;

            if (p != null)
            {
                p.DisableDate = DateTime.Now;
                var item = DA.Current.Single<EmailPreference>(p.EmailPreferenceID);
                var c = DA.Current.Query<ClientInfo>().First(x => x.ClientID == p.ClientID);
                message = string.Format("Your email <strong>{0}</strong> has been successfully unsubscribed. You will no longer receive emails for <strong>{1}</strong>", c.Email, item.ReportName);
            }
            else
                errmsg = "You are not currenlty subscribed. If you have any questions please contact lnf-support@umich.edu";

            ViewBag.Message = message;
            ViewBag.ErrorMessage = errmsg;

            return View();
        }
    }
}