using System;

namespace Reports.Models
{
    public class DurationInfoArgs
    {
        public int ResourceID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}