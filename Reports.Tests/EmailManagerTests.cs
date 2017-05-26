using LNF;
using LNF.Models.Reporting;
using LNF.Reporting;
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
            using (Providers.DataAccess.StartUnitOfWork())
            {
                var period = DateTime.Parse("2017-04-01");
                var mgr1 = ClientItemUtility.CreateClientItem(2823);
                var mgr2 = ClientItemUtility.CreateClientItem(245);
                var expected = 2;
                var actual = EmailManager.SendManagerSummaryReport(period, new ClientItem[] { mgr1, mgr2 }, false);
                Assert.AreEqual(expected, actual);
            }
        }
    }
}
