using eTimeTrial.Web.Backend.DataService;
using eTimeTrial.Web.Backend.Entities;
using Microsoft.Azure.Cosmos;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace eTimeTrial.Web.Test.Backend.DataService
{
    public  class CosmosRaceRepositoryTests
    {
        private static CosmosRaceRepository CreateRepository(Mock<Container> containerMock)
        {
            var contextMock = new Mock<ICosmosContext>();

            contextMock
            .Setup(x => x.RaceContainer)
            .Returns(containerMock.Object);

            return new CosmosRaceRepository(
            contextMock.Object);
        }
        [Fact]
        public async Task GetRaceAsync_WhenRaceExists_ReturnsRace()
        {
            var race = new Race
            {
                Id = "1",
                RaceName = "Tour"
            };

            var responseMock = new Mock<ItemResponse<Race>>();

            responseMock.Setup(x => x.Resource).Returns(race);

            var containerMock =new Mock<Container>();

            containerMock.Setup(x => x.ReadItemAsync<Race>("1",It.IsAny<PartitionKey>(),null,default))
                .ReturnsAsync(responseMock.Object);

            var repository = CreateRepository(containerMock);

            var result = await repository.GetRaceAsync("1","Tour");

            Assert.NotNull(result);
            Assert.Equal("1", result.Id);
        }
        [Fact]
        public async Task GetRaceAsync_WhenRaceNotFound_ReturnsNull()
        {
            var containerMock = new Mock<Container>();

            containerMock.Setup(x => x.ReadItemAsync<Race>(It.IsAny<string>(),It.IsAny<PartitionKey>(),null,default))
                .ThrowsAsync(new CosmosException("Not Found", HttpStatusCode.NotFound, 0,string.Empty,0));

            var repository =CreateRepository(containerMock);

            var result = await repository.GetRaceAsync("1","Tour");

            Assert.Null(result);
        }
        [Fact]
        public async Task CreateRaceAsync_WhenRaceDoesNotExist_CreatesRace()
        {
            var race = new Race
            {
                Id = "1",
                RaceName = "Tour"
            };

            var createResponse =
                new Mock<ItemResponse<Race>>();

            createResponse
                .Setup(x => x.Resource)
                .Returns(race);

            var containerMock =
                new Mock<Container>();

            containerMock
                .Setup(x => x.ReadItemAsync<Race>(
                    It.IsAny<string>(),
                    It.IsAny<PartitionKey>(),
                    null,
                    default))
                .ThrowsAsync(
                    new CosmosException(
                        "Not Found",
                        HttpStatusCode.NotFound,
                        0,
                        string.Empty,
                        0));

            containerMock
                .Setup(x => x.CreateItemAsync(
                    race,
                    It.IsAny<PartitionKey?>(),
                    null,
                    default))
                .ReturnsAsync(createResponse.Object);

            var repository =
                CreateRepository(containerMock);

            var result =
                await repository.CreateRaceAsync(race);

            Assert.Equal(race.Id, result.Id);

            containerMock.Verify(
                x => x.CreateItemAsync(
                    race,
                    It.IsAny<PartitionKey?>(),
                    null,
                    default),
                Times.Once);
        }
        [Fact]
        public async Task CreateRaceAsync_WhenRaceExists_ReturnsExistingRace()
        {
            var race = new Race
            {
                Id = "1",
                RaceName = "Tour"
            };

            var response =
                new Mock<ItemResponse<Race>>();

            response
                .Setup(x => x.Resource)
                .Returns(race);

            var containerMock =
                new Mock<Container>();

            containerMock
                .Setup(x => x.ReadItemAsync<Race>(
                    race.Id,
                    It.IsAny<PartitionKey>(),
                    null,
                    default))
                .ReturnsAsync(response.Object);

            var repository =
                CreateRepository(containerMock);

            var result =
                await repository.CreateRaceAsync(race);

            Assert.Equal("1", result.Id);

            containerMock.Verify(
                x => x.CreateItemAsync(
                    It.IsAny<Race>(),
                    It.IsAny<PartitionKey?>(),
                    null,
                    default),
                Times.Never);
        }
        [Fact]
        public async Task UpdateRaceAsync_ReturnsUpdatedRace()
        {
            var race = new Race
            {
                Id = "1",
                RaceName = "Tour"
            };

            var response =
                new Mock<ItemResponse<Race>>();

            response
                .Setup(x => x.Resource)
                .Returns(race);

            var containerMock =
                new Mock<Container>();

            containerMock
                .Setup(x => x.ReplaceItemAsync(
                    race,
                    race.Id,
                    It.IsAny<PartitionKey>(),
                    null,
                    default))
                .ReturnsAsync(response.Object);

            var repository =
                CreateRepository(containerMock);

            var result =
                await repository.UpdateRaceAsync(race);

            Assert.Equal(race.Id, result.Id);
        }
        [Fact]
        public async Task UpdateRaceAsync_CallsReplaceItem()
        {
            var race = new Race
            {
                Id = "1",
                RaceName = "Tour"
            };

            var response =
                new Mock<ItemResponse<Race>>();

            response
                .Setup(x => x.Resource)
                .Returns(race);

            var containerMock =
                new Mock<Container>();

            containerMock
                .Setup(x => x.ReplaceItemAsync(
                    It.IsAny<Race>(),
                    It.IsAny<string>(),
                    It.IsAny<PartitionKey>(),
                    null,
                    default))
                .ReturnsAsync(response.Object);

            var repository =
                CreateRepository(containerMock);

            await repository.UpdateRaceAsync(race);

            containerMock.Verify(
                x => x.ReplaceItemAsync(
                    race,
                    race.Id,
                    It.IsAny<PartitionKey>(),
                    null,
                    default),
                Times.Once);
        }
    }
}
