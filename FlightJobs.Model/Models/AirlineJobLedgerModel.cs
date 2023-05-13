namespace FlightJobs.Model.Models
{
    public class AirlineJobLedgerModel
    {
        public long Id { get; set; }
        public AirlineModel Airline { get; set; }
        public JobModel Job { get; set; }
        public long JobDebtValue { get; set; }
        public double FuelPrice { get; set; }
        public double FuelCost { get; set; }
        public double FuelCostPerNM { get; set; }
        public double GroundCrewCost { get; set; }
        public double FlightCrewCost { get; set; }
        public double FlightAttendantCost { get; set; }
        public double TotalCrewCostLabor { get; set; }
        public double TotalFlightCost { get; set; }
        public double RevenueEarned { get; set; }
        public double FlightIncome { get; set; }
        public string WeightUnit { get; set; }
    }
}
