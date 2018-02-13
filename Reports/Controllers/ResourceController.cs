using LNF;
using LNF.Billing;
using LNF.CommonTools;
using LNF.Models.Data;
using LNF.Models.Scheduler;
using LNF.PhysicalAccess;
using LNF.Repository;
using LNF.Repository.Scheduler;
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
    public class ResourceController : Controller
    {
        private static readonly string[] roomNames = { "Clean Room", "Wet Chemistry" };

        private static readonly Dictionary<string, string> roomDisplayNames = new Dictionary<string, string>()
        {
            { "Clean Room", "Clean Room" },
            { "Wet Chemistry", "ROBIN" }
        };

        [Route("resource")]
        public ActionResult Index()
        {
            return View();
        }

        [Route("resource/durations")]
        public ActionResult Durations(DurationsModel model)
        {
            model.Resources = DA.Current.Query<Resource>().Where(x => x.IsActive).OrderBy(x => x.ResourceName).Model<ResourceModel>();

            if (model.ReservationID > 0)
            {
                var rsv = DA.Current.Single<Reservation>(model.ReservationID);

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

            ReservationDateRange.DateRange range = default(ReservationDateRange.DateRange);

            if (model.SelectedResource != null)
            {
                var sd = model.GetStartDateTime();
                var ed = model.GetEndDateTime();
                var resourceId = model.SelectedResource.ResourceID;

                range = ReservationDateRange.ExpandRange(resourceId, sd, ed);
            }

            model.Range = range;

            return View(model);
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
                    
                    var feed = JsonConvert.DeserializeObject<DataFeedModel<ToolUsageSummaryItem>>(json);

                    var items = feed.Data["default"];

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
            RssFeed rss = new RssFeed();

            rss.Channel = new RssChannel();
            rss.Version = "2.0";
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
            RssFeed rss = new RssFeed();

            rss.Channel = new RssChannel();
            rss.Version = "2.0";
            rss.Channel.Title = "Lab Activity";
            rss.Channel.PubDate = DateTime.Now;
            rss.Channel.LastBuildDate = DateTime.Now;
            rss.Channel.WebMaster = "lnf-it@umich.com";
            rss.Channel.Description = "The current status of the LNF.";
            rss.Channel.Link = string.Empty;

            LabStatus labStatus = LabStatus.GetCurrentStatus();

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
            IList<Badge> inlab = Providers.PhysicalAccess.CurrentlyInArea().ToList();
            List<InLabClient> result = inlab
                .Where(x => x.CurrentAreaName == areaName)
                .Select(x => new InLabClient(x))
                .OrderBy(x => x.LastName)
                .ToList();
            return result;
        }
    }
}