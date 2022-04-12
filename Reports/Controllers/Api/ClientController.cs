using LNF;
using LNF.Reporting;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace Reports.Controllers.Api
{
    public class ClientController : ReportApiController
    {
        public ClientController(IProvider provider) : base(provider) { }

        [Route("api/client")]
        public IEnumerable<IReportingClient> GetClients(DateTime period)
        {
            return Provider.Reporting.ClientItem.SelectActiveClients(period);
        }

        [Route("api/client/manager")]
        public IEnumerable<IReportingClient> GetManagers(DateTime period)
        {
            return Provider.Reporting.ClientItem.SelectActiveManagers(period);
        }
    }
}
