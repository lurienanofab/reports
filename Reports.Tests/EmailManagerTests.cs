using LNF.Reporting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reports.Models;
using System;

namespace Reports.Tests
{
    [TestClass]
    public class EmailManagerTests : TestBase
    {
        [TestMethod]
        public void CanSendManagerUsageSummaryEmail()
        {
            using (StartUnitOfWork())
            {
                var emailManager = new EmailManager(Provider);
                var period = DateTime.Parse("2017-04-01");
                var mgr1 = Provider.Reporting.ClientItem.CreateClientItem(2823);
                var mgr2 = Provider.Reporting.ClientItem.CreateClientItem(245);
                var expected = 2;
                var actual = emailManager.SendManagerSummaryReport(1301, period, new IReportingClient[] { mgr1, mgr2 }, string.Empty, string.Empty, false, false);
                Assert.AreEqual(expected, actual);
            }
        }
    }
}
