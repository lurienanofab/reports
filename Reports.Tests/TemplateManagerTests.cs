﻿using LNF;
using LNF.Reporting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reports.Models;
using System;

namespace Reports.Tests
{
    [TestClass]
    public class TemplateManagerTests : TestBase
    {
        [TestMethod]
        public void CanUseManagerUsageReportMessageTemplate()
        {
            using (StartUnitOfWork())
            {
                //string expected;
                string actual;

                var generator = new ReportGenerator(Provider);

                //expected = "<p>Dear Test User,<br/><br/>Below is a summary of your group's usage charges in the LNF during the month of April 2017. The numbers include all LNF charges (room, tool and store). Please note that, while they are estimates that may still change, this message intends to give you an early reasonable indication of usage charges, before the final numbers are posted/invoiced.</p><hr/>";
                var period = DateTime.Parse("2017-04-01");
                var client = Provider.Reporting.ClientItem.CreateClientItem(2823);
                var model = generator.CreateManagerUsageSummary(period, client, false);
                var @class = string.Format("col-md-{0}", model.ShowSubsidyColumn ? "7" : "4");

                actual = TemplateManager.ManagerUsageSummaryEmailTemplate(new { period, client, model, @class });
                Console.WriteLine(actual);
                //Assert.AreEqual(expected, actual);


                //expected = "As a reminder, more details about lab usage for each user listed above can be found in the LNF Online Services (<a href=\"http://ssel-sched.eecs.umich.edu/sselonline\">http://ssel-sched.eecs.umich.edu/sselonline</a>). Instructions are available at <a href=\"http://lnf-wiki.eecs.umich.edu/wiki/Online_Usage_Report\">http://lnf-wiki.eecs.umich.edu/wiki/Online_Usage_Report</a>.<br/><br/>If any change in the list of lab users or accounts is needed for future usage, or if you have any question, please contact <a href=\"mailto:LNF-billing@umich.edu\">LNF-billing@umich.edu</a>.<br/><br/>Best regards,<br/><br/>Sandrine";
                //actual = TemplateManager.ManagerUsageReportBodyFooterTemplate(null);
                //Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void CanGetEmailTemplate()
        {
            using (StartUnitOfWork())
            {
                var generator = new ReportGenerator(Provider);

                var period = DateTime.Parse("2017-04-01");
                var client = Provider.Reporting.ClientItem.CreateClientItem(2823);
                var model = generator.CreateManagerUsageSummary(period, client, false);
                var @class = string.Format("col-md-{0}", model.ShowSubsidyColumn ? "7" : "4");
                var content = TemplateManager.ExecuteTemplate("manager-usage-summary", "email", new { period, client, model, @class });
                Assert.IsFalse(string.IsNullOrEmpty(content));
            }
        }

        [TestMethod]
        public void CanGetReportTemplate()
        {
            using (StartUnitOfWork())
            {
                var generator = new ReportGenerator(Provider);

                var period = DateTime.Parse("2017-04-01");
                var client = Provider.Reporting.ClientItem.CreateClientItem(2823);
                var model = generator.CreateManagerUsageSummary(period, client, false);
                var @class = string.Format("col-md-{0}", model.ShowSubsidyColumn ? "7" : "4");
                var content = TemplateManager.ExecuteTemplate("manager-usage-summary", "report", new { period, client, model, @class });
                Assert.IsFalse(string.IsNullOrEmpty(content));
            }
        }
    }
}
