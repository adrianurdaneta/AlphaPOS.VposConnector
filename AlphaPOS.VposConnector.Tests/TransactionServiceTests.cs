using Microsoft.VisualStudio.TestTools.UnitTesting;
using AlphaPOS.VposConnector.Application;
using AlphaPOS.VposConnector.Infrastructure.Http;
using AlphaPOS.VposConnector.Infrastructure.Logging;
using AlphaPOS.VposConnector.Infrastructure.Security;
using System.IO;
using System.Net;

namespace AlphaPOS.VposConnector.Tests
{
    [TestClass]
    public class TransactionServiceTests
    {
        private class FakeHttpClientWrapper : HttpClientWrapper
        {
            public FakeHttpClientWrapper(FileLogger logger, ClientCertificateProvider certProvider) : base(logger, certProvider) { }

            protected override string ExecuteRequest(HttpWebRequest req, string body)
            {
                var uri = req.RequestUri.ToString();
                if (req.Method == "POST" && (uri == "https://api.test" || uri == "https://api.test/")) return "OK_POST";
                if (req.Method == "GET" && uri.StartsWith("https://api.test/status/")) return "STATUS_OK";
                if (req.Method == "GET" && uri.StartsWith("https://api.test/voucher/")) return "VOUCHER_OK";
                if (req.Method == "POST" && uri.EndsWith("/cancel")) return "CANCEL_OK";
                return "UNKNOWN";
            }
        }

        [TestMethod]
        public void StartTransaction_ReturnsOk()
        {
            var logger = new FileLogger(Path.Combine(Path.GetTempPath(), "ts_test.log"));
            var certProv = new ClientCertificateProvider();
            var fake = new FakeHttpClientWrapper(logger, certProv);
            var svc = new TransactionService("https://api.test", fake, logger, certProv);
            var resp = svc.StartTransaction("{\"monto\":1}");
            Assert.AreEqual("OK_POST", resp);
        }

        [TestMethod]
        public void PollStatus_ReturnsStatus()
        {
            var logger = new FileLogger(Path.Combine(Path.GetTempPath(), "ts_test.log"));
            var certProv = new ClientCertificateProvider();
            var fake = new FakeHttpClientWrapper(logger, certProv);
            var svc = new TransactionService("https://api.test", fake, logger, certProv);
            var resp = svc.PollStatus("TX123");
            Assert.AreEqual("STATUS_OK", resp);
        }

        [TestMethod]
        public void GetVoucher_ReturnsVoucher()
        {
            var logger = new FileLogger(Path.Combine(Path.GetTempPath(), "ts_test.log"));
            var certProv = new ClientCertificateProvider();
            var fake = new FakeHttpClientWrapper(logger, certProv);
            var svc = new TransactionService("https://api.test", fake, logger, certProv);
            var resp = svc.GetVoucher("TX123");
            Assert.AreEqual("VOUCHER_OK", resp);
        }

        [TestMethod]
        public void CancelTransaction_ReturnsCancel()
        {
            var logger = new FileLogger(Path.Combine(Path.GetTempPath(), "ts_test.log"));
            var certProv = new ClientCertificateProvider();
            var fake = new FakeHttpClientWrapper(logger, certProv);
            var svc = new TransactionService("https://api.test", fake, logger, certProv);
            var resp = svc.CancelTransaction("TX123");
            Assert.AreEqual("CANCEL_OK", resp);
        }
    }
}

