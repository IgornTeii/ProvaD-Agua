using Microsoft.EntityFrameworkCore;
using ProvaDagua.Api.Auth;
using ProvaDagua.Api.Workers;
using ProvaDagua.Application.Alerts;
using ProvaDagua.Application.FloodEvents;
using ProvaDagua.Application.Readings;
using ProvaDagua.Infrastructure;
using ProvaDagua.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.Configure<SensorAuthOptions>(builder.Configuration.GetSection("SensorAuth"));

builder.Services.AddScoped<RegisterReadingHandler>();
builder.Services.AddScoped<EvaluateSuspectedEventsHandler>();
builder.Services.AddScoped<CloseRecededEventsHandler>();
builder.Services.AddScoped<SubscribeToAlertsHandler>();
builder.Services.AddHostedService<FloodEventEvaluationWorker>();

builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins("http://localhost:3000", "http://localhost:5173").AllowAnyHeader().AllowAnyMethod()));

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
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Prova d'Água API v1"));
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
