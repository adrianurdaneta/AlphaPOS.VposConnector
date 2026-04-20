using System;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace AlphaPOS.VposConnector.Infrastructure.Security
{
    public class ClientCertificateProvider
    {
        private X509Certificate2 _certificate;

        public ClientCertificateProvider() { }

        public ClientCertificateProvider(string path, string password)
        {
            LoadCertificate(path, password);
        }

        public void LoadCertificate(string path, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(path)) return;
                if (!File.Exists(path)) return;
                _certificate = new X509Certificate2(path, password);
            }
            catch
            {
                _certificate = null;
            }
        }

        public void SetCertificate(X509Certificate2 cert)
        {
            _certificate = cert;
        }

        public X509Certificate2 GetCertificate()
        {
            return _certificate;
        }
    }
}