using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Gibbon.Git.Server.Services;

namespace Gibbon.Git.Server.Security;

public class CertificateService(IPathResolver pathResolver) : ICertificateService
{
    private const string CertificateFileName = "gibbon.crt";
    private const string PrivateKeyFileName = "gibbon.pfx";
    private readonly IPathResolver _pathResolver = pathResolver;

    public void CreatePrivateCertificate()
    {
        var privateKeyPath = _pathResolver.ResolveRoot(PrivateKeyFileName);
        var certificatePath = _pathResolver.ResolveRoot(CertificateFileName);

        using var rsa = RSA.Create(2048);
        var certificateRequest = new CertificateRequest("CN=localhost", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        certificateRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
        certificateRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, false));
        certificateRequest.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(certificateRequest.PublicKey, false));

        var sanBuilder = new SubjectAlternativeNameBuilder();
        sanBuilder.AddDnsName("localhost");
        certificateRequest.CertificateExtensions.Add(sanBuilder.Build());

        var certificate = certificateRequest.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddYears(1));

        var pfxBytes = certificate.Export(X509ContentType.Pfx);
        File.WriteAllBytes(privateKeyPath, pfxBytes);

        var certBytes = certificate.Export(X509ContentType.Cert);
        File.WriteAllBytes(certificatePath, certBytes);
    }

    public X509Certificate2 GetPrivateCertificate()
    {
        var privateKeyPath = _pathResolver.ResolveRoot(PrivateKeyFileName);
        if (!File.Exists(privateKeyPath))
        {
            CreatePrivateCertificate();
        }
        return new X509Certificate2(privateKeyPath);
    }

    public X509Certificate2 GetPublicCertificate()
    {
        var certificatePath = _pathResolver.ResolveRoot(CertificateFileName);
        if (!File.Exists(certificatePath))
        {
            CreatePrivateCertificate();
        }
        var certBytes = File.ReadAllBytes(certificatePath);
        return new X509Certificate2(certBytes);
    }
}
