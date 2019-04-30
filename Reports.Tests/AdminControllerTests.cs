using LNF.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reports.Models;
using System;
using System.Linq;

namespace Reports.Tests
{
    [TestClass]
    public class AdminControllerTests
    {
        [TestMethod]
        public void SendManagerUsageSummaryEmailsTest()
        {
            using (DA.StartUnitOfWork())
            {
                var group = "financial-internal";
                var period = DateTime.Parse("2017-09-01");

                int currentUserClientId = 1301;
                bool remote = false;
                var managers = EmailManager.GetManagers(currentUserClientId, group, period, remote);
                var emails = EmailManager.CreateManagerUsageSummaryEmails(currentUserClientId, period, managers, remote).ToList();

                var e = emails.First(x => x.RecipientEmail == "clemk@umich.edu");
            }
        }
    }
}
