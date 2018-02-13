using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Reports.Models
{
    public static class HelperExtensions
    {
        public static string GetNumericStyle(this HtmlHelper helper, int n, params string[] s)
        {
            var styles = new List<string>(s);

            if (n < 0)
                styles.Add("color: red;");

            return string.Join(" ", styles);
        }

        public static string GetNumericStyle(this HtmlHelper helper, decimal n, params string[] s)
        {
            var styles = new List<string>(s);

            if (n < 0)
                styles.Add("color: red;");

            return string.Join(" ", styles);
        }

        public static string GetNumericStyle(this HtmlHelper helper, double n, params string[] s)
        {
            var styles = new List<string>(s);

            if (n < 0)
                styles.Add("color: red;");

            return string.Join(" ", styles);
        }
    }
}