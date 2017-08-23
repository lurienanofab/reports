using LNF.Models.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using LNF.Billing;

namespace Reports.Models
{
    public class DurationsModel
    {
        public int ReservationID { get; set; }
        public int ResourceID { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public IEnumerable<ResourceModel> Resources { get; set; }
        public ReservationDateRange.DateRange Range { get; set; }
        public int? Zoom { get; set; }

        public ResourceModel SelectedResource
        {
            get
            {
                if (Resources != null)
                    return Resources.FirstOrDefault(x => x.ResourceID == ResourceID);
                else
                    return null;
            }
        }

        public DateTime GetStartDateTime()
        {
            if (Range == default(ReservationDateRange.DateRange))
                return StartDate.GetValueOrDefault(DateTime.Now.Date.AddDays(-1));
            else
                return Range.StartDate;
        }

        public DateTime GetEndDateTime()
        {
            if (Range == default(ReservationDateRange.DateRange))
                return EndDate.GetValueOrDefault(GetStartDateTime().AddDays(1));
            else
                return Range.EndDate;
        }
    }
}