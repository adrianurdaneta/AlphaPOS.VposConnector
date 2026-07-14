using System;
using System.IO;
using System.Net;
using System.Text;
using AlphaPOS.VposConnector.Infrastructure.Logging;

namespace AlphaPOS.VposConnector.Infrastructure.Http
{
    public class HttpClientWrapper
    {
        private readonly FileLogger _logger;

        public HttpClientWrapper(FileLogger logger)
        {
            _logger = logger;
        }

        public string Send(string url, string method, string body, int timeoutMs)
        {
            _logger?.Info("Send: {0} {1} (timeout={2})", method, url, timeoutMs);

            try
            {
                var req = CreateRequest(url, method, timeoutMs);
                return ExecuteRequest(req, body);
            }
            catch (Exception ex)
            {
                _logger?.Error("Send failed: {0}", ex.Message);
                return "ERROR: " + ex.Message;
            }
        }

        private static HttpWebRequest CreateRequest(string url, string method, int requestTimeout)
        {
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = method ?? "GET";
            req.Timeout = requestTimeout > 0 ? requestTimeout : 10000;
            req.ReadWriteTimeout = req.Timeout;
            return req;
        }

        protected virtual string ExecuteRequest(HttpWebRequest req, string body)
        {
            try
            {
                if (!string.IsNullOrEmpty(body))
                {
                    var data = Encoding.UTF8.GetBytes(body);
                    req.ContentType = "application/json";
                    req.ContentLength = data.Length;
                    using (var s = req.GetRequestStream()) s.Write(data, 0, data.Length);
                }

                using (var resp = (HttpWebResponse)req.GetResponse())
                using (var sr = new StreamReader(resp.GetResponseStream()))
                {
                    return sr.ReadToEnd();
                }
            }
            catch (WebException wex)
            {
                var resp = wex.Response as HttpWebResponse;
                if (resp != null)
                {
                    try
                    {
                        using (var sr = new StreamReader(resp.GetResponseStream()))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                    catch
                    {
                    }
                }

                throw;
            }
        }
    }
}
