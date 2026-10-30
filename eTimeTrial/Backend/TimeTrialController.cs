using eTimeTrial.Web.Backend.Entities;
using eTimeTrial.Web.Backend.Orchestrator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eTimeTrial.Web.Backend
{
    [Authorize(Policy = "PagePolicy")]
    [ApiController]
    [Route("api/[controller]")]
    public class TimeTrialController : ControllerBase
    {
        private readonly IRaceOrchestrator  _raceOrchestrator; 
        public TimeTrialController(IRaceOrchestrator raceOrchestrator) 
        {
            _raceOrchestrator = raceOrchestrator;
        }
       
        [HttpGet("GetUpcomingRaces")]
        public async Task<List<Race>> GetUpComingRaces()
        {
           var races = await _raceOrchestrator.UpComingRaces();
            return races;
        }
       
        [HttpGet("CalculateSpeed")]
        public string CalculateSpeed(int distance, string duration)
        {
            
            TimeSpan timeSpan = TimeSpan.Parse(duration.ToString());

            // Convert the total time to minutes
            double totalMinutes = timeSpan.TotalMinutes;
            if(totalMinutes == 0)
            {
                return JsonSerializer.Serialize(0);
            }
            var speed = (distance / totalMinutes) *60;
         

            return JsonSerializer.Serialize(Math.Round(speed, 2));
        }
        [HttpGet("CalculateDuration")]
        public string CalculateDuration(DateTime startTime, DateTime endTime)
        {
           if(startTime> endTime)
            {
                return JsonSerializer.Serialize("00:00:00:00");
            }

            TimeSpan duration = endTime - startTime;
            var hours = duration.Hours.ToString("D2");
            var minutes = duration.Minutes.ToString("D2");
            var seconds = duration.Seconds.ToString("D2");
            var milliseconds = duration.Milliseconds.ToString("D3"); ;

            var formattedDuration = $"{hours}:{minutes}:{seconds}.{milliseconds}";
            return JsonSerializer.Serialize(formattedDuration.ToString());
        }
        [HttpGet("GetStartTime")]
        public string GetStartTime()
        {


            return JsonSerializer.Serialize(DateTime.Now.ToString("hh:mm:ss.fff"));
        }
        [HttpGet("StartRace")]
        public async Task<string> StartRace(string raceId, string raceName)
        {
           await  _raceOrchestrator.StartRace(raceId,raceName);

           return JsonSerializer.Serialize(DateTime.Now.ToString("hh:mm:ss.fff"));
        }
        [HttpGet("GetRace")]
        public async Task<Race?> GetRace(string raceId, string raceName)
        {
            if (raceId == "999")
            {

                var testRace = new Race { RaceName = "test", Id = "999", RaceStart = DateTime.Now, DistanceKm = 15, Status = RaceStatus.NotStarted };
                testRace.Teams = new List<Team>();
                testRace.Teams.Add(new Team { Id = "1", TeamName = "TestTeam1", TeamNumber = "1", OffSetMin = 1, TeamStartTime = "2025-11-25T12:27:33.9922801Z" });
                return testRace;
            }
            ;

            var race = await _raceOrchestrator.GetRace(raceId,  raceName);

            return race;
        }
        [HttpGet("GetRaceStatistics")]
        public async Task<RaceStatistics?> GetRaceStatistics(string raceId, string raceName)
        {
            var race = await _raceOrchestrator.GetRaceStatistics(raceId, raceName);

            return race;
        }
        [HttpGet("GetTeamStartTime")]
        public string GetTeamStartTime(DateTime startTime, int minutesOffset)
        {
            TimeSpan timeSpan = TimeSpan.Parse(minutesOffset.ToString());
            var x = startTime.AddMinutes(minutesOffset);
            return JsonSerializer.Serialize(startTime.AddMinutes(minutesOffset).ToString("hh:mm:ss.fff"));
        }
        [HttpGet("GetFinishTime")]
        public string GetFinishTime()
        {

            return JsonSerializer.Serialize(DateTime.Now.ToString("hh:mm:ss.fff"));
        }
        [HttpPost("Download")]
        public FileResult Download([FromBody] JsonElement resultsJson)
        {
            var results = JsonSerializer.Deserialize<List<Result>>(resultsJson.ToString());

           

            if (results == null) return null;
            // Convert list to CSV string
            var csvContent = new StringBuilder();
            foreach (var row in results)
            {
                csvContent.AppendLine(string.Join(",", row));
            }

            // Convert CSV string to byte array
            var bytes = Encoding.UTF8.GetBytes(csvContent.ToString());

            // Return file as a downloadable response
            return File(bytes, "text/csv", "results.csv");



          //  return JsonSerializer.Serialize(DateTime.Now.ToString("hh:mm:ss.fff"));
        }
        [HttpGet("SaveTeamResult")]
        public async Task<Team> SaveTeamResult(string teamNumber, string raceId, string raceName)
        {
            try
            {
               

                var savedRace = await _raceOrchestrator.TeamFinished(teamNumber, raceId,raceName);
                return savedRace;
               
            }
            catch (Exception ex) { return null; }

       
        }
        [HttpPost("NewRace")]
        public async Task<Race> NewRace([FromBody] JsonElement race)
        {
            try
            {
                var newRace = JsonSerializer.Deserialize<Race>(race.ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                

                    var savedRace = await _raceOrchestrator.SaveRace(newRace);
                    return savedRace; 
              
            }
            catch(Exception ex) { return null; }
        }
    }
}
