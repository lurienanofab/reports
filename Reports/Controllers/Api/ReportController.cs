using LNF.Billing;
using LNF.Models.Reporting;
using LNF.Models.Reporting.Individual;
using LNF.Reporting;
using LNF.Repository;
using LNF.Repository.Data;
using LNF.Repository.Scheduler;
using Reports.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace Reports.Controllers.Api
{
    public class ReportController : ApiController
    {
        [Route("api/report/monthly-usage-detail")]
        [Route("api/report/manager-usage-detail")]
        public HttpResponseMessage GetManagerUsageDetail(DateTime sd, DateTime ed, string username, bool remote = false)
        {
            var mgr = DA.Current.Query<Client>().FirstOrDefault(x => x.UserName == username);

            if (mgr == null)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "Manager not found."));

            var xdoc = ReportGenerator.GetManagerUsageDetail(sd, ed, mgr, remote);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(xdoc.ToString(), Encoding.UTF8, "text/xml")
            };
        }

        [Route("api/report/manager-usage-summary")]
        public ManagerUsageSummary GetManagerUsageSummary(DateTime period, string username, bool remote = false)
        {
            ClientItem mgr = ClientItemUtility.CreateClientItem(DA.Current.Query<ClientInfo>().FirstOrDefault(x => x.UserName == username));

            if (mgr != null)
                return ReportGenerator.CreateManagerUsageSummary(period, mgr, remote);
            else
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "Manager not found."));
        }

        [Route("api/report/manager-usage-summary")]
        public ManagerUsageSummary GetManagerUsageSummary(DateTime period, int clientId, bool remote = false)
        {
            ClientItem mgr = ClientItemUtility.CreateClientItem(DA.Current.Query<ClientInfo>().FirstOrDefault(x => x.ClientID == clientId));

            if (mgr != null)
                return ReportGenerator.CreateManagerUsageSummary(period, mgr, remote);
            else
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "Manager not found."));
        }

        [HttpPost, Route("api/report/manager-usage-summary/email")]
        public EmailReportResult SendManagerUsageSummaryEmail([FromBody] EmailReportModel model)
        {
            try
            {
                var comparer = new ClientItemEqualityComparer();

                ClientItem mgr = DA.Current.Query<ClientAccountInfo>()
                    .Where(x => x.ClientID == model.ClientID && x.EmailRank == 1)
                    .Select(ClientItemUtility.CreateClientItem)
                    .Distinct(comparer)
                    .FirstOrDefault();

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
            ClientItem user = ClientItemUtility.CreateClientItem(DA.Current.Query<ClientInfo>().FirstOrDefault(x => x.UserName == username));

            if (user != null)
                return ReportGenerator.CreateUserUsageSummary(period, user);
            else
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "Manager not found."));
        }

        [Route("api/report/user-usage-summary")]
        public UserUsageSummary GetUserUsageSummary(DateTime period, int clientId)
        {
            ClientItem user = ClientItemUtility.CreateClientItem(DA.Current.Query<ClientInfo>().FirstOrDefault(x => x.ClientID == clientId));

            if (user != null)
                return ReportGenerator.CreateUserUsageSummary(period, user);
            else
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "Manager not found."));
        }

        [Route("api/report/duration/{reservationId}/info")]
        public ReservationDateRange.DurationInfo GetDurationInfo(int reservationId)
        {
            var rsv = DA.Current.Single<Reservation>(reservationId);

            if (rsv == null)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Reservation not found with ReservationID = {0}.", reservationId)));

            var dr = ReservationDateRange.ExpandRange(rsv.Resource.ResourceID, rsv.ChargeBeginDateTime(), rsv.ChargeEndDateTime());
            var range = new ReservationDateRange(rsv.Resource.ResourceID, dr);
            var durations = range.CreateReservationDurations();
            var item = durations[rsv.Resource.ResourceID].FirstOrDefault(x => x.Reservation.ReservationID == reservationId);

            if (item != null)
                return range.GetDurationInfo(item.Reservation);
            else
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Empty));
        }

        [HttpPost, Route("api/report/duration/info")]
        public IEnumerable<ReservationDateRange.DurationInfo> GetDurationInfos([FromBody] DurationInfoArgs args)
        {
            var range = new ReservationDateRange(args.ResourceID, args.StartDate, args.EndDate);
            var durations = range.CreateReservationDurations();
            var result = durations[args.ResourceID].Select(x => range.GetDurationInfo(x.Reservation)).ToArray();
            return result;
        }
    }

    public class DurationInfoArgs
    {
        public int ResourceID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
