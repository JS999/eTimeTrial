using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace eTimeTrial.Web.Backend.DataService
{
    public interface ICosmosContext
    {
        Container RaceContainer { get; }
    }
    public class CosmosContext : ICosmosContext
    {
        public Container RaceContainer { get; }

        public CosmosContext(
        CosmosClient cosmosClient,
        IOptions<CosmosDbOptions> options)
        {
            var database =
            cosmosClient.CreateDatabaseIfNotExistsAsync(
            options.Value.DatabaseName)
            .GetAwaiter()
            .GetResult()
            .Database;

            RaceContainer =
            database.CreateContainerIfNotExistsAsync(
            options.Value.ContainerName,
            "/RaceName")
            .GetAwaiter()
            .GetResult()
            .Container;
        }
    }
}
