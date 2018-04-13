using LNF.Billing;
using LNF.CommonTools;
using LNF.Models.Reporting;
using LNF.Models.Reporting.Individual;
using LNF.Models.Scheduler;
using LNF.Reporting;
using LNF.Repository;
using LNF.Repository.Data;
using LNF.Repository.Scheduler;
using LNF.Scheduler;
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
        [Obsolete]
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
            Client mgr = null;

            if (!string.IsNullOrEmpty(username))
            {
                mgr = DA.Current.Query<Client>().FirstOrDefault(x => x.UserName == username);

                if (mgr == null)
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "Manager not found."));
            }

            if (!new[] { "xml", "json" }.Contains(format))
            {
                throw new ArgumentException("Format must be 'xml' or 'json'.", "format");
            }
            else
            {
                var items = ReportGenerator.GetManagerUsageDetailItems(sd, ed, mgr, remote);

                if (format == "xml")
                {
                    var xdoc = ReportGenerator.GetManagerUsageDetailXml(items);

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(xdoc.ToString(), Encoding.UTF8, "text/xml")
                    };
                }
                else
                {
                    var json = ReportGenerator.GetManagerUsageDetailJson(items);

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
            ClientItem mgr = ClientItemUtility.CreateClientItems(DA.Current.Query<ClientInfo>().Where(x => x.UserName == username)).FirstOrDefault();

            if (mgr == null)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "Manager not found."));

            return ReportGenerator.CreateManagerUsageSummary(period, mgr, remote);
        }

        [Route("api/report/manager-usage-summary")]
        public ManagerUsageSummary GetManagerUsageSummary(DateTime period, int clientId, bool remote = false)
        {
            ClientItem mgr = ClientItemUtility.CreateClientItems(DA.Current.Query<ClientInfo>().Where(x => x.ClientID == clientId)).FirstOrDefault();

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

                ClientItem mgr = ClientItemUtility.CreateClientItems(DA.Current.Query<ClientAccountInfo>()
                        .Where(x => x.ClientID == model.ClientID && x.EmailRank == 1))
                    .Distinct(comparer)
                    .FirstOrDefault();

                int count = Models.EmailManager.SendManagerSummaryReport(model.CurrentUserClientID, model.Period, new[] { mgr }, model.Message, model.CCAddress, model.Debug, model.IncludeRemote);

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
            var user = ClientItemUtility.CreateClientItems(DA.Current.Query<ClientInfo>().Where(x => x.UserName == username)).FirstOrDefault();

            if (user != null)
                return ReportGenerator.CreateUserUsageSummary(period, user);
            else
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "Manager not found."));
        }

        [Route("api/report/user-usage-summary")]
        public UserUsageSummary GetUserUsageSummary(DateTime period, int clientId)
        {
            var user = ClientItemUtility.CreateClientItems(DA.Current.Query<ClientInfo>().Where(x => x.ClientID == clientId)).FirstOrDefault();

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
}
