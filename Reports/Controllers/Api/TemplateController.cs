using LNF.Repository;
using LNF.Repository.Reporting;
using Reports.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace Reports.Controllers.Api
{
    public class TemplateController : ApiController
    {
        [Route("api/template")]
        public int Put([FromBody] TemplateModel model)
        {
            var template = new Template();
            template.TemplateName = model.TemplateName;
            template.TemplateContent = model.TemplateContent;
            DA.Current.Insert(template);
            return template.TemplateID;
        }

        [Route("api/template")]
        public int Post([FromBody] TemplateModel model)
        {
            var template = DA.Current.Single<Template>(model.TemplateID);

            if (template != null)
            {
                template.TemplateContent = model.TemplateContent;
                return template.TemplateID;
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }

        [Route("api/template")]
        public bool Delete(int templateId)
        {
            var template = DA.Current.Single<Template>(templateId);

            if (template != null)
            {
                DA.Current.Delete(template);
                return true;
            }
            else
            {
                return false;
            }
        }

        [Route("api/template")]
        public IEnumerable<TemplateModel> Get(string report)
        {
            return DA.Current.Query<Template>().Where(x => x.Report == report).Model<TemplateModel>();
        }
    }
}
