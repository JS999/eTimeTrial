using System.Text.Json;
using eTimeTrial.Web.Backend;
using eTimeTrial.Web.Backend.Entities;
using eTimeTrial.Web.Backend.Orchestrator;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace eTimeTrial.Web.Test.Backend.Orchestrator;

public class TimeTrialControllerTests
{
    private readonly Mock<IRaceOrchestrator> _orchestratorMock;
    private readonly TimeTrialController _controller;

    public TimeTrialControllerTests()
    {
        _orchestratorMock = new Mock<IRaceOrchestrator>();
        _controller = new TimeTrialController(_orchestratorMock.Object);
    }

    [Fact]
    public async Task GetUpComingRaces_Returns_Races()
    {
        var races = new List<Race>
        {
            new() { Id = "1", RaceName = "Race 1" }
        };

        _orchestratorMock
            .Setup(x => x.UpComingRaces())
            .ReturnsAsync(races);

        var result = await _controller.GetUpComingRaces();

        Assert.Single(result);
        Assert.Equal("Race 1", result[0].RaceName);
    }

    [Fact]
    public void CalculateSpeed_Returns_Zero_When_Duration_Is_Zero()
    {
        var result = _controller.CalculateSpeed(10, "00:00:00");

        Assert.Equal("0", result);
    }

    [Fact]
    public void CalculateSpeed_Returns_Correct_Speed()
    {
        var result = _controller.CalculateSpeed(20, "00:30:00");

        Assert.Equal("40", result);
    }

    [Fact]
    public void CalculateDuration_Returns_Zero_When_StartTime_After_EndTime()
    {
        var result = _controller.CalculateDuration(
            DateTime.Now,
            DateTime.Now.AddMinutes(-1));

        Assert.Equal("\"00:00:00:00\"", result);
    }

    [Fact]
    public void CalculateDuration_Returns_Formatted_Duration()
    {
        var start = new DateTime(2025, 1, 1, 10, 0, 0);
        var end = start.AddMinutes(5).AddSeconds(30);

        var result = _controller.CalculateDuration(start, end);

        Assert.Contains("00:05:30", result);
    }

    [Fact]
    public void GetStartTime_Returns_Time_String()
    {
        var result = _controller.GetStartTime();

        Assert.False(string.IsNullOrWhiteSpace(result));
    }

    [Fact]
    public async Task StartRace_Calls_Orchestrator()
    {
        var result = await _controller.StartRace(
            "1",
            "Race");

        _orchestratorMock.Verify(
            x => x.StartRace("1", "Race"),
            Times.Once);

        Assert.False(string.IsNullOrWhiteSpace(result));
    }

    [Fact]
    public async Task GetRace_Returns_TestRace_When_Id_Is_999()
    {
        var result = await _controller.GetRace(
            "999",
            "Anything");

        Assert.NotNull(result);

        Assert.Equal("999", result!.Id);
        Assert.Equal("test", result.RaceName);
        Assert.Single(result.Teams);
    }

    [Fact]
    public async Task GetRace_Returns_Race_From_Orchestrator()
    {
        var race = new Race
        {
            Id = "1",
            RaceName = "Summer Race"
        };

        _orchestratorMock
            .Setup(x => x.GetRace("1", "Summer Race"))
            .ReturnsAsync(race);

        var result = await _controller.GetRace(
            "1",
            "Summer Race");

        Assert.NotNull(result);
        Assert.Equal("Summer Race", result!.RaceName);
    }

    [Fact]
    public async Task GetRaceStatistics_Returns_Statistics()
    {
        var stats = new RaceStatistics
        {
            Started = 10,
            Finished = 8
        };

        _orchestratorMock
            .Setup(x => x.GetRaceStatistics("1", "Race"))
            .ReturnsAsync(stats);

        var result = await _controller.GetRaceStatistics(
            "1",
            "Race");

        Assert.NotNull(result);
        Assert.Equal(10, result!.Started);
        Assert.Equal(8, result.Finished);
    }

    [Fact]
    public void GetTeamStartTime_Returns_Offset_Time()
    {
        var startTime = new DateTime(
            2025,
            1,
            1,
            10,
            0,
            0);

        var result = _controller.GetTeamStartTime(
            startTime,
            5);

        Assert.Contains("10:05:00", result);
    }

    [Fact]
    public void GetFinishTime_Returns_Time_String()
    {
        var result = _controller.GetFinishTime();

        Assert.False(string.IsNullOrWhiteSpace(result));
    }

    [Fact]
    public void Download_Returns_Csv_File()
    {
        var results = new List<Result>
        {
            new Result(),
            new Result()
        };

        var json =
            JsonSerializer.SerializeToElement(results);

        var result = _controller.Download(json);

        Assert.NotNull(result);
        Assert.IsType<FileContentResult>(result);

        var file = (FileContentResult)result;

        Assert.Equal("text/csv", file.ContentType);
        Assert.Equal("results.csv", file.FileDownloadName);
        Assert.NotEmpty(file.FileContents);
    }

    [Fact]
    public async Task SaveTeamResult_Returns_Team()
    {
        var team = new Team
        {
            TeamNumber = "1"
        };

        _orchestratorMock
            .Setup(x => x.TeamFinished(
                "1",
                "race1",
                "race"))
            .ReturnsAsync(team);

        var result = await _controller.SaveTeamResult(
            "1",
            "race1",
            "race");

        Assert.NotNull(result);
        Assert.Equal("1", result!.TeamNumber);
    }

    [Fact]
    public async Task SaveTeamResult_Returns_Null_When_Exception_Occurs()
    {
        _orchestratorMock
            .Setup(x => x.TeamFinished(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ThrowsAsync(new Exception());

        var result = await _controller.SaveTeamResult(
            "1",
            "race1",
            "race");

        Assert.Null(result);
    }

    [Fact]
    public async Task NewRace_Returns_Saved_Race()
    {
        var race = new Race
        {
            RaceName = "Test Race"
        };

        _orchestratorMock
            .Setup(x => x.SaveRace(It.IsAny<Race>()))
            .ReturnsAsync((Race r) => r);

        var json =
            JsonSerializer.SerializeToElement(race);

        var result = await _controller.NewRace(json);

        Assert.NotNull(result);
        Assert.Equal("Test Race", result!.RaceName);

        _orchestratorMock.Verify(
            x => x.SaveRace(It.IsAny<Race>()),
            Times.Once);
    }

    [Fact]
    public async Task NewRace_Returns_Null_When_Exception_Occurs()
    {
        _orchestratorMock
            .Setup(x => x.SaveRace(It.IsAny<Race>()))
            .ThrowsAsync(new Exception());

        var race = new Race
        {
            RaceName = "Test Race"
        };

        var json =
            JsonSerializer.SerializeToElement(race);

        var result = await _controller.NewRace(json);

        Assert.Null(result);
    }
}