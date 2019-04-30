using LNF.Models.Reporting;
using LNF.Reporting;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace Reports.Controllers.Api
{
    public class ClientController : ApiController
    {
        [Route("api/client")]
        public IEnumerable<ReportingClientItem> GetClients(DateTime period)
        {
            return ClientItemUtility.SelectActiveClients(period);
        }

        [Route("api/client/manager")]
        public IEnumerable<ReportingClientItem> GetManagers(DateTime period)
        {
            return ClientItemUtility.SelectActiveManagers(period);
        }
    }
}
