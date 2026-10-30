using eTimeTrial.Web.Backend.Entities;

namespace eTimeTrial.Web.Backend.DataService
{
    public interface IRaceService
    {
        Task StartRaceAsync(
        string raceId,
        string raceName);

        Task UpdateRaceAsync(
        Race race,
        Team? team);
    }
    public class RaceService : IRaceService
    {
        private readonly IRaceRepository _repository;

        public RaceService(
        IRaceRepository repository)
        {
            _repository = repository;
        }

        public async Task StartRaceAsync(
        string raceId,
        string raceName)
        {
            var race =
            await _repository.GetRaceAsync(
            raceId,
            raceName);

            if (race is null)
            {
                throw new InvalidOperationException(
                $"Race '{raceId}' not found.");
            }

            race.RaceStart = DateTime.UtcNow;

            foreach (var team in race.Teams)
            {
                team.TeamStartTime =
                race.RaceStart.Value
                .AddMinutes(team.OffSetMin)
                .ToLongTimeString();
            }

            await _repository.UpdateRaceAsync(race);
        }

        public async Task UpdateRaceAsync(
        Race race,
        Team? team)
        {
            if (team != null)
            {
                var existingTeam =
                race.Teams.FirstOrDefault(
                x => x.Id == team.Id);

                if (existingTeam != null)
                {
                    existingTeam.TeamName = team.TeamName;
                    existingTeam.StartPosition = team.StartPosition;
                    existingTeam.OffSetMin = team.OffSetMin;
                    existingTeam.TeamStartTime = team.TeamStartTime;
                    existingTeam.TeamFinishTime = team.TeamFinishTime;
                    existingTeam.PodiumPosition = team.PodiumPosition;
                    existingTeam.SpeedKm = team.SpeedKm;
                }
            }

            await _repository.UpdateRaceAsync(race);
        }
    }
}
