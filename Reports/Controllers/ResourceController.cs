using LNF;
using LNF.Billing;
using LNF.CommonTools;
using LNF.Data;
using LNF.Impl;
using LNF.Impl.Repository.Scheduler;
using LNF.PhysicalAccess;
using LNF.Repository;
using LNF.Scheduler;
using Newtonsoft.Json;
using Reports.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Reports.Controllers
{
    public class ResourceController : ReportsController
    {
        private static readonly string[] roomNames = { "Clean Room", "Wet Chemistry" };

        private static readonly Dictionary<string, string> roomDisplayNames = new Dictionary<string, string>()
        {
            { "Clean Room", "Clean Room" },
            { "Wet Chemistry", "ROBIN" }
        };

        public ResourceController(IProvider provider) : base(provider) { }

        [Route("resource")]
        public ActionResult Index()
        {
            return View();
        }

        [Route("resource/durations")]
        public ActionResult Durations(DurationsModel model)
        {
            model.Resources = Repository.Query<Resource>().Where(x => x.IsActive).OrderBy(x => x.ResourceName).CreateModels<IResource>();

            if (model.ReservationID > 0)
            {
                var rsv = Repository.Single<Reservation>(model.ReservationID);

                if (rsv != null)
                {
                    model.StartDate = rsv.ChargeBeginDateTime();
                    model.EndDate = rsv.ChargeEndDateTime();
                    model.ResourceID = rsv.Resource.ResourceID;
                }
                else
                {
                    model.ReservationID = 0;
                    ModelState["ReservationID"].Value = new ValueProviderResult("0", "0", CultureInfo.CurrentCulture);
                }
            }

            DateRange range = default(DateRange);

            if (model.SelectedResource != null)
            {
                var sd = model.GetStartDateTime();
                var ed = model.GetEndDateTime();
                var resourceId = model.SelectedResource.ResourceID;
                var reservations = Provider.Scheduler.Reservation.GetReservations(sd, ed, resourceId: resourceId);
                range = DateRange.ExpandRange(reservations, sd, ed);
            }

            model.Range = range;

            return View(model);
        }

        [Route("resource/reservation-states")]
        public ActionResult ReservationStates(DateTime? sd = null, DateTime? ed = null, int? rid = null, int? cid = null, int? reserver = null, bool inlab = true, string run = null)
        {
            var startDate = sd.GetValueOrDefault(DateTime.Now.Date.AddDays(-2));
            var endDate = ed.GetValueOrDefault(startDate.AddMonths(1));

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.ResourceID = rid;
            ViewBag.ClientID = cid;
            ViewBag.Reserver = reserver;
            ViewBag.InLab = inlab;
            ViewBag.RunReport = run == "report";

            var activeClients = Provider.Data.Client.GetActiveClients(startDate, endDate, ClientPrivilege.LabUser | ClientPrivilege.Staff)
                .OrderBy(x => x.LName)
                .ThenBy(x => x.FName)
                .Select(x => new ClientItem()
                {
                    ClientID = x.ClientID,
                    Privs = x.Privs,
                    LName = x.LName,
                    FName = x.FName
                }).ToList();

            ViewBag.ActiveClients = activeClients;

            var resources = Repository.Query<Resource>().Where(x => x.IsActive).OrderBy(x => x.ResourceName).Select(x => new ResourceListItem()
            {
                ResourceID = x.ResourceID,
                ResourceName = x.ResourceName
            }).ToList();

            ViewBag.Resources = resources;

            var resourceListItems = resources.Select(x => new SelectListItem()
            {
                Text = x.ResourceName,
                Value = x.ResourceID.ToString(),
                Selected = x.ResourceID == rid.GetValueOrDefault()
            }).ToList();

            var currentUserListItems = activeClients.Select(x => new SelectListItem()
            {
                Text = Clients.GetDisplayName(x.LName, x.FName),
                Value = x.ClientID.ToString(),
                Selected = x.ClientID == cid.GetValueOrDefault()
            }).ToList();

            var reserverListItems = activeClients.Select(x => new SelectListItem()
            {
                Text = Clients.GetDisplayName(x.LName, x.FName),
                Value = x.ClientID.ToString(),
                Selected = x.ClientID == reserver.GetValueOrDefault()
            }).ToList();

            ViewBag.ResourceListItems = resourceListItems;
            ViewBag.CurrentUserListItems = currentUserListItems;
            ViewBag.ReserverListItems = reserverListItems;

            return View();
        }

        /// <summary>
        /// This method exists for backwards compatability. The display in the lab entrance uses this route.
        /// </summary>
        [Route("resource/RSS.ashx")]
        public ActionResult InLabRssFeedRedirect(int rid = 0)
        {
            return RedirectToAction("InLab", "Rss", new { rid });
        }

        [Route("resource/rss/inlab")]
        public ActionResult InLabRssFeed(int rid = 0)
        {
            if (rid >= roomNames.Length)
                throw new IndexOutOfRangeException("The parameter rid is out of range.");

            RssFeed rss = PopulateInLabRss(roomNames[rid]);

            return Content(rss.ToString(), "application/rss+xml");
        }

        [Route("resource/rss/lab-status")]
        public ActionResult LabStatusRssFeed()
        {
            RssFeed rss = PopulateLabActivityRss();
            return Content(rss.ToString(), "application/rss+xml");
        }

        [Route("resource/tool-usage-summary/{resource?}")]
        public async Task<ActionResult> ToolUsageSummary(string resource = "", string command = null, int? m = null, int? y = null, int? n = null, string a = null)
        {
            ViewBag.Resource = resource;

            int startYear;
            int startMonth;
            DateTime startDate;
            DateTime endDate;
            DateTime defaultStartDate = DateTime.Now.FirstOfMonth().AddMonths(-1);

            if (y.HasValue)
                startYear = y.Value;
            else
                startYear = defaultStartDate.Year;

            if (m.HasValue)
                startMonth = m.Value;
            else
                startMonth = defaultStartDate.Month;

            startDate = new DateTime(startYear, startMonth, 1);

            ViewBag.StartDate = startDate;

            if (n.HasValue)
            {
                ViewBag.Months = n.Value;
                endDate = startDate.AddMonths(n.Value);
            }
            else
            {
                ViewBag.Months = 1;
                endDate = startDate.AddMonths(1);
            }

            int[] accountTypes;

            if (!string.IsNullOrEmpty(a))
                accountTypes = a.Split(' ').Select(x => Convert.ToInt32(x)).ToArray();
            else
                accountTypes = new int[] { 1 };

            ViewBag.AccountTypes = accountTypes;

            ViewBag.Command = command;

            if (command == "view")
            {
                using (var hc = new HttpClient())
                {
                    hc.BaseAddress = new Uri(GetFeedBaseUrl());

                    string feedUri = string.Format("data/feed/tool-usage-summary/json?opt={0}&sd={1:yyyy-MM-dd}&ed={2:yyyy-MM-dd}&rid={3}&at={4}",
                        "Combined", startDate, endDate, null, string.Join(",", accountTypes));

                    var msg = await hc.GetAsync(feedUri);

                    msg.EnsureSuccessStatusCode();

                    var json = await msg.Content.ReadAsStringAsync();

                    var feed = JsonConvert.DeserializeObject<DataFeedResult>(json);
                    // DataFeedModel<ToolUsageSummaryItem>>

                    //DataFeedResultItemCollection items = feed.Data["default"];

                    var converter = new ToolUsageSummaryItemConverter();

                    var items = feed.Data.Items(converter);

                    ViewBag.Items = items;
                }
            }
            else if (command == "export")
            {
                string feedRedirectUrl = GetFeedBaseUrl() + string.Format("data/feed/tool-usage-summary/csv?opt={0}&sd={1:yyyy-MM-dd}&ed={2:yyyy-MM-dd}&rid={3}&at={4}",
                    "Combined", startDate, endDate, null, string.Join(",", accountTypes));

                return Redirect(feedRedirectUrl);
            }

            return View();
        }

        private RssFeed PopulateInLabRss(string roomName, string channelName = "", string userName = "")
        {
            RssFeed rss = new RssFeed
            {
                Channel = new RssChannel(),
                Version = "2.0"
            };

            rss.Channel.Title = GetRoomDisplayName(roomName);
            rss.Channel.PubDate = DateTime.Now;
            rss.Channel.LastBuildDate = DateTime.Now;
            rss.Channel.WebMaster = "lnf-it@umich.com";
            rss.Channel.Description = "Users currently in one of the LNF lab areas";
            rss.Channel.Link = GetFullUrl("/sselresreports/AccInLab.aspx");

            if (!string.IsNullOrEmpty(channelName))
            {
                rss.Channel.Title += " '" + channelName + "'";
                if (!string.IsNullOrEmpty(userName))
                    rss.Channel.Title += string.Format(" (generated for {0})", userName);
            }

            rss.Channel.Items = GetCurrentUsersInRoom(roomName).Select(CreateInLabRssItem).ToList();

            return rss;
        }

        private RssFeed PopulateLabActivityRss()
        {
            RssFeed rss = new RssFeed
            {
                Channel = new RssChannel(),
                Version = "2.0"
            };

            rss.Channel.Title = "Lab Activity";
            rss.Channel.PubDate = DateTime.Now;
            rss.Channel.LastBuildDate = DateTime.Now;
            rss.Channel.WebMaster = "lnf-it@umich.com";
            rss.Channel.Description = "The current status of the LNF.";
            rss.Channel.Link = string.Empty;

            LabStatus labStatus = LabStatus.GetCurrentStatus(Provider);

            rss.Channel.Items = new List<RssItem>();

            foreach (var item in labStatus.RoomOccupancies)
            {
                var room = GetRoomDisplayName(item.RoomName);

                rss.Channel.Items.Add(new RssItem()
                {
                    Description = string.Format("{0} users in {1}", item.Occupancy, room),
                    Guid = Guid.NewGuid().ToString("n"),
                    Link = string.Empty,
                    PubDate = labStatus.StatusDateTime,
                    Title = string.Format("Current Occupancy in {0}", room)
                });
            }

            rss.Channel.Items.Add(new RssItem()
            {
                Description = string.Format("{0} active reservations", labStatus.ActiveReservations.Count()),
                Guid = Guid.NewGuid().ToString("n"),
                Link = string.Empty,
                PubDate = labStatus.StatusDateTime,
                Title = "Active reservations"
            });

            return rss;
        }

        private string GetFeedBaseUrl()
        {
            return ConfigurationManager.AppSettings["FeedBaseUrl"];
        }

        private string GetRoomDisplayName(string key)
        {
            if (roomDisplayNames.ContainsKey(key))
                return roomDisplayNames[key];
            else
                return key;
        }

        private string GetFullUrl(string virtualPath)
        {
            string result = string.Empty;
            string absolutePath = VirtualPathUtility.ToAbsolute(virtualPath);
            result += Request.Url.GetLeftPart(UriPartial.Authority);
            result += absolutePath;
            return result;
        }

        private RssItem CreateInLabRssItem(InLabClient inLabClient)
        {
            return new RssItem()
            {
                Title = inLabClient.FullName,
                Description = string.Format("{0:#.00 hours}", inLabClient.Duration),
                Link = inLabClient.CardNumber,
                PubDate = inLabClient.EventDateTime,
                Guid = inLabClient.ClientID.ToString()
            };
        }

        private IList<InLabClient> GetCurrentUsersInRoom(string areaName)
        {
            IList<Badge> inlab = Provider.PhysicalAccess.GetCurrentlyInArea("all").ToList();

            List<InLabClient> result = inlab
                .Where(x => x.CurrentAreaName == areaName)
                .Select(x => new InLabClient(x))
                .OrderBy(x => x.LastName)
                .ToList();

            return result;
        }
    }
}