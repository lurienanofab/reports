using LNF.Models.Data;
using LNF.Reporting;
using LNF.Repository;
using LNF.Repository.Reporting;
using LNF.Web;
using Reports.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Reports.Controllers
{
    public class PreferencesController : Controller
    {
        [HttpGet, Route("preferences/email")]
        public ActionResult Email()
        {
            var model = new EmailPreferenceModel
            {
                ClientID = HttpContext.CurrentUser().ClientID,
                DisplayName = HttpContext.CurrentUser().DisplayName,
                AvailableClients = ClientItemUtility.SelectCurrentActiveClients()
            };

            model.AvailableItems = EmailPreferenceItem.Select(model.ClientID);
            model.CanSelectUser = HttpContext.CurrentUser().HasPriv(ClientPrivilege.Staff);

            return View(model);
        }

        [HttpPost, Route("preferences/email")]
        public ActionResult Email(EmailPreferenceModel model)
        {
            model.AvailableClients = ClientItemUtility.SelectCurrentActiveClients();
            model.AvailableItems = EmailPreferenceItem.Select(model.ClientID);
            model.CanSelectUser = HttpContext.CurrentUser().HasPriv(ClientPrivilege.Staff);

            if (model.Command == "save")
            {
                var currentPrefs = DA.Current.Query<ClientEmailPreference>().Where(x => x.ClientID == model.ClientID).ToList();

                foreach (var item in model.AvailableItems)
                {
                    bool isChecked = model.SelectedItems.Any(x => x == item.EmailPreferenceID);

                    item.Active = isChecked;

                    var pref = currentPrefs.FirstOrDefault(x => x.EmailPreferenceID == x.EmailPreferenceID && x.DisableDate == null);

                    if (isChecked)
                    {
                        if (pref == null)
                            DA.Current.Insert(new ClientEmailPreference()
                            {
                                EmailPreferenceID = item.EmailPreferenceID,
                                ClientID = model.ClientID,
                                EnableDate = DateTime.Now,
                                DisableDate = null
                            });

                        // nothing to do if isChecked == true and there is already a pref with DisableDate == null
                    }
                    else
                    {
                        if (pref != null)
                            pref.DisableDate = DateTime.Now;

                        // nothing to do if isChecked == false and there is no pref with DisableDate == null
                    }
                }

                ViewBag.Message = "Preferences saved.";
            }

            return View(model);
        }
    }
}