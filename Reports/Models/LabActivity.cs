using LNF;
using LNF.Repository;
using LNF.Repository.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Reports.Models
{
    public class LabStatus
    {
        public DateTime StatusDateTime { get; set; }
        public IEnumerable<RoomOccupancy> RoomOccupancies { get; set; }
        public IEnumerable<ActiveReservation> ActiveReservations { get; set; }

        public class RoomOccupancy
        {
            public string RoomName { get; set; }
            public int Occupancy { get; set; }
        }

        public class ActiveReservation
        {
            public int ReservationID { get; set; }
            public string ResourceName { get; set; }
            public string ClientName { get; set; }
            public string ActivityName { get; set; }
            public DateTime ActualBeginDateTime { get; set; }
            public DateTime EndDateTime { get; set; }

        }

        public static LabStatus GetCurrentStatus()
        {
            LabStatus result = new LabStatus();

            result.StatusDateTime = DateTime.Now;

            var inArea = Providers.PhysicalAccess.CurrentlyInArea();
            result.RoomOccupancies = inArea.GroupBy(x => x.CurrentAreaName).Select(x => new RoomOccupancy { RoomName = x.Key, Occupancy = x.Count() }).ToList();

            var activeReservations = DA.Current.Query<Reservation>().Where(x => x.ActualBeginDateTime.HasValue && !x.ActualEndDateTime.HasValue && x.IsStarted && x.IsActive);
            result.ActiveReservations = activeReservations.Select(x => new ActiveReservation()
            {
                ReservationID = x.ReservationID,
                ResourceName = x.Resource.ResourceName,
                ClientName = x.Client.FName + " " + x.Client.LName,
                ActivityName = x.Activity.ActivityName,
                ActualBeginDateTime = x.ActualBeginDateTime.Value,
                EndDateTime = x.EndDateTime
            });

            return result;
        }
    }
}