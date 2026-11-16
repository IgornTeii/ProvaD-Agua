using ProvaDagua.Application.Abstractions;
using ProvaDagua.Domain.Certificates;

namespace ProvaDagua.Infrastructure.Persistence.Repositories;

internal sealed class FloodCertificateRepository(AppDbContext db) : IFloodCertificateRepository
{
    public void Add(FloodCertificate certificate) => db.FloodCertificates.Add(certificate);
}