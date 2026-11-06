using System.IO;
using System.Net;
using AlphaPOS.VposConnector.Application;
using AlphaPOS.VposConnector.Infrastructure.Http;
using AlphaPOS.VposConnector.Infrastructure.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AlphaPOS.VposConnector.Tests
{
    [TestClass]
    public class TransactionServiceTests
    {
        private class FakeHttpClientWrapper : HttpClientWrapper
        {
            public string LastUri { get; private set; }
            public string LastMethod { get; private set; }
            public string LastBody { get; private set; }

            public FakeHttpClientWrapper(FileLogger logger) : base(logger)
            {
            }

            protected override string ExecuteRequest(HttpWebRequest req, string body)
            {
                LastUri = req.RequestUri.ToString();
                LastMethod = req.Method;
                LastBody = body;
                return "OK";
            }
        }

        [TestMethod]
        public void ExecuteMetodo_UsesMetodoEndpoint()
        {
            var logger = new FileLogger(Path.Combine(Path.GetTempPath(), "ts_test.log"));
            var fake = new FakeHttpClientWrapper(logger);
            var svc = new TransactionService("https://api.test", fake, logger);

            var resp = svc.ExecuteMetodo("{\"accion\":\"tarjeta\"}");

            Assert.AreEqual("OK", resp);
            Assert.AreEqual("POST", fake.LastMethod);
            Assert.AreEqual("https://api.test/vpos/metodo", fake.LastUri);
        }

        [TestMethod]
        public void TestConnection_UsesObtenerMediosPagoAction()
        {
            var logger = new FileLogger(Path.Combine(Path.GetTempPath(), "ts_test.log"));
            var fake = new FakeHttpClientWrapper(logger);
            var svc = new TransactionService("http://localhost:8085", fake, logger);

            var resp = svc.TestConnection();

            Assert.AreEqual("OK", resp);
            StringAssert.Contains(fake.LastBody, "\"accion\":\"obtenerMediosPago\"");
        }
    }
}
