using Microsoft.VisualStudio.TestTools.UnitTesting;
using AlphaPOS.VposConnector.Infrastructure.Http;
using AlphaPOS.VposConnector.Infrastructure.Security;
using AlphaPOS.VposConnector.Infrastructure.Logging;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net;

namespace AlphaPOS.VposConnector.Tests
{
    [TestClass]
    public class HttpClientWrapperTests
    {
        private class SequenceHttpClientWrapper : HttpClientWrapper
        {
            private int _calls = 0;
            public SequenceHttpClientWrapper(FileLogger logger, ClientCertificateProvider certProvider) : base(logger, certProvider) { }

            protected override string ExecuteRequest(HttpWebRequest req, string body)
            {
                _calls++;
                if (_calls == 1)
                {
                    throw new UnauthorizedException("Simulated unauthorized");
                }
                return "CERT_OK";
            }
        }

        [TestMethod]
        public void Send_WithApiKey_Unauthorized_Then_CertFallback_ReturnsCertOk()
        {
            var logger = new FileLogger(Path.Combine(Path.GetTempPath(), "hcw_test.log"));
            var certProv = new ClientCertificateProvider();
            certProv.SetCertificate(new X509Certificate2()); // empty cert for test

            var wrapper = new SequenceHttpClientWrapper(logger, certProv);
            wrapper.SetApiKey("dummy-key", "X-Api-Key");

            var result = wrapper.Send("https://api.test", "POST", "{\"monto\":1}", 10000);
            Assert.AreEqual("CERT_OK", result);
        }
    }
}