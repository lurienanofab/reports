using LNF.PhysicalAccess;
using System;

namespace Reports.Models
{
    public class InLabClient
    {
        private readonly DateTime _now;

        public InLabClient() { }

        public InLabClient(Badge badge)
        {
            _now = DateTime.Now;

            ClientID = badge.ClientID;
            CardNumber = badge.CurrentCardNumber.GetValueOrDefault().ToString();
            EventDateTime = badge.CurrentAccessTime.GetValueOrDefault(_now);
            FirstName = badge.FirstName;
            LastName = badge.LastName;
        }

        public string FullName => $"{FirstName} {LastName}";
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public double Duration => (_now - EventDateTime).TotalHours;
        public string CardNumber { get; set; }
        public DateTime EventDateTime { get; set; }
        public int ClientID { get; set; }
    }
}