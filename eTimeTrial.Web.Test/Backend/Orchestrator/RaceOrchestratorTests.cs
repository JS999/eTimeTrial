using eTimeTrial.Web.Backend;
using eTimeTrial.Web.Backend.DataService;
using eTimeTrial.Web.Backend.Entities;
using eTimeTrial.Web.Backend.Orchestrator;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace eTimeTrial.Web.Test.Backend.Orchestrator;

public class RaceOrchestratorTests
{
    private readonly Mock<IRaceRepository> _repositoryMock;
    private readonly Mock<ILogger<RaceOrchestrator>> _loggerMock;
    private readonly RaceOrchestrator _sut;

    public RaceOrchestratorTests()
    {
        _repositoryMock = new Mock<IRaceRepository>();
        _loggerMock = new Mock<ILogger<RaceOrchestrator>>();

        _sut = new RaceOrchestrator(
            _repositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task SaveRace_Should_SetDefaults_And_CreateRace()
    {
        // Arrange
        var race = new Race
        {
            RaceName = "Summer TT",
            Interval = 2,
            Teams =
            [
                new Team { StartPosition = 1 },
                new Team { StartPosition = 2 },
                new Team { StartPosition = 3 }
            ]
        };

        _repositoryMock
            .Setup(r => r.CreateRaceAsync(It.IsAny<Race>()))
            .ReturnsAsync((Race r) => r);

        // Act
        var result = await _sut.SaveRace(race);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RaceStatus.NotStarted, result!.Status);
        Assert.False(string.IsNullOrWhiteSpace(result.Id));

        Assert.Equal(0, result.Teams[0].OffSetMin);
        Assert.Equal(2, result.Teams[1].OffSetMin);
        Assert.Equal(4, result.Teams[2].OffSetMin);

        Assert.All(result.Teams,
            t => Assert.False(string.IsNullOrWhiteSpace(t.Id)));

        _repositoryMock.Verify(
            r => r.CreateRaceAsync(It.IsAny<Race>()),
            Times.Once);
    }

    [Fact]
    public async Task SaveRace_Should_ReturnNull_WhenRaceIsNull()
    {
        // Act
        var result = await _sut.SaveRace(null);

        // Assert
        Assert.Null(result);

        _repositoryMock.Verify(
            r => r.CreateRaceAsync(It.IsAny<Race>()),
            Times.Never);
    }

    [Fact]
    public async Task SaveRace_Should_Rethrow_WhenRepositoryThrows()
    {
        // Arrange
        var race = new Race
        {
            RaceName = "Race",
            Teams = []
        };

        _repositoryMock
            .Setup(r => r.CreateRaceAsync(It.IsAny<Race>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(
            () => _sut.SaveRace(race));
    }

    [Fact]
    public async Task StartRace_Should_SetStatus_StartTime_AndTeamStartTimes()
    {
        // Arrange
        var race = new Race
        {
            Id = "1",
            RaceName = "Race",
            Status = RaceStatus.NotStarted,
            Teams =
            [
                new Team
                {
                    TeamNumber = "1",
                    OffSetMin = 0
                },
                new Team
                {
                    TeamNumber = "2",
                    OffSetMin = 2
                }
            ]
        };

        _repositoryMock
            .Setup(r => r.GetRaceAsync("1", "Race"))
            .ReturnsAsync(race);

        // Act
        await _sut.StartRace("1", "Race");

        // Assert
        Assert.Equal(RaceStatus.InProgress, race.Status);
        Assert.NotNull(race.RaceStart);

        Assert.All(race.Teams,
            team => Assert.False(
                string.IsNullOrWhiteSpace(team.TeamStartTime)));

        _repositoryMock.Verify(
            r => r.UpdateRaceAsync(race),
            Times.Once);
    }

    [Fact]
    public async Task TeamFinished_Should_ReturnNull_WhenTeamDoesNotExist()
    {
        // Arrange
        var race = new Race
        {
            Teams =
            [
                new Team { TeamNumber = "1" }
            ]
        };

        _repositoryMock
            .Setup(r => r.GetRaceAsync("raceId", "race"))
            .ReturnsAsync(race);

        // Act
        var result = await _sut.TeamFinished(
            "99",
            "raceId",
            "race");

        // Assert
        Assert.Null(result);

        _repositoryMock.Verify(
            r => r.UpdateRaceAsync(It.IsAny<Race>()),
            Times.Never);
    }

    [Fact]
    public async Task TeamFinished_Should_SetDurationFinishTimeAndSpeed()
    {
        // Arrange
        var team = new Team
        {
            TeamNumber = "1",
            TeamStartTime = DateTime.Now
                .AddMinutes(-30)
                .ToString()
        };

        var race = new Race
        {
            DistanceKm = 20,
            Teams = [team]
        };

        _repositoryMock
            .Setup(r => r.GetRaceAsync("1", "Test"))
            .ReturnsAsync(race);

        // Act
        var result = await _sut.TeamFinished(
            "1",
            "1",
            "Test");

        // Assert
        Assert.NotNull(result);
        Assert.False(
            string.IsNullOrWhiteSpace(result!.TeamFinishTime));

        Assert.False(
            string.IsNullOrWhiteSpace(result.TeamDuration));

        Assert.True(result.SpeedKm > 0);

        _repositoryMock.Verify(
            r => r.UpdateRaceAsync(race),
            Times.Once);
    }

    [Fact]
    public async Task TeamFinished_Should_SetZeroDuration_WhenStartTimeIsInFuture()
    {
        // Arrange
        var team = new Team
        {
            TeamNumber = "1",
            TeamStartTime = DateTime.Now
                .AddMinutes(10)
                .ToString()
        };

        var race = new Race
        {
            Teams = [team]
        };

        _repositoryMock
            .Setup(r => r.GetRaceAsync("1", "Race"))
            .ReturnsAsync(race);

        // Act
        var result = await _sut.TeamFinished(
            "1",
            "1",
            "Race");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(
            "00:00:00:00",
            result!.TeamDuration);
    }

    [Fact]
    public async Task RaceFinished_Should_MarkRaceAsFinished()
    {
        // Arrange
        var race = new Race
        {
            Status = RaceStatus.InProgress,
            Teams =
            [
                new Team
                {
                    TeamNumber = "1",
                    TeamFinishTime = null
                },
                new Team
                {
                    TeamNumber = "2",
                    TeamFinishTime = "12:30:00"
                }
            ]
        };

        _repositoryMock
            .Setup(r => r.GetRaceAsync("1", "Race"))
            .ReturnsAsync(race);

        // Act
        await _sut.RaceFinished("1", "Race");

        // Assert
        Assert.Equal(RaceStatus.Finished, race.Status);
        Assert.NotNull(race.RaceFinish);
        Assert.Equal("DNF", race.Teams[0].TeamFinishTime);

        _repositoryMock.Verify(
            r => r.UpdateRaceAsync(race),
            Times.Once);
    }

    [Fact]
    public async Task GetRace_Should_ReturnRace()
    {
        // Arrange
        var race = new Race
        {
            Id = "1",
            RaceName = "Test Race"
        };

        _repositoryMock
            .Setup(r => r.GetRaceAsync("1", "Test Race"))
            .ReturnsAsync(race);

        // Act
        var result = await _sut.GetRace(
            "1",
            "Test Race");

        // Assert
        Assert.Same(race, result);
    }

    [Fact]
    public async Task UpComingRaces_Should_ReturnOnlyNotStartedAndInProgress()
    {
        // Arrange
        var races = new List<Race>
        {
            new()
            {
                Status = RaceStatus.NotStarted
            },
            new()
            {
                Status = RaceStatus.InProgress
            },
            new()
            {
                Status = RaceStatus.Finished
            }
        };

        _repositoryMock
            .Setup(r => r.GetRacesAsync())
            .ReturnsAsync(races);

        // Act
        var result = await _sut.UpComingRaces();

        // Assert
        Assert.Equal(2, result.Count);

        Assert.DoesNotContain(
            result,
            r => r.Status == RaceStatus.Finished);
    }

    [Fact]
    public async Task GetRaceStatistics_Should_ReturnStatistics()
    {
        // Arrange
        var race = new Race
        {
            RaceStart = DateTime.Now.AddHours(-1),
            RaceFinish = DateTime.Now,
            Teams =
            [
                new Team
                {
                    TeamStartTime = "10:00",
                    TeamFinishTime = "11:00"
                },
                new Team
                {
                    TeamStartTime = "10:05",
                    TeamFinishTime = null
                }
            ]
        };

        _repositoryMock
            .Setup(r => r.GetRaceAsync("1", "Race"))
            .ReturnsAsync(race);

        // Act
        var result = await _sut.GetRaceStatistics(
            "1",
            "Race");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result!.Started);
        Assert.Equal(1, result.Finished);

        Assert.False(
            string.IsNullOrWhiteSpace(result.StartTime));

        Assert.False(
            string.IsNullOrWhiteSpace(result.FinishTime));
    }

    [Fact]
    public async Task GetRace_Should_Rethrow_WhenRepositoryThrows()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetRaceAsync("1", "Race"))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(
            () => _sut.GetRace("1", "Race"));
    }

    [Fact]
    public async Task UpComingRaces_Should_Rethrow_WhenRepositoryThrows()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetRacesAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(
            () => _sut.UpComingRaces());
    }
}