using ProvaDagua.Application.Readings;
using ProvaDagua.Api.Auth;
using ProvaDagua.Infrastructure;
using ProvaDagua.Api.Workers;
using ProvaDagua.Application.FloodEvents;
using Microsoft.EntityFrameworkCore;
using ProvaDagua.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<RegisterReadingHandler>();
builder.Services.Configure<SensorAuthOptions>(builder.Configuration.GetSection("SensorAuth"));
builder.Services.AddScoped<EvaluateSuspectedEventsHandler>();
builder.Services.AddScoped<CloseRecededEventsHandler>();  
builder.Services.AddHostedService<FloodEventEvaluationWorker>();

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/openapi/v1.json", "Prova d'Água API v1"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();