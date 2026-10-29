using eTimeTrial.Web.Backend.Entities;
using Microsoft.Azure.Cosmos;
using System.Net;

namespace eTimeTrial.Web.Backend.DataService
{
    public interface IRaceRepository
    {
        Task<Race?> GetRaceAsync(
        string raceId,
        string raceName);

        Task<List<Race>> GetRacesAsync();

        Task<Race> CreateRaceAsync(Race race);

        Task<Race> UpdateRaceAsync(Race race);
    }

    public class CosmosRaceRepository : IRaceRepository
    {
        private readonly Container _container;

        public CosmosRaceRepository(
        ICosmosContext context)
        {
            _container = context.RaceContainer;
        }

        public async Task<Race?> GetRaceAsync(
        string raceId,
        string raceName)
        {
            try
            {
                var response =
                await _container.ReadItemAsync<Race>(
                raceId,
                new PartitionKey(raceName));

                return response.Resource;
            }
            catch (CosmosException ex)
            when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<Race> CreateRaceAsync(
        Race race)
        {
            var existing =
            await GetRaceAsync(
            race.Id,
            race.RaceName);

            if (existing is not null)
            {
                return existing;
            }

            var response =
            await _container.CreateItemAsync(
            race,
            new PartitionKey(race.RaceName));

            return response.Resource;
        }

        public async Task<Race> UpdateRaceAsync(
        Race race)
        {
            var response =
            await _container.ReplaceItemAsync(
            race,
            race.Id,
            new PartitionKey(race.RaceName));

            return response.Resource;
        }

        public async Task<List<Race>> GetRacesAsync()
        {
            var races = new List<Race>();

            var query =
            new QueryDefinition("SELECT * FROM c");

            using var iterator =
            _container.GetItemQueryIterator<Race>(query);

            while (iterator.HasMoreResults)
            {
                var response =
                await iterator.ReadNextAsync();

                races.AddRange(response);
            }

            return races;
        }
    }
}
