namespace eTimeTrial.Web.Backend.Entities
{
    public class RaceReport
    {
        public string RaceID { get; set; }
        public int Started { get; set; }
        public int Finished { get; set; }
        public int DNF { get; set; }
        public List<Team> Podium {  get; set; }    

    }
}
