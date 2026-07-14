using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using AlphaPOS.VposConnector.Application;
using AlphaPOS.VposConnector.Domain;
using AlphaPOS.VposConnector.Infrastructure.Http;
using AlphaPOS.VposConnector.Infrastructure.Logging;

namespace AlphaPOS.VposConnector
{
    [ComVisible(true)]
    [ProgId("AlphaPOS.VposConnector")]
    [Guid("D1A1DAF1-4C3E-4B49-A4F8-8F94B2D6EA1A")]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class VposConnector
    {
        private const string DefaultBaseUrl = "http://localhost:8085";

        private readonly FileLogger _logger;
        private readonly ITransactionService _service;

        public string LastCode { get; private set; }
        public string LastMessage { get; private set; }
        public string LastStatus { get; private set; }
        public string LastSequence { get; private set; }
        public string LastVoucher { get; private set; }
        public string LastOrderId { get; private set; }
        public string LastReference { get; private set; }
        public string LastRawResponse { get; private set; }
        public string LastJsonRequest { get; private set; }
        public string LastJsonResponse { get; private set; }
        public string LastUrlRequest { get; private set; }

        public VposConnector()
        {
            var defaultLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AlphaPOS.VposConnector.log");
            _logger = new FileLogger(defaultLog);
            var http = new HttpClientWrapper(_logger);
            _service = new TransactionService(DefaultBaseUrl, http, _logger);
            ClearLastResult();
        }

        public void SetBaseUrl(string baseUrl)
        {
            _service.SetBaseUrl(baseUrl);
        }

        public void SetLogPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            _logger.SetLogFile(path);
        }

        public void SetTimeout(int ms)
        {
            _service.SetTimeout(ms);
        }

        public string TestConnection()
        {
            var response = _service.TestConnection();
            LastJsonRequest = _service.LastJsonRequest ?? string.Empty;
            LastJsonResponse = _service.LastJsonResponse ?? (response ?? string.Empty);
            LastUrlRequest = _service.LastUrlRequest ?? string.Empty;
            return response;
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
            LastJsonRequest = string.Empty;
            LastJsonResponse = string.Empty;
            LastUrlRequest = string.Empty;
        }

        public int PagarTarjetaDebito(string monto, string cedula)
        {
            return PagarTarjeta(monto, cedula);
        }

        public int PagarTarjetaCredito(string monto, string cedula)
        {
            return PagarTarjeta(monto, cedula);
        }

        public int AnularTarjetaPorSecuencia(string secuencia, string cedula)
        {
            var json = BuildJson(
                Tuple.Create("accion", "anulacion", false),
                Tuple.Create("numSeq", secuencia, false),
                Tuple.Create("cedula", cedula, false));

            return ExecuteAndSetResult(json);
        }

        public int VerificarP2C(string monto, string telefono, string banco)
        {
            var json = BuildJson(
                Tuple.Create("accion", "verificacionP2C", false),
                Tuple.Create("montoTransaccion", monto, true),
                Tuple.Create("telefono", telefono, false),
                Tuple.Create("banco", banco, false));

            return ExecuteAndSetResult(json);
        }

        public int PagarConCambio(string monto, string cedula, string tipoMoneda)
        {
            var json = BuildJson(
                Tuple.Create("accion", "cambio", false),
                Tuple.Create("montoTransaccion", monto, true),
                Tuple.Create("cedula", cedula, false),
                Tuple.Create("tipoMoneda", tipoMoneda, false));

            return ExecuteAndSetResult(json);
        }

        public int PagarBiopago(string monto, string cedula)
        {
            var json = BuildJson(
                Tuple.Create("accion", "serviciosExternos", false),
                Tuple.Create("montoTransaccion", monto, true),
                Tuple.Create("cedula", cedula, false));

            return ExecuteAndSetResult(json);
        }

        public int ObtenerMediosPago()
        {
            var json = BuildJson(Tuple.Create("accion", "obtenerMediosPago", false));
            return ExecuteAndSetResult(json);
        }

        public int ImprimirUltimoVoucherAprobado()
        {
            var json = BuildJson(Tuple.Create("accion", "imprimeUltimoVoucher", false));
            return ExecuteAndSetResult(json);
        }

        public int EjecutarPrecierre()
        {
            var json = BuildJson(Tuple.Create("accion", "precierre", false));
            return ExecuteAndSetResult(json);
        }

