using System.Security.Cryptography;
using ProvaDagua.Application.Abstractions;

namespace ProvaDagua.Infrastructure.Certificates;

public sealed class VerificationCodeGenerator : IVerificationCodeGenerator
{
    private const string Alphabet = "23456789ABCDEFGHJKMNPQRSTUVWXYZ";

    public string Generate()
    {
        var part1 = RandomNumberGenerator.GetString(Alphabet, 4);
        var part2 = RandomNumberGenerator.GetString(Alphabet, 4);
        return $"PDA-{part1}-{part2}";
    }
}