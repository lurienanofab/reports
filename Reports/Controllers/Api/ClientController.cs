using LNF.Models.Reporting;
using LNF.Reporting;
using System;
using System.Collections.Generic;
using System.Web.Http;
using LNF;

namespace Reports.Controllers.Api
{
    public class ClientController : ApiController
    {
        [Route("api/client")]
        public IEnumerable<IReportingClient> GetClients(DateTime period)
        {
            return ServiceProvider.Current.Reporting.ClientItem.SelectActiveClients(period);
        }

        [Route("api/client/manager")]
        public IEnumerable<IReportingClient> GetManagers(DateTime period)
        {
            return ServiceProvider.Current.Reporting.ClientItem.SelectActiveManagers(period);
        }
    }
}
