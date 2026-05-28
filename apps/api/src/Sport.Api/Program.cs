var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { name = "Sport.Api" }));

app.Run();

// Required so WebApplicationFactory<Program> can find Program in tests.
public partial class Program { }
