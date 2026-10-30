using System.Security.Claims;
using System.Text.Encodings.Web;
using eTimeTrial.Web.Backend;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace eTimeTrial.Web.Test.Backend;

public class EasyAuthHandlerTests
{
    private readonly IOptionsMonitor<AuthenticationSchemeOptions> _options;
    private readonly ILoggerFactory _loggerFactory;
    private readonly UrlEncoder _encoder;
    private readonly ISystemClock _clock;

    public EasyAuthHandlerTests()
    {
        _options = new TestOptionsMonitor();
        _loggerFactory = LoggerFactory.Create(builder => { });
        _encoder = UrlEncoder.Default;
        _clock = new SystemClock();
    }

    [Fact]
    public async Task HandleAuthenticateAsync_Should_ReturnSuccess_WhenUserIsAuthenticated()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "James")
        };

        var identity = new ClaimsIdentity(
            claims,
            authenticationType: "TestAuth");

        var principal = new ClaimsPrincipal(identity);

        var context = new DefaultHttpContext
        {
            User = principal
        };

        var handler = CreateHandler(context);

        // Act
        var result = await handler.TestHandleAuthenticateAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Ticket);
        Assert.Equal("EasyAuth", result.Ticket.AuthenticationScheme);
        Assert.Same(principal, result.Principal);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_Should_ReturnNoResult_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var identity = new ClaimsIdentity();

        var principal = new ClaimsPrincipal(identity);

        var context = new DefaultHttpContext
        {
            User = principal
        };

        var handler = CreateHandler(context);

        // Act
        var result = await handler.TestHandleAuthenticateAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.None);
        Assert.Null(result.Ticket);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_Should_ReturnNoResult_WhenUserIsNull()
    {
        // Arrange
        var context = new DefaultHttpContext
        {
            User = null!
        };

        var handler = CreateHandler(context);

        // Act
        var result = await handler.TestHandleAuthenticateAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.None);
        Assert.Null(result.Ticket);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_Should_ReturnNoResult_WhenIdentityIsNull()
    {
        // Arrange
        var principal = new ClaimsPrincipal();

        var context = new DefaultHttpContext
        {
            User = principal
        };

        var handler = CreateHandler(context);

        // Act
        var result = await handler.TestHandleAuthenticateAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.None);
        Assert.Null(result.Ticket);
    }

    private TestEasyAuthHandler CreateHandler(HttpContext context)
    {
        var handler = new TestEasyAuthHandler(
            _options,
            _loggerFactory,
            _encoder,
            _clock);

        handler.InitializeAsync(
            new AuthenticationScheme(
                "EasyAuth",
                "EasyAuth",
                typeof(TestEasyAuthHandler)),
            context).GetAwaiter().GetResult();

        return handler;
    }
}

/// <summary>
/// Exposes protected HandleAuthenticateAsync for testing.
/// </summary>
public class TestEasyAuthHandler : EasyAuthHandler
{
    public TestEasyAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    public Task<AuthenticateResult> TestHandleAuthenticateAsync()
    {
        return HandleAuthenticateAsync();
    }
}

/// <summary>
/// Minimal options monitor implementation for tests.
/// </summary>
public class TestOptionsMonitor : IOptionsMonitor<AuthenticationSchemeOptions>
{
    private readonly AuthenticationSchemeOptions _options = new();

    public AuthenticationSchemeOptions CurrentValue => _options;

    public AuthenticationSchemeOptions Get(string? name)
    {
        return _options;
    }

    public IDisposable? OnChange(
        Action<AuthenticationSchemeOptions, string?> listener)
    {
        return null;
    }
}