using eTimeTrial.Web.Backend;
using eTimeTrial.Web.Backend.DataService;
using eTimeTrial.Web.Backend.Orchestrator;
//using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Azure.Cosmos;
//using Microsoft.IdentityModel.Tokens;
//using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationInsightsTelemetry();

// ------------------------------------------------------------
// 1. Razor Pages + Controllers
// ------------------------------------------------------------
//builder.Services.AddRazorPages(options =>
//{
//    options.Conventions.AuthorizeFolder("/", "PagePolicy");
//    options.Conventions.AllowAnonymousToPage("/AccessDenied");
//});
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// ------------------------------------------------------------
// 2. Swagger with JWT Bearer support
// ------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "eTimeTrial API", Version = "v1" });

    //c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    //{
    //    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
    //    Description = "Enter JWT Bearer token",
    //    Name = "Authorization",
    //    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
    //    Scheme = "bearer",
    //    BearerFormat = "JWT"
    //});

    //c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    //{
    //    {
    //        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    //        {
    //            Reference = new Microsoft.OpenApi.Models.OpenApiReference
    //            {
    //                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
    //                Id = "Bearer"
    //            }
    //        },
    //        Array.Empty<string>()
    //    }
    //});
});

// ------------------------------------------------------------
// 3. Authentication: Cookies (UI) + OpenID Connect + JWT Bearer (API)
// ------------------------------------------------------------

var tenantId = "8f3c88c1-8477-49a2-a4a3-388052b55f75";
var frontendClientId = "2c84ea02-7314-4bd8-aa9e-20d37c53388e"; // Razor Pages login
var apiClientId = "975bdfbb-2eee-45e7-bda8-4d79336067fd";      // API audience

//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
//})
//.AddCookie(options =>
//{
//    options.AccessDeniedPath = "/AccessDenied";
//})

//.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
//{
//    options.ClientId = frontendClientId;
//    options.ClientSecret = builder.Configuration["AzureAd:FrontendClientSecret"];
//    options.Authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";
//    options.CallbackPath = "/signin-oidc";
//    options.ResponseType = "code";
//    options.SaveTokens = true;
//    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

//    options.Scope.Add("openid");
//    options.Scope.Add("profile");
//    options.Scope.Add("email");
//    options.TokenValidationParameters = new TokenValidationParameters { NameClaimType = "name" };
//});
//.AddJwtBearer("Bearer", options =>
//{
//    options.Authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";
//    options.Audience = $"api://{apiClientId}";
//});


// ------------------------------------------------------------
// 4. Authorization Policies
// ------------------------------------------------------------
//builder.Services.AddAuthorization(options =>
//{
//    // Razor Pages use cookie auth
//    options.AddPolicy("PagePolicy", policy =>
//    {
//        policy.RequireAuthenticatedUser();
//       // policy.RequireClaim("this-claim-does-not-exist");
//    });



//});
builder.Services.AddAuthorization();
// ------------------------------------------------------------
// 5. Cosmos + orchestrators
// ------------------------------------------------------------
builder.Services.AddHttpClient();

builder.Services.Configure<CosmosDbOptions>(
builder.Configuration.GetSection("CosmosDb"));
builder.Services.AddSingleton(sp =>
{
    var options =
    sp.GetRequiredService<IConfiguration>()
    .GetSection("CosmosDb")
    .Get<CosmosDbOptions>();

    return new CosmosClient(
    options!.Endpoint,
    options.Key);
});
builder.Services.AddScoped<ICosmosContext,CosmosContext>();
builder.Services.AddScoped<IRaceRepository,CosmosRaceRepository>();
//builder.Services.AddScoped<IRaceService,RaceService>();
//var cosmosConnectionString = builder.Configuration["CosmosDb:CosmosDBConnection"];
//builder.Services.AddSingleton<CosmosClient>(sp =>
//{
//   // var conn = builder.Configuration.GetConnectionString("CosmosDBConnection");
//    return new CosmosClient(cosmosConnectionString);
//});
//builder.Services.AddSingleton<ICosmosDataService, CosmosDataService>();
builder.Services.AddScoped<IRaceOrchestrator, RaceOrchestrator>();

var app = builder.Build();

// ------------------------------------------------------------
// 6. Pipeline
// ------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication + Authorization
//app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<GlobalExceptionMiddleware>();


// Endpoints
app.MapControllers();
app.MapRazorPages();

app.Run();
