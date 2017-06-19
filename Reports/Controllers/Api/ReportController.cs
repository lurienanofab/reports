using LNF.Models.Reporting;
using LNF.Models.Reporting.Individual;
using LNF.Reporting;
using LNF.Repository;
using LNF.Repository.Data;
using Reports.Models;
using System;
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

                int count = EmailManager.SendManagerSummaryReport(model.Period, new[] { mgr }, model.IncludeRemote);

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
    }
}
