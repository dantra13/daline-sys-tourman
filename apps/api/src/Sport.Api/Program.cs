using Scalar.AspNetCore;
using Sport.Core.DisciplineRegistry;
using Sport.Disciplines.ATH;
using Sport.Disciplines.BDM;
using Sport.Disciplines.BKB;
using Sport.Disciplines.BOX;
using Sport.Disciplines.FBL;
using Sport.Disciplines.VBV;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSportCore()
    .AddDisciplineModule<FblModule>()
    .AddDisciplineModule<BkbModule>()
    .AddDisciplineModule<BdmModule>()
    .AddDisciplineModule<VbvModule>()
    .AddDisciplineModule<BoxModule>()
    .AddDisciplineModule<AthModule>();

builder.Services.AddOpenApi();

var app = builder.Build();

app.Services.BuildSportRegistry();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapGet("/",             () => Results.Ok(new { name = "Sport.Api" }));
app.MapGet("/health",       () => Results.Ok(new { status = "alive" }));
app.MapGet("/health/ready", () => Results.Ok(new { status = "ready" }));

app.Run();

public partial class Program { }
