using ProvaDagua.Domain.Common;

namespace ProvaDagua.Domain.Alerts;

public static class AlertErrors
{
    public static readonly Error AddressRequired = new("alert.address_required", "Escolha um endereço.");
    public static readonly Error InvalidPhone = new("alert.invalid_phone", "Informe um celular com DDD, por exemplo (11) 91234-5678.");
    public static readonly Error AddressNotFound = new("alert.address_not_found", "Endereço não encontrado.");
}
