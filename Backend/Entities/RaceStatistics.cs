namespace eTimeTrial.Web.Backend.Entities
{
    public class RaceStatistics
    {
        public String StartTime { get; set; }
        public String FinishTime { get; set; }
        public String AverageTime { get; set; }
        public int Started { get; set; }
        public int Finished { get; set; }
        public int Dnf { get; set; }

    }
}
