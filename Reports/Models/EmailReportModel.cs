using System;

namespace Reports.Models
{
    public class EmailReportModel
    {
        public int ClientID { get; set; }
        public int CurrentUserClientID { get; set; }
        public DateTime Period { get; set; }
        public string Message { get; set; }
        public string CCAddress { get; set; }
        public bool Debug { get; set; }
        public bool IncludeRemote { get; set; }
    }
}