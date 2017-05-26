using System;

namespace Reports.Models
{
    public class EmailReportModel
    {
        public int ClientID { get; set; }
        public DateTime Period { get; set; }
        public bool IncludeRemote { get; set; }
    }
}