using ProvaDagua.Domain.Common;

namespace ProvaDagua.Domain.Certificates;

public static class FloodCertificateErrors
{
    public static readonly Error EventNotClosed = new("certificate.event_not_closed", "A certificate can only be issued for a closed flood event.");
    public static readonly Error AddressRequired = new("certificate.address_required", "Address is required.");
    public static readonly Error VerificationCodeRequired = new("certificate.verification_code_required", "Verification code is required.");
}