using LNF;
using LNF.Models.Reporting;
using LNF.Reporting;
using LNF.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reports.Models;
using System;

namespace Reports.Tests
{
    [TestClass]
    public class EmailManagerTests
    {
        [TestMethod]
        public void CanSendManagerUsageSummaryEmail()
        {
            using (DA.StartUnitOfWork())
            {
                var period = DateTime.Parse("2017-04-01");
                var mgr1 = ServiceProvider.Current.Reporting.ClientItem.CreateClientItem(2823);
                var mgr2 = ServiceProvider.Current.Reporting.ClientItem.CreateClientItem(245);
                var expected = 2;
                var actual = EmailManager.SendManagerSummaryReport(1301, period, new IReportingClient[] { mgr1, mgr2 }, string.Empty, string.Empty, false, false);
                Assert.AreEqual(expected, actual);
            }
        }
    }
}
