using HandlebarsDotNet;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.RegularExpressions;

namespace Reports.Models
{
    public static class TemplateManager
    {
        private static readonly ConcurrentDictionary<string, HandlebarsTemplate<object, object>> _templates = new ConcurrentDictionary<string, HandlebarsTemplate<object, object>>();

        private static string _basePath;

        static TemplateManager()
        {
            Handlebars.RegisterHelper("format_currency", (output, context, arguments) =>
            {
                double val = 0;

                if (arguments.Length > 0)
                {
                    if (!double.TryParse(arguments[0].ToString(), out val))
                        val = 0;
                }

                output.WriteSafeString(val.ToString("C"));
            });

            Handlebars.RegisterHelper("format_date", (output, context, arguments) =>
            {
                DateTime d = DateTime.Now;
                string format = "M/d/yyyy h:mm:ss tt";

                if (arguments.Length > 0)
                {
                    if (!DateTime.TryParse(arguments[0].ToString(), out d))
                        d = DateTime.Now;
                }

                if (arguments.Length > 1)
                {
                    format = arguments[1].ToString();
                }

                if (arguments.Length > 2)
                {
                    Func<int, DateTime, DateTime> operation = null;

                    string value = arguments[2].ToString();

                    var matches = Regex.Match(value, "(\\+|-)(\\d+) (day|month|year|hour|minute|second|ms)");

                    var op = matches.Groups[1].Value;
                    var amt = int.Parse(matches.Groups[2].Value) * (op == "-" ? -1 : 1);
                    var part = matches.Groups[3].Value;

                    switch (part)
                    {
                        case "day":
                            operation = (i, date) => date.AddDays(i);
                            break;
                        case "month":
                            operation = (i, date) => date.AddMonths(i);
                            break;
                        case "year":
                            operation = (i, date) => date.AddYears(i);
                            break;
                        case "hour":
                            operation = (i, date) => date.AddYears(i);
                            break;
                        case "minute":
                            operation = (i, date) => date.AddMinutes(i);
                            break;
                        case "second":
                            operation = (i, date) => date.AddSeconds(i);
                            break;
                        case "ms":
                            operation = (i, date) => date.AddMilliseconds(i);
                            break;
                    }

                    if (operation != null)
                    {
                        d = operation(amt, d);
                    }
                }

                if (string.IsNullOrEmpty(format))
                    output.WriteSafeString(d.ToString());
                else
                    output.WriteSafeString(d.ToString(format));
            });

            Handlebars.RegisterHelper("link_to", (output, context, arguments) =>
            {
                string url = null;
                string text = null;

                if (arguments.Length > 0)
                    url = arguments[0].ToString();

                if (arguments.Length > 1)
                    text = arguments[1].ToString();
                else
                    text = url;

                if (!string.IsNullOrEmpty(url))
                    output.WriteSafeString(string.Format("<a href=\"{0}\">{1}</a>", url, text));
                else
                    output.WriteSafeString("[link_to: at least one parameter is required]");
            });

            Handlebars.RegisterHelper("mail_to", (output, context, arguments) =>
            {
                string email = null;
                string text = null;

                if (arguments.Length > 0)
                    email = arguments[0].ToString();

                if (arguments.Length > 1)
                    text = arguments[1].ToString();
                else
                    text = email;

                if (!string.IsNullOrEmpty(email))
                    output.WriteSafeString(string.Format("<a href=\"mailto:{0}\">{1}</a>", email, text));
                else
                    output.WriteSafeString("[mail_to: at least one parameter is required]");
            });

            //var template = GetTemplate(System.Web.HttpContext.Current.Server.MapPath("~"), "manager-usage-summary", "email");
            //_templates.TryAdd("manager-usage-summary-email", Handlebars.Compile(template));
        }

        public static string GetTemplateDirectoryPath()
        {
            return Path.Combine(_basePath, "Content", "Templates");
        }

        public static string GetTemplateFilePath(string report, string name)
        {
            var result = Path.Combine(GetTemplateDirectoryPath(), $"{report}-{name}.hbs");
            return result;
        }

        public static void SetBasePath(string value)
        {
            if (!Directory.Exists(value))
                throw new Exception($"Directory not found: {value}");

            _basePath = value;
        }

        public static string ExecuteTemplate(string report, string name, object args)
        {
            var key = $"{report}-name";

            if (!_templates.TryGetValue(key, out HandlebarsTemplate<object, object> tmpl))
            {
                var template = GetTemplate(report, name);
                tmpl = Handlebars.Compile(template);
                _templates.TryAdd(key, tmpl);
            }

            return tmpl(args);
        }

        public static string ManagerUsageSummaryEmailTemplate(object args)
        {
            var result = ExecuteTemplate("manager-usage-summary", "email", args);
            return result;
        }

        public static string GetTemplate(string report, string name)
        {
            var templatePath = GetTemplateFilePath(report, name);
            string templateContent;

            if (File.Exists(templatePath))
                templateContent = File.ReadAllText(templatePath);
            else
                throw new TemplateNotFoundException(report, name);

            return templateContent;
        }
    }

    public class TemplateNotFoundException : Exception
    {
        public string Report { get; }
        public string Name { get; }


        public override string Message => string.Format("Template not found [{0}/{1}].", Report, Name);

        public TemplateNotFoundException(string report, string name)
        {
            Report = report;
            Name = name;
        }
    }
}