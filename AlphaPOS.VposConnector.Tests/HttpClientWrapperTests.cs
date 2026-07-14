using System;
using System.IO;
using System.Net;
using AlphaPOS.VposConnector.Infrastructure.Http;
using AlphaPOS.VposConnector.Infrastructure.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AlphaPOS.VposConnector.Tests
{
    [TestClass]
    public class HttpClientWrapperTests
    {
        private class OkHttpClientWrapper : HttpClientWrapper
        {
            public OkHttpClientWrapper(FileLogger logger) : base(logger)
            {
            }

            protected override string ExecuteRequest(HttpWebRequest req, string body)
            {
                return "OK_RESPONSE";
            }
        }

        private class FailingHttpClientWrapper : HttpClientWrapper
        {
            public FailingHttpClientWrapper(FileLogger logger) : base(logger)
            {
            }

            protected override string ExecuteRequest(HttpWebRequest req, string body)
            {
                throw new InvalidOperationException("simulated-failure");
            }
        }

        [TestMethod]
        public void Send_ReturnsResponse_WhenRequestSucceeds()
        {
            var logger = new FileLogger(Path.Combine(Path.GetTempPath(), "hcw_test.log"));
            var wrapper = new OkHttpClientWrapper(logger);

            var result = wrapper.Send("http://localhost:8085/vpos/metodo", "POST", "{\"accion\":\"tarjeta\"}", 10000);

            Assert.AreEqual("OK_RESPONSE", result);
        }

        [TestMethod]
        public void Send_ReturnsErrorPrefix_WhenRequestFails()
        {
            var logger = new FileLogger(Path.Combine(Path.GetTempPath(), "hcw_test.log"));
            var wrapper = new FailingHttpClientWrapper(logger);

            var result = wrapper.Send("http://localhost:8085/vpos/metodo", "POST", "{\"accion\":\"tarjeta\"}", 10000);

            StringAssert.StartsWith(result, "ERROR:");
        }
    }
}
