using HandlebarsDotNet;
using System;
using System.Configuration;
using System.IO;

namespace Reports.Models
{
    public static class TemplateManager
    {
        private readonly static Func<object, string> _managerUsageSummaryEmailTemplate;


        static TemplateManager()
        {
            Handlebars.RegisterHelper("format_currency", (writer, context, parameters) =>
            {
                double val = 0;

                if (parameters.Length > 0)
                {
                    if (!double.TryParse(parameters[0].ToString(), out val))
                        val = 0;
                }

                writer.WriteSafeString(val.ToString("C"));
            });

            Handlebars.RegisterHelper("format_date", (writer, context, parameters) =>
            {
                DateTime d = DateTime.Now;
                string format = "M/d/yyyy h:mm:ss tt";

                if (parameters.Length > 0)
                {
                    if (!DateTime.TryParse(parameters[0].ToString(), out d))
                        d = DateTime.Now;
                }

                if (parameters.Length > 1)
                {
                    format = parameters[1].ToString();
                }

                if (string.IsNullOrEmpty(format))
                    writer.WriteSafeString(d.ToString());
                else
                    writer.WriteSafeString(d.ToString(format));
            });

            Handlebars.RegisterHelper("link_to", (writer, context, parameters) =>
            {
                string url = null;
                string text = null;

                if (parameters.Length > 0)
                    url = parameters[0].ToString();

                if (parameters.Length > 1)
                    text = parameters[1].ToString();
                else
                    text = url;

                if (!string.IsNullOrEmpty(url))
                    writer.WriteSafeString(string.Format("<a href=\"{0}\">{1}</a>", url, text));
                else
                    writer.WriteSafeString("[link_to: at least one parameter is required]");
            });

            Handlebars.RegisterHelper("mail_to", (writer, context, parameters) =>
            {
                string email = null;
                string text = null;

                if (parameters.Length > 0)
                    email = parameters[0].ToString();

                if (parameters.Length > 1)
                    text = parameters[1].ToString();
                else
                    text = email;

                if (!string.IsNullOrEmpty(email))
                    writer.WriteSafeString(string.Format("<a href=\"mailto:{0}\">{1}</a>", email, text));
                else
                    writer.WriteSafeString("[mail_to: at least one parameter is required]");
            });

            var templateDir = ConfigurationManager.AppSettings["TemplateDirectory"];

            _managerUsageSummaryEmailTemplate = Handlebars.Compile(File.ReadAllText(Path.Combine(templateDir, "managerUsageSummaryEmail.hbs")));
        }

        public static string ManagerUsageSummaryEmailTemplate(object arg)
        {
            return _managerUsageSummaryEmailTemplate(arg);
        }
    }
}