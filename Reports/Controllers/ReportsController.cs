using LNF;
using LNF.Data;
using LNF.DataAccess;
using LNF.Web;
using System.Web.Mvc;

namespace Reports.Controllers
{
    public abstract class ReportsController : Controller
    {
        public IProvider Provider { get; }

        public ISession Repository => Provider.DataAccess.Session;

        public IClient CurrentUser => HttpContext.CurrentUser(Provider);

        public ReportsController(IProvider provider)
        {
            Provider = provider;
        }
    }
}