using LNF.Billing;
using LNF.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Reports.Models
{
    public class DurationsModel
    {
        public int ReservationID { get; set; }
        public int ResourceID { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public IEnumerable<IResource> Resources { get; set; }
        public DateRange Range { get; set; }
        public int? Zoom { get; set; }

        public IResource SelectedResource
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
            if (Range == default(DateRange))
                return StartDate.GetValueOrDefault(DateTime.Now.Date.AddDays(-1));
            else
                return Range.StartDate;
        }

        public DateTime GetEndDateTime()
        {
            if (Range == default(DateRange))
                return EndDate.GetValueOrDefault(GetStartDateTime().AddDays(1));
            else
                return Range.EndDate;
        }
    }
}