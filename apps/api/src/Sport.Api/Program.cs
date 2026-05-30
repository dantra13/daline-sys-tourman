using JasperFx.CodeGeneration.Model;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using Sport.Api.Endpoints.Competitions;
using Sport.Api.ErrorHandling;
using Sport.Application;
using Sport.Core.DisciplineRegistry;
using Sport.Disciplines.ATH;
using Sport.Disciplines.BDM;
using Sport.Disciplines.BKB;
using Sport.Disciplines.BOX;
using Sport.Disciplines.FBL;
using Sport.Disciplines.JUD;
using Sport.Disciplines.VBV;
using Sport.Infrastructure;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSportCore()
    .AddDisciplineModule<FblModule>()
    .AddDisciplineModule<BkbModule>()
    .AddDisciplineModule<BdmModule>()
    .AddDisciplineModule<VbvModule>()
    .AddDisciplineModule<BoxModule>()
    .AddDisciplineModule<AthModule>()
    .AddDisciplineModule<JudModule>();

builder.Services.AddSportInfrastructure();
builder.Services.AddOpenApi();
builder.Services.AddUnifiedProblemDetails();

// Keep minimal-API body-binding failures consistent across environments. The framework defaults
// ThrowOnBadRequest to true in Development, which makes the binder throw BadHttpRequestException
// (swallowed by ExceptionHandlingMiddleware as a 500) instead of short-circuiting with a 400.
// Forcing it false routes every parse failure through the unified request.malformed envelope.
builder.Services.Configure<RouteHandlerOptions>(o => o.ThrowOnBadRequest = false);

builder.Host.UseWolverine(opts =>
{
    opts.Discovery.IncludeAssembly(typeof(AssemblyMarker).Assembly);
    // EF Core registers DbContextOptions<T> via an opaque lambda factory that Wolverine's
    // codegen cannot inline-resolve. The default in Wolverine 6.x is NotAllowed, which
    // throws InvalidServiceLocationException at first dispatch. AllowedButWarn restores
    // working behaviour while still surfacing the opaque-factory diagnostic.
    opts.ServiceLocationPolicy = ServiceLocationPolicy.AllowedButWarn;
});

var app = builder.Build();

app.Services.BuildSportRegistry();

if (builder.Configuration.GetValue("RunMigrationsOnStartup", true))
{
    using (var scope = app.Services.CreateScope())
    {
        var runner = scope.ServiceProvider.GetRequiredService<SportMigrationRunner>();
        await runner.ApplyAsync();
    }
}

app.UseStatusCodePages();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapGet("/",       () => Results.Ok(new { name = "Sport.Api" }));
app.MapGet("/health", () => Results.Ok(new { status = "alive" }));
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
});

app.MapCompetitionEndpoints();

app.Run();

public partial class Program { }
