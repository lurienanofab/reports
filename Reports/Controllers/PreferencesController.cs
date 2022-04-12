using LNF;
using LNF.Data;
using LNF.Impl.Repository.Reporting;
using LNF.Repository;
using Reports.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Reports.Controllers
{
    public class PreferencesController : ReportsController
    {
        public PreferencesController(IProvider provider) : base(provider) { }

        [HttpGet, Route("preferences/email")]
        public ActionResult Email()
        {
            var model = new EmailPreferenceModel
            {
                ClientID = CurrentUser.ClientID,
                DisplayName = CurrentUser.DisplayName,
                AvailableClients = Provider.Reporting.ClientItem.SelectCurrentActiveClients()
            };

            model.AvailableItems = EmailPreferenceItemFactory.Create(Provider).Select(model.ClientID);
            model.CanSelectUser = CurrentUser.HasPriv(ClientPrivilege.Staff);

            return View(model);
        }

        [HttpPost, Route("preferences/email")]
        public ActionResult Email(EmailPreferenceModel model)
        {
            model.AvailableClients = Provider.Reporting.ClientItem.SelectCurrentActiveClients();
            model.AvailableItems = EmailPreferenceItemFactory.Create(Provider).Select(model.ClientID);
            model.CanSelectUser = CurrentUser.HasPriv(ClientPrivilege.Staff);

            if (model.Command == "save")
            {
                var currentPrefs = Repository.Query<ClientEmailPreference>().Where(x => x.ClientID == model.ClientID).ToList();

                foreach (var item in model.AvailableItems)
                {
                    bool isChecked = model.SelectedItems.Any(x => x == item.EmailPreferenceID);

                    item.Active = isChecked;

                    var pref = currentPrefs.FirstOrDefault(x => x.EmailPreferenceID == x.EmailPreferenceID && x.DisableDate == null);

                    if (isChecked)
                    {
                        if (pref == null)
                            Repository.Insert(new ClientEmailPreference()
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