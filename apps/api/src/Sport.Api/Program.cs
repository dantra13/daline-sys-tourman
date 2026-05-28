var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/",       () => Results.Ok(new { name = "Sport.Api" }));
app.MapGet("/health", () => Results.Ok(new { status = "alive" }));

app.Run();

public partial class Program { }
