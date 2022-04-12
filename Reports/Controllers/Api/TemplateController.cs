using LNF;
using LNF.Impl;
using LNF.Impl.Repository.Reporting;
using LNF.Repository;
using Reports.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.IO;
using System.Text.RegularExpressions;
using System;

namespace Reports.Controllers.Api
{
    public class TemplateController : ReportApiController
    {
        public TemplateController(IProvider provider) : base(provider) { }

        [Route("api/template")]
        public int Put([FromBody] TemplateModel model)
        {
            var template = new Template()
            {
                TemplateName = model.TemplateName,
                TemplateContent = model.TemplateContent,
                Report = model.Report
            };

            var templatePath = TemplateManager.GetTemplateFilePath(model.Report, model.TemplateName);
            File.WriteAllText(templatePath, model.TemplateContent);

            Session.Insert(template);

            return template.TemplateID;
        }

        [Route("api/template")]
        public int Post([FromBody] TemplateModel model)
        {
            var template = Session.Single<Template>(model.TemplateID);

            if (template != null)
            {
                template.TemplateContent = model.TemplateContent;

                var templatePath = TemplateManager.GetTemplateFilePath(model.Report, model.TemplateName);
                File.WriteAllText(templatePath, model.TemplateContent);

                Session.SaveOrUpdate(template);

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
            var template = Session.Single<Template>(templateId);

            if (template != null)
            {
                var templatePath = TemplateManager.GetTemplateFilePath(template.Report, template.TemplateName);
                File.Delete(templatePath);

                Session.Delete(template);

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
            return Session.Query<Template>().Where(x => x.Report == report).CreateModels<TemplateModel>();
        }

        [HttpGet, Route("api/template/update")]
        public IEnumerable<TemplateUpdateResult> Update()
        {
            var result = new List<TemplateUpdateResult>();

            var templateDir = TemplateManager.GetTemplateDirectoryPath();

            foreach (var f in Directory.GetFiles(templateDir))
            {
                var fileName = Path.GetFileName(f);

                ParseTemplateFileName(fileName, out string report, out string name);

                if (string.IsNullOrEmpty(report) || string.IsNullOrEmpty(name))
                    throw new Exception($"Invalid file name: {fileName}. Expected <report>-<name>.hbs, for example manager-usage-summary-email.hbs (report = 'manager-usage-summary', name = 'email').");

                var template = Session.Query<Template>().FirstOrDefault(x => x.Report == report && x.TemplateName == name);
                var content = File.ReadAllText(f);

                var u = new TemplateUpdateResult { TemplateID = 0, FileName = fileName, Action = string.Empty };

                if (template == null)
                {
                    template = new Template
                    {
                        Report = report,
                        TemplateName = name,
                        TemplateContent = content
                    };

                    Session.Insert(template);
                    u.Action = "insert";
                }
                else
                {
                    template.TemplateContent = content;
                    Session.SaveOrUpdate(template);
                    u.Action = "update";
                }

                if (template != null)
                    u.TemplateID = template.TemplateID;

                result.Add(u);
            }

            return result;
        }

        private void ParseTemplateFileName(string fileName, out string report, out string name)
        {
            var match = Regex.Match(fileName, "^(.+)-(.+)\\.hbs$");

            if (match.Success)
            {
                report = match.Groups[1].Value;
                name = match.Groups[2].Value;
            }
            else
            {
                report = string.Empty;
                name = string.Empty;
            }
        }
    }
}
