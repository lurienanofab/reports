using LNF;
using LNF.CommonTools;
using LNF.Data;
using LNF.Impl.Repository.Data;
using LNF.Impl.Repository.Reporting;
using Reports.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Reports.Controllers
{
    public class HomeController : ReportsController
    {
        public HomeController(IProvider provider) : base(provider) { }

        [Route("")]
        public ActionResult Index()
        {
            ViewBag.IsAdmin = CurrentUser.HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Developer);
            return View();
        }

        [Route("unsubscribe/{hash}")]
        public ActionResult Unsubscribe(string hash)
        {
            var prefs = Repository.Query<ClientEmailPreference>().Where(x => x.DisableDate == null).ToList();
            var p = prefs.FirstOrDefault(x => hash == Encryption.SHA256.EncryptText(x.ClientEmailPreferenceID.ToString()));

            string message = string.Empty;
            string errmsg = string.Empty;

            if (p != null)
            {
                p.DisableDate = DateTime.Now;
                var item = Repository.Single<EmailPreference>(p.EmailPreferenceID);
                var c = Repository.Query<ClientInfo>().First(x => x.ClientID == p.ClientID);
                message = string.Format("Your email <strong>{0}</strong> has been successfully unsubscribed. You will no longer receive emails for <strong>{1}</strong>", c.Email, item.ReportName);
            }
            else
                errmsg = "You are not currenlty subscribed. If you have any questions please contact lnf-support@umich.edu";

            ViewBag.Message = message;
            ViewBag.ErrorMessage = errmsg;

            return View();
        }

        [Route("dispatch/{name?}")]
        public ActionResult Dispatch(string name = null, string returnTo = null)
        {
            string action;
            string controller;

            object routeValues = null;

            switch (name)
            {
                case "durations-report":
                    action = "Durations";
                    controller = "Resource";

                    if (int.TryParse(Request.QueryString["ReservationID"], out int reservationId))
                    {
                        routeValues = new { ReservationID = reservationId };
                    }

                    break;
                case "manager-usage-summary":
                    action = "ManagerUsageSummary";
                    controller = "Individual";
                    break;
                case "manager-usage-emails":
                    action = "Email";
                    controller = "Admin";
                    break;
                case "all-tool-usage-summary":
                    action = "ToolUsageSummary";
                    controller = "Resource";
                    routeValues = new { resource = "all" };
                    break;
                default:
                    Session.Remove("return-to");
                    return RedirectToAction("Index");
            }

            Session["return-to"] = returnTo;

            return RedirectToAction(action, controller, routeValues);
        }

        [Route("return")]
        public ActionResult Return()
        {
            if (string.IsNullOrEmpty(Convert.ToString(Session["return-to"])))
                return RedirectToAction("Index");
            else
                return Redirect(Convert.ToString(Session["return-to"]));
        }

        [Route("template/{report}/{name}")]
        public ActionResult Template(string report, string name)
        {
            try
            {
                var templateContent = TemplateManager.GetTemplate(report, name);
                return Content(templateContent, "text/x-handlebars-template");
            }
            catch (TemplateNotFoundException ex)
            {
                return new HttpNotFoundResult(ex.Message);
            }
        }
    }
}