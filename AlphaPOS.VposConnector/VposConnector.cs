using System;
using System.Runtime.InteropServices;
using System.IO;
using AlphaPOS.VposConnector.Application;
using AlphaPOS.VposConnector.Domain;
using AlphaPOS.VposConnector.Infrastructure.Config;
using AlphaPOS.VposConnector.Infrastructure.Http;
using AlphaPOS.VposConnector.Infrastructure.Logging;
using AlphaPOS.VposConnector.Infrastructure.Security;

namespace AlphaPOS.VposConnector
{
    [ComVisible(true)]
    [ProgId("AlphaPOS.VposConnector")]
    [Guid("D1A1DAF1-4C3E-4B49-A4F8-8F94B2D6EA1A")]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class VposConnector
    {
        private ITransactionService _service;
        private FileLogger _logger;
        private IniConfiguration _config;
        private ClientCertificateProvider _certProvider;

        public VposConnector()
        {
            var defaultLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AlphaPOS.VposConnector.log");
            _logger = new FileLogger(defaultLog);
        }

        public bool Init(string iniPath)
        {
            try
            {
                _config = new IniConfiguration(iniPath);
                var baseUrl = _config.Get("General", "Merchant_Server")?.Trim();
                if (string.IsNullOrEmpty(baseUrl)) return false;

                _certProvider = new ClientCertificateProvider();
                var certPath = _config.Get("General", "ClientCertPath");
                var certPass = _config.Get("General", "ClientCertPassword");
                if (!string.IsNullOrEmpty(certPath)) _certProvider.LoadCertificate(certPath, certPass);

                var http = new HttpClientWrapper(_logger, _certProvider);
                _service = new TransactionService(baseUrl, http, _logger, _certProvider);

                var apiKey = _config.Get("General", "ApiKey");
                var apiKeyHeader = _config.Get("General", "ApiKeyHeader");
                if (!string.IsNullOrEmpty(apiKey)) _service.SetApiKey(apiKey, string.IsNullOrEmpty(apiKeyHeader) ? "X-Api-Key" : apiKeyHeader);

                var timeout = _config.Get("General", "TimeoutMs");
                if (int.TryParse(timeout, out int t) && t > 0) _service.SetTimeout(t);

                _logger.Info("Init completed. baseUrl={0}", baseUrl);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error("Init error: {0}", ex.Message);
                return false;
            }
        }

        public void SetApiKey(string key)
        {
            if (_service == null) { _logger.Error("SetApiKey: service not initialized"); return; }
            _service.SetApiKey(key);
        }

        public void SetApiKeyWithHeader(string key, string header)
        {
            if (_service == null) { _logger.Error("SetApiKeyWithHeader: service not initialized"); return; }
            _service.SetApiKey(key, header);
        }

        public void SetClientCertificate(string path, string password)
        {
            if (_certProvider == null) _certProvider = new ClientCertificateProvider();
            _certProvider.LoadCertificate(path, password);
        }

        public void SetLogPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            _logger.SetLogFile(path);
        }

        public void SetTimeout(int ms)
        {
            _service?.SetTimeout(ms);
        }

        public string TestConnection()
        {
            if (_service == null) return "NO_INIT";
            return _service.TestConnection();
        }

        public string StartTransaction(string jsonRequest)
        {
            if (_service == null) return "NO_INIT";
            return _service.StartTransaction(jsonRequest);
        }

        public string PollStatus(string transactionId)
        {
            if (_service == null) return "NO_INIT";
            return _service.PollStatus(transactionId);
        }

        public string GetVoucher(string transactionId)
        {
            if (_service == null) return "NO_INIT";
            return _service.GetVoucher(transactionId);
        }

        public string CancelTransaction(string transactionId)
        {
            if (_service == null) return "NO_INIT";
            return _service.CancelTransaction(transactionId);
        }
    }
}
