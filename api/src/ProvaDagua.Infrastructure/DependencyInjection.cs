using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProvaDagua.Application.Abstractions;
using ProvaDagua.Application.Queries;
using ProvaDagua.Infrastructure.Ai;
using ProvaDagua.Infrastructure.Certificates;
using ProvaDagua.Infrastructure.Persistence;
using ProvaDagua.Infrastructure.Persistence.Repositories;

namespace ProvaDagua.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Database")));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IReadingRepository, ReadingRepository>();
        services.AddScoped<IFloodEventRepository, FloodEventRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IFloodCertificateRepository, FloodCertificateRepository>();
        services.AddScoped<IAlertRepository, AlertRepository>();
        services.AddScoped<IReadQueries, ReadQueries>();
        services.AddSingleton<IVerificationCodeGenerator, VerificationCodeGenerator>();

        services.AddHttpClient<IFloodClassifier, FloodClassifierHttpClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["Ai:BaseUrl"] ?? "http://localhost:8000");
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        return services;
    }
}
