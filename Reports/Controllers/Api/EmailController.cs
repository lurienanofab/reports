using Reports.Models;
using System.Collections.Generic;
using System.Web.Http;
using System;

namespace Reports.Controllers.Api
{
    public class EmailController : ApiController
    {
        [Route("api/email")]
        public IEnumerable<EmailPreferenceItem> GetEmailPreferenceItems(int clientId)
        {
            return EmailPreferenceItem.Select(clientId);
        }
    }
}
