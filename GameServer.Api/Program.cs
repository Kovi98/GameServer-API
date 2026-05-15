using GameServer.Api.Authentication;
using GameServer.Api.Options;
using GameServer.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<GameServerOptions>()
    .Bind(builder.Configuration.GetSection(GameServerOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(static options => options.MaxPlayers > 0, "GameServer:MaxPlayers must be greater than 0.")
    .Validate(static options => options.StartedAtUtc != default, "GameServer:StartedAtUtc must be configured.")
    .Validate(static options => options.Randomization.MinOnlinePlayers >= 0, "GameServer:Randomization:MinOnlinePlayers must be >= 0.")
    .Validate(static options => options.Randomization.MaxOnlinePlayers >= 0, "GameServer:Randomization:MaxOnlinePlayers must be >= 0.")
    .Validate(static options => options.Randomization.MinPingMs >= 0, "GameServer:Randomization:MinPingMs must be >= 0.")
    .Validate(static options => options.Randomization.MaxPingMs >= 0, "GameServer:Randomization:MaxPingMs must be >= 0.")
    .Validate(static options => options.Randomization.MaxOnlinePlayers >= options.Randomization.MinOnlinePlayers,
        "GameServer:Randomization:MaxOnlinePlayers must be >= MinOnlinePlayers.")
    .Validate(static options => options.Randomization.MaxPingMs >= options.Randomization.MinPingMs,
        "GameServer:Randomization:MaxPingMs must be >= MinPingMs.")
    .ValidateOnStart();

builder.Services.AddSingleton<IGameServerFakeDataService, GameServerFakeDataService>();
builder.Services
    .AddAuthentication(ApiKeyAuthenticationHandler.SchemeName)
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationHandler.SchemeName,
        static _ => { });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(ApiKeyAuthenticationHandler.PolicyName, policy =>
    {
        policy.AddAuthenticationSchemes(ApiKeyAuthenticationHandler.SchemeName);
        policy.RequireAuthenticatedUser();
    });

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "GameServer API";
        document.Info.Version = "v1";
        document.Info.Description = "Fake game server endpoints secured by an API key passed in the X-API-KEY header.";

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();
        document.Components.SecuritySchemes[ApiKeyAuthenticationHandler.SchemeName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Name = ApiKeyAuthenticationHandler.HeaderName,
            Description = "API key required for all /api endpoints."
        };

        return Task.CompletedTask;
    });
});

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseAuthentication();
app.UseAuthorization();

var api = app.MapGroup("/api")
    .WithTags("Game Server")
    .RequireAuthorization(ApiKeyAuthenticationHandler.PolicyName);

api.MapGet("/server/status", Ok<ServerStatusDto> (IGameServerFakeDataService fakeDataService) =>
{
    var status = fakeDataService.GetServerStatus();
    return TypedResults.Ok(status);
})
.WithName("GetServerStatus")
.WithSummary("Gets current fake game server status.")
.WithDescription("Returns the current state of the fake game server including player counts, map, version and generation timestamp.")
.Produces<ServerStatusDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized)
.Produces(StatusCodes.Status403Forbidden)
.WithOpenApi(operation =>
{
    operation.Security =
    [
        new OpenApiSecurityRequirement
        {
            [
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = ApiKeyAuthenticationHandler.SchemeName
                    }
                }
            ] = Array.Empty<string>()
        }
    ];

    return operation;
});

api.MapGet("/players", Ok<IReadOnlyList<PlayerDto>> (IGameServerFakeDataService fakeDataService) =>
{
    var players = fakeDataService.GetPlayers();
    return TypedResults.Ok(players);
})
.WithName("GetPlayers")
.WithSummary("Gets fake game players.")
.WithDescription("Returns the list of configured players including their online state, ban state, level, ping and last activity.")
.Produces<IReadOnlyList<PlayerDto>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized)
.Produces(StatusCodes.Status403Forbidden)
.WithOpenApi(operation =>
{
    operation.Security =
    [
        new OpenApiSecurityRequirement
        {
            [
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = ApiKeyAuthenticationHandler.SchemeName
                    }
                }
            ] = Array.Empty<string>()
        }
    ];

    return operation;
});

app.Run();
