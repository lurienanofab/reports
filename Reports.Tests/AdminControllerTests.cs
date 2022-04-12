using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reports.Models;
using System;
using System.Linq;

namespace Reports.Tests
{
    [TestClass]
    public class AdminControllerTests : TestBase
    {
        [TestMethod]
        public void SendManagerUsageSummaryEmailsTest()
        {
            using (StartUnitOfWork())
            {
                var emailManager = new EmailManager(Provider);

                var group = "financial-internal";
                var period = DateTime.Parse("2017-09-01");

                //int currentUserClientId = 1301;
                bool remote = false;
                var managers = emailManager.GetManagers(group, period);
                var emails = emailManager.CreateManagerUsageSummaryEmails(period, managers, remote).ToList();

                var e = emails.First(x => x.RecipientEmail == "clemk@umich.edu");
            }
        }
    }
}
