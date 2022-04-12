using LNF.Data;

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

    public class ToolUsageSummaryItemConverter : IDataFeedResultItemConverter<ToolUsageSummaryItem>
    {
        public string Key => "default";

        public ToolUsageSummaryItem Convert(DataFeedResultItem item)
        {
            return new ToolUsageSummaryItem
            {
                ResourceID = int.Parse(item["ResourceID"]),
                ResourceName = item["ResourceName"],
                ChargeTypeID = int.Parse(item["ChargeTypeID"]),
                ChargeTypeName = item["ChargeTypeName"],
                TotalUses = double.Parse(item["TotalUses"]),
                TotalSchedHours = double.Parse(item["TotalSchedHours"]),
                TotalActHours = double.Parse(item["TotalActHours"]),
                NormalHoursGross = double.Parse(item["NormalHoursGross"]),
                NormalHoursForgiven = double.Parse(item["NormalHoursForgiven"]),
                NormalHoursNet = double.Parse(item["NormalHoursNet"]),
                NormalAmountGross = decimal.Parse(item["NormalAmountGross"]),
                NormalAmountForgiven = decimal.Parse(item["NormalAmountForgiven"]),
                NormalAmountNet = decimal.Parse(item["NormalAmountNet"]),
                OverTimeHoursGross = double.Parse(item["OverTimeHoursGross"]),
                OverTimeHoursForgiven = double.Parse(item["OverTimeHoursForgiven"]),
                OverTimeHoursNet = double.Parse(item["OverTimeHoursNet"]),
                OverTimeAmountGross = decimal.Parse(item["OverTimeAmountGross"]),
                OverTimeAmountForgiven = decimal.Parse(item["OverTimeAmountForgiven"]),
                OverTimeAmountNet = decimal.Parse(item["OverTimeAmountNet"]),
                BookingFeeGross = decimal.Parse(item["BookingFeeGross"]),
                BookingFeeForgiven = decimal.Parse(item["BookingFeeForgiven"]),
                BookingFeeNet = decimal.Parse(item["BookingFeeNet"]),
                BilledAmountGross = decimal.Parse(item["BilledAmountGross"]),
                BilledAmountForgiven = decimal.Parse(item["BilledAmountForgiven"]),
                BilledAmountNet = decimal.Parse(item["BilledAmountNet"])
            };
        }
    }
}