var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health");
app.MapGet("/api/info", () => Results.Ok(new
{
    service = "FieldOps.Api",
    phase = 1,
    status = "foundation",
    timestamp = DateTimeOffset.UtcNow
}));

app.Run();

public partial class Program;
