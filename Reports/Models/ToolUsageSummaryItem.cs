namespace Reports.Models
{
    public class ToolUsageSummaryItem
    {
        public int ResourceID { get; set; }
        public string ResourceName { get; set; }
        public int ChargeTypeID { get; set; }
        public string ChargeTypeName { get; set; }
        public double TotalUses { get; set; }
        public double TotalSchedHours { get; set; }
        public double TotalActHours { get; set; }
        public double NormalHoursGross { get; set; }
        public double NormalHoursForgiven { get; set; }
        public double NormalHoursNet { get; set; }
        public decimal NormalAmountGross { get; set; }
        public decimal NormalAmountForgiven { get; set; }
        public decimal NormalAmountNet { get; set; }
        public double OverTimeHoursGross { get; set; }
        public double OverTimeHoursForgiven { get; set; }
        public double OverTimeHoursNet { get; set; }
        public decimal OverTimeAmountGross { get; set; }
        public decimal OverTimeAmountForgiven { get; set; }
        public decimal OverTimeAmountNet { get; set; }
        public decimal BookingFeeGross { get; set; }
        public decimal BookingFeeForgiven { get; set; }
        public decimal BookingFeeNet { get; set; }
        public decimal BilledAmountGross { get; set; }
        public decimal BilledAmountForgiven { get; set; }
        public decimal BilledAmountNet { get; set; }
    }
}