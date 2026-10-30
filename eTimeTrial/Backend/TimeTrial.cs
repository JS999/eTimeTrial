namespace eTimeTrial.Web.Backend
{
    public class Result
    {
        public string? TeamNumber { get; set; }
        public string? TeamName { get; set; }
        public string? StartTime { get; set; }
        public string? FinishTime { get; set; }
        public int? IntervalOffSet { get; set; }
        public string?    Duration { get; set; }
        public double? Speed { get; set; }
    }
    public class Results
    {
        public List<Result>? Teams { get; set; }
    }
}
