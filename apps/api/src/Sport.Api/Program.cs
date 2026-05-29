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
    .AddDisciplineModule<AthModule>();

builder.Services.AddSportInfrastructure();
builder.Services.AddOpenApi();
builder.Services.AddUnifiedProblemDetails();

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

if (!app.Environment.IsEnvironment("Testing"))
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
