using LNF;
using LNF.Billing;
using LNF.CommonTools;
using LNF.Data;
using LNF.Reporting;
using LNF.Reporting.Individual;
using Reports.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Hosting;
using System.Web.Http;

namespace Reports.Controllers.Api
{
    public class ReportController : ReportApiController
    {
        public EmailManager EmailManager { get; }

        public ReportController(IProvider provider) : base(provider)
        {
            ;
            EmailManager = new EmailManager(provider);
        }

        [Route("api/report/monthly-usage-detail")]
        public HttpResponseMessage GetManagerUsageDetail(DateTime sd, DateTime ed, string username, bool remote = false)
        {
            return GetManagerUsageDetail(sd, ed, username, remote, "xml");
        }

        [Route("api/report/manager-usage-detail/{format?}")]
        public HttpResponseMessage GetManagerUsageDetail(string username = null, bool remote = false, string format = "json")
        {
            DateTime ed = DateTime.Now.Date;
            DateTime fom = ed.FirstOfMonth();
            DateTime sd = fom.AddYears(-1);
            return GetManagerUsageDetail(sd, ed, username, remote, format);
        }

        [Route("api/report/manager-usage-detail/{format?}")]
        public HttpResponseMessage GetManagerUsageDetail(DateTime sd, DateTime ed, string username = null, bool remote = false, string format = "json")
        {
            IClient mgr = null;

            if (!string.IsNullOrEmpty(username))
            {
                mgr = Provider.Data.Client.GetClient(username);

                if (mgr == null)
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "Manager not found."));
            }

            if (!new[] { "xml", "json" }.Contains(format))
            {
                throw new ArgumentException("Format must be 'xml' or 'json'.", "format");
            }
            else
            {
                var generator = new ReportGenerator(Provider);
                var items = generator.GetManagerUsageDetailItems(sd, ed, mgr, remote);

                if (format == "xml")
                {
                    var xdoc = generator.GetManagerUsageDetailXml(items);

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(xdoc.ToString(), Encoding.UTF8, "text/xml")
                    };
                }
                else
                {
                    var json = generator.GetManagerUsageDetailJson(items);

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json")
                    };
                }
            }

        }

        [Route("api/report/manager-usage-summary")]
        public ManagerUsageSummary GetManagerUsageSummary(DateTime period, string username, bool remote = false)
        {
            var generator = new ReportGenerator(Provider);

            IReportingClient mgr = null;

            var c = Provider.Data.Client.GetClient(username);

            if (c != null)
                mgr = Provider.Reporting.ClientItem.CreateClientItem(c.ClientID);

            if (mgr == null)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "Manager not found."));

            return generator.CreateManagerUsageSummary(period, mgr, remote);
        }

        [Route("api/report/manager-usage-summary")]
        public ManagerUsageSummary GetManagerUsageSummary(DateTime period, int clientId, bool remote = false)
        {
            var generator = new ReportGenerator(Provider);

            IReportingClient mgr = Provider.Reporting.ClientItem.CreateClientItem(clientId);

            if (mgr != null)
                return generator.CreateManagerUsageSummary(period, mgr, remote);
            else
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "Manager not found."));
        }

        [HttpPost, Route("api/report/manager-usage-summary/email")]
        public EmailReportResult SendManagerUsageSummaryEmail([FromBody] EmailReportModel model)
        {
            try
            {
                IReportingClient mgr = Provider.Reporting.ClientItem.CreateClientItem(model.ClientID);

                int count = EmailManager.SendManagerSummaryReport(model.CurrentUserClientID, model.Period, new[] { mgr }, model.Message, model.CCAddress, model.Debug, model.IncludeRemote);

                return new EmailReportResult() { Count = count, ErrorMessage = null };
            }
            catch (Exception ex)
            {
                return new EmailReportResult() { Count = 0, ErrorMessage = ex.Message };
            }
        }

        [Route("api/report/user-usage-summary")]
        public UserUsageSummary GetUserUsageSummary(DateTime period, string username)
        {
            var generator = new ReportGenerator(Provider);

            var c = Provider.Data.Client.GetClient(username);

            IReportingClient user = null;

            if (c != null)
                user = Provider.Reporting.ClientItem.CreateClientItem(c.ClientID);

            if (user != null)
                return generator.CreateUserUsageSummary(period, user);
            else
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "Manager not found."));
        }

        [Route("api/report/user-usage-summary")]
        public UserUsageSummary GetUserUsageSummary(DateTime period, int clientId)
        {
            var generator = new ReportGenerator(Provider);

            var user = Provider.Reporting.ClientItem.CreateClientItem(clientId);

            if (user != null)
                return generator.CreateUserUsageSummary(period, user);
            else
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "Manager not found."));
        }

        [Route("api/report/duration/{reservationId}/info")]
        public DurationInfo GetDurationInfo(int reservationId)
        {
            var rsv = Provider.Scheduler.Reservation.GetReservation(reservationId);

            if (rsv == null)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Reservation not found with ReservationID = {0}.", reservationId)));

            var sd = rsv.ChargeBeginDateTime;
            var ed = rsv.ChargeEndDateTime;

            var reservations = Provider.Scheduler.Reservation.GetReservations(sd, ed, resourceId: rsv.ResourceID);
            var costs = Provider.Data.Cost.FindToolCosts(rsv.ResourceID, ed);
            var durations = new ReservationDurations(reservations, costs, sd, ed);

            var item = durations[rsv.ResourceID].FirstOrDefault(x => x.Reservation.ReservationID == reservationId);

            DurationInfo result;

            if (item != null)
                result = durations.GetDurationInfo(item.Reservation);
            else
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Empty));

            return result;
        }

        [HttpPost, Route("api/report/duration/info")]
        public IEnumerable<DurationInfo> GetDurationInfos([FromBody] DurationInfoArgs args)
        {
            var reservations = Provider.Scheduler.Reservation.GetReservations(args.StartDate, args.EndDate, resourceId: args.ResourceID);
            var costs = Provider.Data.Cost.FindToolCosts(args.ResourceID, args.EndDate);
            var durations = new ReservationDurations(reservations, costs, args.StartDate, args.EndDate);
            var result = durations[args.ResourceID].Select(x => durations.GetDurationInfo(x.Reservation)).ToArray();
            return result;
        }
    }
}
