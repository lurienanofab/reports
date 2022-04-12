using LNF;
using LNF.DataAccess;
using System.Web.Http;

namespace Reports.Controllers.Api
{
    public abstract class ReportApiController : ApiController
    {
        public IProvider Provider { get; }

        public ISession Session => Provider.DataAccess.Session;

        public ReportApiController(IProvider provider)
        {
            Provider = provider;
        }
    }
}
