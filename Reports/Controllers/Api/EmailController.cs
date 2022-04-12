using LNF;
using Reports.Models;
using System.Collections.Generic;
using System.Web.Http;

namespace Reports.Controllers.Api
{
    public class EmailController : ReportApiController
    {
        public EmailController(IProvider provider) : base(provider) { }

        [Route("api/email")]
        public IEnumerable<EmailPreferenceItem> GetEmailPreferenceItems(int clientId)
        {
            return EmailPreferenceItemFactory.Create(Provider).Select(clientId);
        }
    }
}
