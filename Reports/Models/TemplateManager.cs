using HandlebarsDotNet;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;

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

                if (parameters.Length > 2)
                {
                    Func<int, DateTime, DateTime> operation = null;

                    string value = parameters[2].ToString();

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