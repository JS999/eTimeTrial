using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace eTimeTrial.Web.Backend
{
    public class EasyAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public EasyAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock) { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Context.User?.Identity?.IsAuthenticated == true)
            {
                var ticket = new AuthenticationTicket(Context.User, "EasyAuth");
                return Task.FromResult(AuthenticateResult.Success(ticket));
            }

            return Task.FromResult(AuthenticateResult.NoResult());
        }
    }

}
