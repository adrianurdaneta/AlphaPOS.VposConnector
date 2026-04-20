using System;
using System.IO;
using System.Net;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using AlphaPOS.VposConnector.Infrastructure.Logging;
using AlphaPOS.VposConnector.Infrastructure.Security;

namespace AlphaPOS.VposConnector.Infrastructure.Http
{
    public class HttpClientWrapper
    {
        private readonly FileLogger _logger;
        private readonly ClientCertificateProvider _certProvider;
        private string _apiKey;
        private string _apiKeyHeader = "X-Api-Key";

        public HttpClientWrapper(FileLogger logger, ClientCertificateProvider certProvider)
        {
            _logger = logger;
            _certProvider = certProvider;
        }

        public void SetApiKey(string apiKey, string header)
        {
            _apiKey = apiKey;
            if (!string.IsNullOrEmpty(header)) _apiKeyHeader = header;
        }

        public string Send(string url, string method, string body, int timeoutMs)
        {
            _logger?.Info("Send: {0} {1} (timeout={2})", method, url, timeoutMs);

            // Try API Key
            if (!string.IsNullOrEmpty(_apiKey))
            {
                try
                {
                    var req = CreateRequest(url, method, timeoutMs);
                    req.Headers[_apiKeyHeader] = _apiKey;
                    return ExecuteRequest(req, body);
                }
                catch (UnauthorizedException)
                {
                    _logger?.Info("API key unauthorized, will try certificate fallback if available.");
                }
                catch (Exception ex)
                {
                    _logger?.Error("API key attempt failed: {0}", ex.Message);
                }
            }

            var cert = _certProvider?.GetCertificate();
            if (cert != null)
            {
                try
                {
                    var req2 = CreateRequest(url, method, timeoutMs);
                    req2.ClientCertificates.Add(cert);
                    return ExecuteRequest(req2, body);
                }
                catch (Exception ex)
                {
                    _logger?.Error("Client cert attempt failed: {0}", ex.Message);
                    return "ERROR: " + ex.Message;
                }
            }

            try
            {
                var req3 = CreateRequest(url, method, timeoutMs);
                return ExecuteRequest(req3, body);
            }
            catch (Exception ex)
            {
                _logger?.Error("No-auth attempt failed: {0}", ex.Message);
                return "ERROR: " + ex.Message;
            }
        }

        private HttpWebRequest CreateRequest(string url, string method, int requestTimeout)
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
                if (resp != null && ((int)resp.StatusCode == 401 || (int)resp.StatusCode == 403))
                {
                    throw new UnauthorizedException("Unauthorized");
                }
                if (resp != null)
                {
                    try { using (var sr = new StreamReader(resp.GetResponseStream())) return sr.ReadToEnd(); } catch { }
                }
                throw;
            }
        }
    }
}