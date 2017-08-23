using LNF;
using LNF.Billing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Reports.Tests
{
    [TestClass]
    public class ReservationDateRangeTests
    {
        [TestMethod]
        public void CanCreateReservationDateRange()
        {
            using (Providers.DataAccess.StartUnitOfWork())
            {
                //var a = RunTest(DateTime.Parse("2017-07-03 10:30"), DateTime.Parse("2017-07-04 14:00"), 14021);
                //var b = RunTest(DateTime.Parse("2017-07-03 10:00"), DateTime.Parse("2017-07-04 12:00"), 14021);

                var x = RunTest(DateTime.Parse("2017-07-11 13:30"), DateTime.Parse("2017-07-11 18:30"), 130011);

                var n = x[759580];

                Console.WriteLine(n.ToString());

                //Assert.AreEqual(
                //    a.First(x => x.ReservationID == 757883).TransferredDuration,
                //    b.First(x => x.ReservationID == 757883).TransferredDuration);
            }
        }

        private ReservationDurations RunTest(DateTime sd, DateTime ed, int resourceId)
        {
            var dr = ReservationDateRange.ExpandRange(resourceId, sd, ed);
            var range = new ReservationDateRange(resourceId, dr);
            return range.CreateReservationDurations();
        }
    }
}