        public int EjecutarCierre()
        {
            var json = BuildJson(Tuple.Create("accion", "cierre", false));
            return ExecuteAndSetResult(json);
        }

        private int PagarTarjeta(string monto, string cedula)
        {
            var json = BuildJson(
                Tuple.Create("accion", "tarjeta", false),
                Tuple.Create("montoTransaccion", monto, true),
                Tuple.Create("cedula", cedula, false),
                Tuple.Create("medioPago", string.Empty, false));

            return ExecuteAndSetResult(json);
        }

        private int ExecuteAndSetResult(string json)
        {
            var response = _service.ExecuteMetodo(json);
            LastJsonRequest = _service.LastJsonRequest ?? (json ?? string.Empty);
            LastJsonResponse = _service.LastJsonResponse ?? (response ?? string.Empty);
            LastUrlRequest = _service.LastUrlRequest ?? string.Empty;
            return SetResultFromResponse(response);
        }

        private int SetResultFromResponse(string response)
        {
            LastRawResponse = response ?? string.Empty;
            LastJsonResponse = LastRawResponse;
            LastCode = string.Empty;
            LastMessage = string.Empty;
            LastStatus = string.Empty;
            LastSequence = string.Empty;
            LastVoucher = string.Empty;
            LastOrderId = string.Empty;
            LastReference = string.Empty;

            if (string.IsNullOrWhiteSpace(response))
            {
                LastCode = "EMPTY_RESPONSE";
                LastMessage = "Respuesta vacia";
                LastStatus = "ERROR";
                return -1;
            }

            LastCode = ExtractJsonString(response, "codRespuesta");
            LastMessage = ExtractJsonString(response, "mensajeRespuesta");
            LastSequence = ExtractJsonValue(response, "numSeq");
            LastVoucher = ExtractJsonString(response, "nombreVoucher");
            LastReference = ExtractJsonString(response, "numeroReferencia");
            LastOrderId = ExtractJsonString(response, "idOrden");

            if (string.IsNullOrEmpty(LastMessage))
            {
                LastMessage = response;
            }

            if (ContainsIgnoreCase(response, "ERROR:") || ContainsIgnoreCase(response, "exception"))
            {
                LastCode = string.IsNullOrEmpty(LastCode) ? "TECH_ERROR" : LastCode;
                LastStatus = "ERROR";
                return -1;
            }

            var approved = string.Equals(LastCode, "00", StringComparison.OrdinalIgnoreCase) || ContainsIgnoreCase(LastMessage, "APROBADA");
            if (approved)
            {
                LastCode = string.IsNullOrEmpty(LastCode) ? "00" : LastCode;
                LastStatus = "APPROVED";
                return 1;
            }

            var rejected = (!string.IsNullOrEmpty(LastCode) && !string.Equals(LastCode, "00", StringComparison.OrdinalIgnoreCase))
                || ContainsIgnoreCase(LastMessage, "RECHAZ")
                || ContainsIgnoreCase(LastMessage, "DECLIN")
                || ContainsIgnoreCase(LastMessage, "DENEG");

            if (rejected)
            {
                LastCode = string.IsNullOrEmpty(LastCode) ? "REJECTED" : LastCode;
                LastStatus = "REJECTED";
                return 0;
            }

            LastCode = string.IsNullOrEmpty(LastCode) ? "UNKNOWN_RESPONSE" : LastCode;
            LastStatus = "ERROR";
            return -1;
        }

        private static bool ContainsIgnoreCase(string value, string token)
        {
            return !string.IsNullOrEmpty(value) && value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string ExtractJsonString(string response, string key)
        {
            var pattern = "\""+ Regex.Escape(key) + "\"\\s*:\\s*\"([^\"]*)\"";
            var match = Regex.Match(response ?? string.Empty, pattern, RegexOptions.CultureInvariant);
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        private static string ExtractJsonValue(string response, string key)
        {
            var pattern = "\"" + Regex.Escape(key) + "\"\\s*:\\s*(\"([^\"]*)\"|[-]?[0-9]+)";
            var match = Regex.Match(response ?? string.Empty, pattern, RegexOptions.CultureInvariant);
            if (!match.Success) return string.Empty;

            var quotedValue = match.Groups[2].Value;
            if (!string.IsNullOrEmpty(quotedValue)) return quotedValue;
            return match.Groups[1].Value.Trim('"');
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
