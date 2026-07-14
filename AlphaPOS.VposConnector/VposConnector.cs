using System;
using System.Runtime.InteropServices;
using System.IO;
using AlphaPOS.VposConnector.Application;
using AlphaPOS.VposConnector.Domain;
using AlphaPOS.VposConnector.Infrastructure.Config;
using AlphaPOS.VposConnector.Infrastructure.Http;
using AlphaPOS.VposConnector.Infrastructure.Logging;
using AlphaPOS.VposConnector.Infrastructure.Security;
using System.Globalization;
using System.Text;

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

        public string LastCode { get; private set; }
        public string LastMessage { get; private set; }
        public string LastStatus { get; private set; }
        public string LastSequence { get; private set; }
        public string LastVoucher { get; private set; }
        public string LastOrderId { get; private set; }
        public string LastReference { get; private set; }
        public string LastRawResponse { get; private set; }

        public VposConnector()
        {
            var defaultLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AlphaPOS.VposConnector.log");
            _logger = new FileLogger(defaultLog);
            ClearLastResult();
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

        public int Ping()
        {
            if (_service == null) return SetTechnicalError("NO_INIT");
            return SetResultFromResponse(_service.TestConnection());
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

        public void ClearLastResult()
        {
            LastCode = string.Empty;
            LastMessage = string.Empty;
            LastStatus = string.Empty;
            LastSequence = string.Empty;
            LastVoucher = string.Empty;
            LastOrderId = string.Empty;
            LastReference = string.Empty;
            LastRawResponse = string.Empty;
        }

        public int TerminarServicio()
        {
            if (_service == null) return SetTechnicalError("NO_INIT");
            return SetResultFromResponse(_service.TerminateService());
        }

        public int PagarTarjetaCredito(double monto, string cedula, string referencia, string terminalVirtual, double montoDonativo)
        {
            return PagarTarjeta("Credito", monto, cedula, "", referencia, terminalVirtual, montoDonativo);
        }

        public int PagarTarjetaDebito(double monto, string cedula, string referencia, string terminalVirtual, double montoDonativo)
        {
            return PagarTarjeta("Debito", monto, cedula, "", referencia, terminalVirtual, montoDonativo);
        }

        public int PagarTarjetaOtras(double monto, string cedula, string medioPago, string referencia, string terminalVirtual, double montoDonativo)
        {
            return PagarTarjeta("N/A", monto, cedula, medioPago, referencia, terminalVirtual, montoDonativo);
        }

        public int AnularTarjetaPorSecuencia(string secuencia, string cedula, string terminalVirtual)
        {
            if (_service == null) return SetTechnicalError("NO_INIT");

            var json = BuildJson(
                Tuple.Create("accion", "anulacion", false),
                Tuple.Create("numSeq", secuencia, false),
                Tuple.Create("cedula", cedula, false),
                Tuple.Create("terminalVirtual", terminalVirtual, false));

            return SetResultFromResponse(_service.ExecuteMetodo(json));
        }

        public int VerificarP2C(double monto, string telefono, string banco, string terminalVirtual)
        {
            if (_service == null) return SetTechnicalError("NO_INIT");

            var json = BuildJson(
                Tuple.Create("accion", "verificacionP2C", false),
                Tuple.Create("montoTransaccion", monto.ToString(CultureInfo.InvariantCulture), true),
                Tuple.Create("telefono", telefono, false),
                Tuple.Create("banco", banco, false),
                Tuple.Create("terminalVirtual", terminalVirtual, false));

            return SetResultFromResponse(_service.ExecuteMetodo(json));
        }

        public int VerificarTransferencia(double monto, string cuenta, string banco, string terminalVirtual)
        {
            if (_service == null) return SetTechnicalError("NO_INIT");

            var json = BuildJson(
                Tuple.Create("accion", "verificacionTransferencia", false),
                Tuple.Create("montoTransaccion", monto.ToString(CultureInfo.InvariantCulture), true),
                Tuple.Create("cuenta", cuenta, false),
                Tuple.Create("banco", banco, false),
                Tuple.Create("terminalVirtual", terminalVirtual, false));

            return SetResultFromResponse(_service.ExecuteMetodo(json));
        }

        public int VerificarDeposito(double monto, string cuenta, string banco, string terminalVirtual)
        {
            if (_service == null) return SetTechnicalError("NO_INIT");

            var json = BuildJson(
                Tuple.Create("accion", "verificacionDeposito", false),
                Tuple.Create("montoTransaccion", monto.ToString(CultureInfo.InvariantCulture), true),
                Tuple.Create("cuenta", cuenta, false),
                Tuple.Create("banco", banco, false),
                Tuple.Create("terminalVirtual", terminalVirtual, false));

            return SetResultFromResponse(_service.ExecuteMetodo(json));
        }

        public int PagarConCambio(double monto, string cedula, string tipoMoneda, string terminalVirtual)
        {
            if (_service == null) return SetTechnicalError("NO_INIT");

            var json = BuildJson(
                Tuple.Create("accion", "cambio", false),
                Tuple.Create("montoTransaccion", monto.ToString(CultureInfo.InvariantCulture), true),
                Tuple.Create("cedula", cedula, false),
                Tuple.Create("tipoMoneda", tipoMoneda, false),
                Tuple.Create("terminalVirtual", terminalVirtual, false));

            return SetResultFromResponse(_service.ExecuteMetodo(json));
        }

        public int PagarBiopago(double monto, string cedula, string terminalVirtual)
        {
            if (_service == null) return SetTechnicalError("NO_INIT");

            var json = BuildJson(
                Tuple.Create("accion", "serviciosExternos", false),
                Tuple.Create("montoTransaccion", monto.ToString(CultureInfo.InvariantCulture), true),
                Tuple.Create("cedula", cedula, false),
                Tuple.Create("terminalVirtual", terminalVirtual, false));

            return SetResultFromResponse(_service.ExecuteMetodo(json));
        }

        public int EjecutarPrecierre()
        {
            if (_service == null) return SetTechnicalError("NO_INIT");

            var json = BuildJson(Tuple.Create("accion", "precierre", false));
            return SetResultFromResponse(_service.ExecuteMetodo(json));
        }

        public int EjecutarCierre()
        {
            if (_service == null) return SetTechnicalError("NO_INIT");

            var json = BuildJson(Tuple.Create("accion", "cierre", false));
            return SetResultFromResponse(_service.ExecuteMetodo(json));
        }

        public int CardsConsultarSaldo(string cedula)
        {
            if (_service == null) return SetTechnicalError("NO_INIT");

            var json = BuildJson(
                Tuple.Create("accion", "consultaSaldoTFisica", false),
                Tuple.Create("cedula", cedula, false));

            return SetResultFromResponse(_service.ExecuteCards(json));
        }

        public int CardsPagarConOtp(string cedula, double monto)
        {
            if (_service == null) return SetTechnicalError("NO_INIT");

            var json = BuildJson(
                Tuple.Create("accion", "compraCards", false),
                Tuple.Create("cedula", cedula, false),
                Tuple.Create("saldoPagar", monto.ToString(CultureInfo.InvariantCulture), true));

            return SetResultFromResponse(_service.ExecuteCards(json));
        }

        public int LystoSolicitarOrden(double monto, string terminalVirtual)
        {
            return ExecuteLystoAction(
                Tuple.Create("accion", "solicitudLysto", false),
                Tuple.Create("montoTransaccion", monto.ToString(CultureInfo.InvariantCulture), true),
                Tuple.Create("terminalVirtual", terminalVirtual, false));
        }

        public int LystoCrearOrden(double monto, string cedula, string tipoFinanciamiento, string terminalVirtual)
        {
            return ExecuteLystoAction(
                Tuple.Create("accion", "creacionLysto", false),
                Tuple.Create("montoTransaccion", monto.ToString(CultureInfo.InvariantCulture), true),
                Tuple.Create("cedula", cedula, false),
                Tuple.Create("tipoFinanciamiento", tipoFinanciamiento, false),
                Tuple.Create("terminalVirtual", terminalVirtual, false));
        }

        public int LystoPagarCuotaInicialC2P(double monto, string idOrden, string terminalVirtual)
        {
            return ExecuteLystoAction(
                Tuple.Create("accion", "confirmacionLysto", false),
                Tuple.Create("montoTransaccion", monto.ToString(CultureInfo.InvariantCulture), true),
                Tuple.Create("idOrden", idOrden, false),
                Tuple.Create("medioPago", "merchant", false),
                Tuple.Create("terminalVirtual", terminalVirtual, false));
        }

        public int LystoConfirmarOrden(double monto, string idOrden, string medioPago, string terminalVirtual)
        {
            return ExecuteLystoAction(
                Tuple.Create("accion", "confirmacionLysto", false),
                Tuple.Create("montoTransaccion", monto.ToString(CultureInfo.InvariantCulture), true),
                Tuple.Create("idOrden", idOrden, false),
                Tuple.Create("medioPago", medioPago, false),
                Tuple.Create("terminalVirtual", terminalVirtual, false));
        }

        public int LystoCancelarOrden(string numSeqOrden, string terminalVirtual)
        {
            return ExecuteLystoAction(
                Tuple.Create("accion", "cancelacionLysto", false),
                Tuple.Create("numSeqOrden", numSeqOrden, false),
                Tuple.Create("terminalVirtual", terminalVirtual, false));
        }

        public int CasheaCrearOrden(double monto, string cedula, string otp, string terminalVirtual)
        {
            if (_service == null) return SetTechnicalError("NO_INIT");

            var json = BuildJson(
                Tuple.Create("accion", "crearOrdenCashea", false),
                Tuple.Create("montoTransaccion", monto.ToString(CultureInfo.InvariantCulture), true),
                Tuple.Create("cedula", cedula, false),
                Tuple.Create("otp", otp, false),
                Tuple.Create("terminalVirtual", terminalVirtual, false));

            return SetResultFromResponse(_service.ExecuteMetodo(json));
        }

        public int CasheaConfirmarOrden(string idOrden, double monto, string terminalVirtual)
        {
            if (_service == null) return SetTechnicalError("NO_INIT");

            var json = BuildJson(
                Tuple.Create("accion", "confirmacionCashea", false),
                Tuple.Create("idOrden", idOrden, false),
                Tuple.Create("montoTransaccion", monto.ToString(CultureInfo.InvariantCulture), true),
                Tuple.Create("terminalVirtual", terminalVirtual, false));

            return SetResultFromResponse(_service.ExecuteMetodo(json));
        }

        public int CasheaCancelarOrden(string idOrden, string terminalVirtual)
        {
            if (_service == null) return SetTechnicalError("NO_INIT");

            var json = BuildJson(
                Tuple.Create("accion", "cancelacionCashea", false),
                Tuple.Create("idOrden", idOrden, false),
                Tuple.Create("terminalVirtual", terminalVirtual, false));

            return SetResultFromResponse(_service.ExecuteMetodo(json));
        }

        public int PagarCrixto(double monto, string terminalVirtual)
        {
            return PagarTarjeta("N/A", monto, string.Empty, "Crixto", string.Empty, terminalVirtual, 0);
        }

        public int PagarXcapit(double monto, string telefono, string otp, string terminalVirtual)
        {
            if (_service == null) return SetTechnicalError("NO_INIT");

            var json = BuildJson(
                Tuple.Create("accion", "tarjeta", false),
                Tuple.Create("montoTransaccion", monto.ToString(CultureInfo.InvariantCulture), true),
                Tuple.Create("medioPago", "Xcapit", false),
                Tuple.Create("telefono", telefono, false),
                Tuple.Create("otp", otp, false),
                Tuple.Create("terminalVirtual", terminalVirtual, false));

            return SetResultFromResponse(_service.ExecuteMetodo(json));
        }

        public int PagarAccessPay(double monto, string terminalVirtual)
        {
            return PagarTarjeta("N/A", monto, string.Empty, "Access Pay", string.Empty, terminalVirtual, 0);
        }

        public int ImprimirUltimoVoucherAprobado()
        {
            if (_service == null) return SetTechnicalError("NO_INIT");

            var json = BuildJson(Tuple.Create("accion", "imprimeUltimoVoucher", false));
            return SetResultFromResponse(_service.ExecuteMetodo(json));
        }

        public int ImprimirUltimoVoucherProcesado()
        {
            if (_service == null) return SetTechnicalError("NO_INIT");

            var json = BuildJson(Tuple.Create("accion", "imprimeUltimoVoucherP", false));
            return SetResultFromResponse(_service.ExecuteMetodo(json));
        }

        public int ExecuteRaw(string endpoint, string jsonRequest)
        {
            if (_service == null) return SetTechnicalError("NO_INIT");

            if (string.Equals(endpoint, "metodo", StringComparison.OrdinalIgnoreCase))
                return SetResultFromResponse(_service.ExecuteMetodo(jsonRequest));
            if (string.Equals(endpoint, "cards", StringComparison.OrdinalIgnoreCase))
                return SetResultFromResponse(_service.ExecuteCards(jsonRequest));
            if (string.Equals(endpoint, "lysto", StringComparison.OrdinalIgnoreCase))
                return SetResultFromResponse(_service.ExecuteLysto(jsonRequest));
            if (string.Equals(endpoint, "terminate", StringComparison.OrdinalIgnoreCase))
                return SetResultFromResponse(_service.TerminateService());

            return SetTechnicalError("UNKNOWN_ENDPOINT");
        }

        private int PagarTarjeta(string tipoTarjeta, double monto, string cedula, string medioPago, string referencia, string terminalVirtual, double montoDonativo)
        {
            if (_service == null) return SetTechnicalError("NO_INIT");

            var json = BuildJson(
                Tuple.Create("accion", "tarjeta", false),
                Tuple.Create("montoTransaccion", monto.ToString(CultureInfo.InvariantCulture), true),
                Tuple.Create("cedula", cedula, false),
                Tuple.Create("tipoTarjeta", tipoTarjeta, false),
                Tuple.Create("medioPago", medioPago, false),
                Tuple.Create("referencia", referencia, false),
                Tuple.Create("terminalVirtual", terminalVirtual, false),
                Tuple.Create("montoDonativo", montoDonativo.ToString(CultureInfo.InvariantCulture), true));

            return SetResultFromResponse(_service.ExecuteMetodo(json));
        }

        private int ExecuteLystoAction(params Tuple<string, string, bool>[] values)
        {
            if (_service == null) return SetTechnicalError("NO_INIT");
            var json = BuildJson(values);
            return SetResultFromResponse(_service.ExecuteLysto(json));
        }

        private int SetResultFromResponse(string response)
        {
            LastRawResponse = response ?? string.Empty;

            if (string.IsNullOrWhiteSpace(response))
            {
                LastCode = "EMPTY_RESPONSE";
                LastMessage = "Respuesta vacia";
                LastStatus = "ERROR";
                return -1;
            }

            var normalized = response.ToLowerInvariant();
            if (normalized.Contains("error") || normalized.Contains("exception") || normalized.Contains("timeout") || normalized.StartsWith("error"))
            {
                LastCode = "TECH_ERROR";
                LastMessage = response;
                LastStatus = "ERROR";
                return -1;
            }

            if (normalized.Contains("rechaz") || normalized.Contains("declin") || normalized.Contains("deneg"))
            {
                LastCode = "REJECTED";
                LastMessage = response;
                LastStatus = "REJECTED";
                return 0;
            }

            LastCode = "OK";
            LastMessage = "Operacion procesada";
            LastStatus = "APPROVED";
            return 1;
        }

        private int SetTechnicalError(string message)
        {
            LastCode = "TECH_ERROR";
            LastMessage = message;
            LastStatus = "ERROR";
            LastRawResponse = message;
            return -1;
        }

        private static string BuildJson(params Tuple<string, string, bool>[] values)
        {
            var sb = new StringBuilder();
            sb.Append('{');
            var first = true;

            foreach (var item in values)
            {
                if (item == null) continue;
                var key = item.Item1;
                var value = item.Item2;
                var isNumber = item.Item3;
                if (string.IsNullOrEmpty(key)) continue;
                if (string.IsNullOrEmpty(value) && !isNumber) continue;

                if (!first) sb.Append(',');
                first = false;
                sb.Append('"').Append(JsonEscape(key)).Append('"').Append(':');

                if (isNumber)
                {
                    sb.Append(string.IsNullOrWhiteSpace(value) ? "0" : value);
                }
                else
                {
                    sb.Append('"').Append(JsonEscape(value)).Append('"');
                }
            }

            sb.Append('}');
            return sb.ToString();
        }

        private static string JsonEscape(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
