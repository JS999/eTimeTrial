using eTimeTrial.Web.Backend.DataService;
using eTimeTrial.Web.Backend.Entities;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using System.Reflection.Metadata;

namespace eTimeTrial.Web.Backend.Orchestrator
{
    public interface IRaceOrchestrator
    {
        Task<Race?> SaveRace(Race race);
        Task StartRace(string raceId, string raceName);
        Task<Team> TeamFinished(string teamNumber, string raceId, string raceName);
        Task RaceFinished(string raceId, string raceName);
        Task<Race?> GetRace(string raceId, string raceName);
        Task<List<Race>> UpComingRaces();
        Task<RaceStatistics?> GetRaceStatistics(string raceId, string raceName);
    }
    public class RaceOrchestrator : IRaceOrchestrator
    {
        private readonly ICosmosDataService _cosmosDataService;
        private readonly Database _database;
        private readonly Container _container;
        private readonly ILogger<RaceOrchestrator> _logger;

        public RaceOrchestrator(ICosmosDataService cosmosDataService, ILogger<RaceOrchestrator> logger)
        {
            _logger = logger;
            _cosmosDataService = cosmosDataService;
            _database = _cosmosDataService.CreateDatabaseAsync("eTimeTrial").Result;
            _container = _cosmosDataService.CreateContainerAsync(_database, "Races", "Event").Result;
        }


        public async Task<Race?> SaveRace(Race race)
        {
            try
            {
                if (race != null)
                {
                    race.Status = RaceStatus.NotStarted;
                    race.Id = Guid.NewGuid().ToString();
                    foreach (var team in race.Teams)
                    {
                        team.Id = Guid.NewGuid().ToString();
                        team.OffSetMin = (team.StartPosition - 1) * race.Interval;
                        //team.TeamStartTime = race.RaceDate.AddMinutes(team.OffSetMin);
                    }
                    _ = await _cosmosDataService.AddRaceToContainerAsync(_container, race);
                    return race;
                }


                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Saving Race: {RaceId} {RaceName}", race.Id, race.RaceName);
                throw; // let controller return 500
            }


        }
        public async Task StartRace(string raceId, string raceName)
        {
            try
            {
                var race = await _cosmosDataService.GetRace(_container, raceId, raceName);
                race.Status = RaceStatus.InProgress;
                race.RaceStart = DateTime.UtcNow;

                foreach (var team in race.Teams)
                {
                    team.TeamStartTime = race.RaceStart.Value.AddMinutes(team.OffSetMin).ToLongTimeString();//.ToString("hh:mm:ss");
                }

                var updatedRace = await _cosmosDataService.UpdateRace(_container, race);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Starting Race: {RaceId} {RaceName}", raceId, raceName);
                throw; // let controller return 500
            }
        }
        public async Task<Team> TeamFinished(string teamNumber, string raceId, string raceName)
        {
            try
            {
                var race = await _cosmosDataService.GetRace(_container, raceId, raceName);
                var team = race.Teams.Where(t => t.TeamNumber == teamNumber).FirstOrDefault();
                if (team != null)
                {

                    //team.TeamFinishTime = DateTime.Now.ToString("hh:mm:ss.fff");

                    string formattedDuration;

                    var teamStartTime = DateTime.Parse(team.TeamStartTime);

                    if (teamStartTime > DateTime.Now)
                    {
                        formattedDuration = "00:00:00:00";
                    }
                    else
                    {

                        TimeSpan duration = DateTime.Now - teamStartTime;
                        var hours = duration.Hours.ToString("D2");
                        var minutes = duration.Minutes.ToString("D2");
                        var seconds = duration.Seconds.ToString("D2");
                        var milliseconds = duration.Milliseconds.ToString("D3"); ;

                        formattedDuration = $"{hours}:{minutes}:{seconds}.{milliseconds}";


                        // Convert the total time to minutes
                        double totalMinutes = duration.TotalMinutes;

                        var speed = (race.DistanceKm / totalMinutes) * 60;
                        team.SpeedKm = Math.Round(speed, 2, MidpointRounding.AwayFromZero);

                    }

                    team.TeamFinishTime = DateTime.Now.ToLongTimeString();//.ToString("hh:mm:ss.fff");
                    team.TeamDuration = formattedDuration;

                    var updatedRace = await _cosmosDataService.UpdateRace(_container, race);
                    return team;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Finishing Race for team:{teamNumber} {RaceId} {RaceName}", teamNumber, raceId, raceName);
                throw; // let controller return 500
            }



        }
        public async Task RaceFinished(string raceId, string raceName)
        {
            try
            {
                var race = await _cosmosDataService.GetRace(_container, raceId, raceName);

                foreach (var team in race.Teams.Where(t => string.IsNullOrEmpty(t.TeamFinishTime)))
                {
                    team.TeamFinishTime = "DNF";
                }
                race.RaceFinish = DateTime.Now;
                race.Status = RaceStatus.Finished;
                var updatedRace = await _cosmosDataService.UpdateRace(_container, race);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Finishing Race : {RaceId} {RaceName}", raceId, raceName);
                throw; // let controller return 500
            }
        }
        public async Task<Race?> GetRace(string raceId, string raceName)
        {
            try
            {
                var race = await _cosmosDataService.GetRace(_container, raceId, raceName);

                return race;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Race : {RaceId} {RaceName}", raceId, raceName);
                throw; // let controller return 500
            }
        }
        public async Task<List<Race>> UpComingRaces()
        {
            try
            {
                var raceList = await _cosmosDataService.GetRaces(_container);
                return raceList.Where(item => item.Status == RaceStatus.NotStarted || item.Status == RaceStatus.InProgress).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Upcoming Race List");
                throw; // let controller return 500
            }
        }

        public async Task<RaceStatistics?> GetRaceStatistics(string raceId, string raceName)
        {
            try
            {
                var race = await _cosmosDataService.GetRace(_container, raceId, raceName);
                var raceStatistics = new RaceStatistics
                {
                    StartTime = race.RaceStart.Value.ToShortTimeString(),
                    FinishTime = race.RaceFinish.ToShortTimeString(),
                    Finished = race.Teams.Where(item => item.TeamFinishTime != null).Count(),
                    Started = race.Teams.Where(item => item.TeamStartTime != null).Count(),
                };
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Race statistics : {RaceId} {RaceName}", raceId, raceName);
                throw; // let controller return 500
            }
        }
    }
}
