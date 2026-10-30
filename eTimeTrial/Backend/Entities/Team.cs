namespace eTimeTrial.Web.Backend.Entities
{
    public class Team
    {
        public string Id { get; set; }
        public string TeamNumber { get; set; }
        public string TeamName { get; set; }
        public int StartPosition { get; set; }
        public int OffSetMin {  get; set; }
        public string TeamStartTime { get; set; }
        public string TeamFinishTime { get; set; }
        public int PodiumPosition { get; set; }
        public string TeamDuration { get; set; }
        public double SpeedKm { get; set; }
    }
}
