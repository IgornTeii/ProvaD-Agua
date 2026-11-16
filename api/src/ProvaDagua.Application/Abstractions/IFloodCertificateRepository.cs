using ProvaDagua.Domain.Certificates;

namespace ProvaDagua.Application.Abstractions;

public interface IFloodCertificateRepository
{
    void Add(FloodCertificate certificate);
}