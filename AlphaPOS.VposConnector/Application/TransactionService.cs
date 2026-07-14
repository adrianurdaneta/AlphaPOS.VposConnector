using System;
using AlphaPOS.VposConnector.Domain;
using AlphaPOS.VposConnector.Infrastructure.Http;
using AlphaPOS.VposConnector.Infrastructure.Logging;
using AlphaPOS.VposConnector.Infrastructure.Security;

namespace AlphaPOS.VposConnector.Application
{
    public class TransactionService : ITransactionService
    {
        private readonly string _baseUrl;
        private readonly HttpClientWrapper _http;
        private readonly FileLogger _logger;
        private readonly ClientCertificateProvider _certProvider;
        private string _apiKey;
        private string _apiKeyHeader = "X-Api-Key";
        private int _timeoutMs = 120000;

        public TransactionService(string baseUrl, HttpClientWrapper http, FileLogger logger, ClientCertificateProvider certProvider)
        {
            _baseUrl = baseUrl?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(baseUrl));
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _certProvider = certProvider;
        }

        public void SetApiKey(string apiKey, string header = "X-Api-Key")
        {
            _apiKey = apiKey;
            _apiKeyHeader = string.IsNullOrEmpty(header) ? "X-Api-Key" : header;
            _http.SetApiKey(apiKey, _apiKeyHeader);
            _logger.Info("SetApiKey: {0}", !string.IsNullOrEmpty(apiKey) ? "***" : "(null)");
        }

        public void SetTimeout(int ms)
        {
            if (ms > 0) _timeoutMs = ms;
            _logger.Info("SetTimeout: {0}", _timeoutMs);
        }

        public string TestConnection()
        {
            return _http.Send(_baseUrl, "GET", null, 5000);
        }

        public string StartTransaction(string jsonRequest)
        {
            return _http.Send(_baseUrl, "POST", jsonRequest ?? "{}", _timeoutMs);
        }

        public string ExecuteMetodo(string jsonRequest)
        {
            var url = _baseUrl + "/vpos/metodo";
            return _http.Send(url, "POST", jsonRequest ?? "{}", _timeoutMs);
        }

        public string ExecuteCards(string jsonRequest)
        {
            var url = _baseUrl + "/vpos/metodo_cards";
            return _http.Send(url, "POST", jsonRequest ?? "{}", _timeoutMs);
        }

        public string ExecuteLysto(string jsonRequest)
        {
            var url = _baseUrl + "/vpos/metodo_lysto";
            return _http.Send(url, "POST", jsonRequest ?? "{}", _timeoutMs);
        }

        public string TerminateService()
        {
            var url = _baseUrl + "/vpos/metodo_terminate";
            return _http.Send(url, "GET", null, 10000);
        }

        public string PollStatus(string transactionId)
        {
            var url = _baseUrl + "/status/" + Uri.EscapeDataString(transactionId ?? string.Empty);
            return _http.Send(url, "GET", null, 5000);
        }

        public string GetVoucher(string transactionId)
        {
            var url = _baseUrl + "/voucher/" + Uri.EscapeDataString(transactionId ?? string.Empty);
            return _http.Send(url, "GET", null, 5000);
        }

        public string CancelTransaction(string transactionId)
        {
            var url = _baseUrl + "/cancel";
            var payload = "{\"transactionId\":\"" + (transactionId ?? string.Empty) + "\"}";
            return _http.Send(url, "POST", payload, 10000);
        }
    }
}
