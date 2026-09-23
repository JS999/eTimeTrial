using Azure;
using eTimeTrial.Web.Backend.Entities;
using eTimeTrial.Web.Pages;
using Microsoft.Azure.Cosmos;
//using System.ComponentModel;
using System.Net;
using System.Reflection.Metadata;
using System.Xml.Linq;

namespace eTimeTrial.Web.Backend.DataService
{
    public interface ICosmosDataService
    {
        Task<Database> CreateDatabaseAsync(string databaseId);
        Task<Container> CreateContainerAsync(Database database, string containerId, string containerName);
        Task<string> AddRaceToContainerAsync(Container container, Race race);
        Task UpdateAsync(Container container, Race race, Team team);
        Task StartRace(Container container, Race race);
        void CleanupAsync();
        Task<Race?> GetRace(Container container, string raceId, string raceName);
        Task<Race> UpdateRace(Container container, Race race);
        Task<List<Race>> GetRaces(Container container);
    }


    public class CosmosDataService: ICosmosDataService
    {
        private CosmosClient _cosmosClient;

        public CosmosDataService(CosmosClient cosmosClient) 
        {
            _cosmosClient = cosmosClient;
        }


         

        public async Task<Database> CreateDatabaseAsync(string databaseId)
        {
            // Create a new database
            Database database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            return database;
            //Console.WriteLine("Created Database: {0}\n", this.database.Id);
        }

        public async Task<Container> CreateContainerAsync(Database database, string containerId,string containerName)
        {
            // Create a new container
            Container container = await database.CreateContainerIfNotExistsAsync(containerId, $"/RaceName");
            return container;
        }
       

        public async Task<string> AddRaceToContainerAsync(Container container, Race race)
        {
           

            try
            {
               
                // Read the item to see if it exists.  
                ItemResponse<Race> raceResponse = await container.ReadItemAsync<Race>(race.Id, new PartitionKey(race.RaceName));
              
                Console.WriteLine("Item in database with id: {0} already exists\n", raceResponse.Resource.Id);
                return raceResponse.Resource.Id;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in the container representing the Andersen family. Note we provide the value of the partition key for this item, which is "Andersen"
               
                ItemResponse<Race> raceResponse = await container.CreateItemAsync<Race>(race, new PartitionKey(race.RaceName));

                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", raceResponse.Resource.Id, raceResponse.RequestCharge);
                return raceResponse.Resource.Id;
            }
        }
        public async Task UpdateAsync(Container container, Race race, Team team)
        {
           
            ItemResponse<Race> raceResponse = await container.ReadItemAsync<Race>(race.Id, new PartitionKey(race.RaceName));

            var document = raceResponse.Resource;

            document.RaceStart = race.RaceStart;
            document.RaceName = race.RaceName;
            document.DistanceKm = race.DistanceKm;       
            document.Interval= race.Interval;
            if (team != null)
            {
                if (document.Teams != null && document.Teams.First(item => item.Id == team.Id) == null) { 
                document.Teams.First(item => item.Id == team.Id).OffSetMin = team.OffSetMin;
                document.Teams.First(item => item.Id == team.Id).TeamName = team.TeamName;
                document.Teams.First(item => item.Id == team.Id).StartPosition = team.StartPosition;
                document.Teams.First(item => item.Id == team.Id).OffSetMin = team.OffSetMin;
                document.Teams.First(item => item.Id == team.Id).TeamStartTime = team.TeamStartTime;
                document.Teams.First(item => item.Id == team.Id).TeamFinishTime = team.TeamFinishTime;

                document.Teams.First(item => item.Id == team.Id).PodiumPosition = team.PodiumPosition;
                document.Teams.First(item => item.Id == team.Id).SpeedKm = team.SpeedKm;
            }
            }

            ItemResponse<Race> updateResponse = await container.ReplaceItemAsync(item: document, id: document.Id, partitionKey: new PartitionKey(race.RaceName));

           
        }




        public async Task<Race?> GetRace(Container container, string raceId, string raceName)
        {
              
            ItemResponse<Race> raceResponse = await container.ReadItemAsync<Race>(raceId, new PartitionKey(raceName));
            if (raceResponse != null) {
               return raceResponse.Resource;
            }
            return null;
        }



        public async Task<Race> UpdateRace(Container container, Race race)
        {
            ItemResponse<Race> updateResponse = await container.ReplaceItemAsync(item: race, id: race.Id, partitionKey: new PartitionKey(race.RaceName));

            if (updateResponse != null)
            {
                return updateResponse.Resource;
            }
            return null;
        }
        



        public async Task StartRace(Container container, Race race)
        {

            ItemResponse<Race> raceResponse = await container.ReadItemAsync<Race>(race.Id, new PartitionKey(race.RaceName));

            var document = raceResponse.Resource;
            document.RaceStart = DateTime.UtcNow;

            foreach (var team in document.Teams)
            {
                team.TeamStartTime = race.RaceStart.Value.AddMinutes(team.OffSetMin).ToLongTimeString();//.ToString("hh:mm:ss");
            }


            //document.RaceDate = race.RaceDate;
            //document.RaceName = race.RaceName;
            //document.DistanceKm = race.DistanceKm;
            //document.Interval = race.Interval;
            //if (team != null)
            //{
            //    if (document.Teams != null && document.Teams.First(item => item.Id == team.Id) == null)
            //    {
            //        document.Teams.First(item => item.Id == team.Id).OffSetMin = team.OffSetMin;
            //        document.Teams.First(item => item.Id == team.Id).TeamName = team.TeamName;
            //        document.Teams.First(item => item.Id == team.Id).StartPosition = team.StartPosition;
            //        document.Teams.First(item => item.Id == team.Id).OffSetMin = team.OffSetMin;
            //        document.Teams.First(item => item.Id == team.Id).TeamStartTime = team.TeamStartTime;
            //        document.Teams.First(item => item.Id == team.Id).TeamFinishTime = team.TeamFinishTime;

            //        document.Teams.First(item => item.Id == team.Id).PodiumPosition = team.PodiumPosition;
            //        document.Teams.First(item => item.Id == team.Id).SpeedKm = team.SpeedKm;
            //    }
            //}

            ItemResponse<Race> updateResponse = await container.ReplaceItemAsync(item: document, id: document.Id, partitionKey: new PartitionKey(race.RaceName));


        }

        public void CleanupAsync()
        {

            //Dispose of CosmosClient
            _cosmosClient.Dispose();
        }

        public async Task<List<Race>> GetRaces(Container container)
        {
            var results = new List<Race>();

            // Query definition - SELECT * FROM c gets all documents
            var query = new QueryDefinition("SELECT * FROM c");

            using FeedIterator<Race> feedIterator = container.GetItemQueryIterator<Race>(query);

            while (feedIterator.HasMoreResults)
            {
                FeedResponse<Race> response = await feedIterator.ReadNextAsync();
                results.AddRange(response);
            }

            return results;
        }
    }
}
