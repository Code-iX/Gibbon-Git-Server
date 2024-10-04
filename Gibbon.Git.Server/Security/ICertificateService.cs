using System.Security.Cryptography.X509Certificates;

namespace Gibbon.Git.Server.Security;

public interface ICertificateService
{
    void CreatePrivateCertificate();
    X509Certificate2 GetPublicCertificate();
    X509Certificate2 GetPrivateCertificate();
}
