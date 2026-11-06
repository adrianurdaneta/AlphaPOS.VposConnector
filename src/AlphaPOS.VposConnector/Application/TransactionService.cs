using System;
using AlphaPOS.VposConnector.Domain;
using AlphaPOS.VposConnector.Infrastructure.Http;
using AlphaPOS.VposConnector.Infrastructure.Logging;

namespace AlphaPOS.VposConnector.Application
{
    public class TransactionService : ITransactionService
    {
        private string _baseUrl;
        private readonly HttpClientWrapper _http;
        private readonly FileLogger _logger;
        private int _timeoutMs = 120000;

        public string LastJsonRequest { get; private set; }
        public string LastJsonResponse { get; private set; }
        public string LastUrlRequest { get; private set; }

        public TransactionService(string baseUrl, HttpClientWrapper http, FileLogger logger)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            LastJsonRequest = string.Empty;
            LastJsonResponse = string.Empty;
            LastUrlRequest = string.Empty;
            SetBaseUrl(baseUrl);
        }

        public void SetBaseUrl(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new ArgumentException("baseUrl is required", nameof(baseUrl));
            }

            _baseUrl = baseUrl.TrimEnd('/');
            _logger.Info("SetBaseUrl: {0}", _baseUrl);
        }

        public void SetTimeout(int ms)
        {
            if (ms > 0) _timeoutMs = ms;
            _logger.Info("SetTimeout: {0}", _timeoutMs);
        }

        public string TestConnection()
        {
            return ExecuteMetodo("{\"accion\":\"obtenerMediosPago\"}");
        }

        public string ExecuteMetodo(string jsonRequest)
        {
            var url = _baseUrl + "/vpos/metodo";
            var request = jsonRequest ?? "{}";
            LastUrlRequest = url;
            LastJsonRequest = request;
            var response = _http.Send(url, "POST", request, _timeoutMs);
            LastJsonResponse = response ?? string.Empty;
            return response;
        }
    }
}
