using System;
using System.Collections.Generic;
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
        public int LastSequence { get; private set; }
        public string LastVoucher { get; private set; }
        public string LastVoucherPath { get; private set; }
        public string LastOrderId { get; private set; }
        public string LastReference { get; private set; }
        public string LastRawResponse { get; private set; }
        public string LastJsonRequest { get; private set; }
        public string LastJsonResponse { get; private set; }
        public string LastUrlRequest { get; private set; }
        public string LastTerminal { get; private set; }
        public string LastTid { get; private set; }
        public int LastLote { get; private set; }
        public int LastTipoTransaccion { get; private set; }
        public string LastFechaTransaccion { get; private set; }
        public string LastHoraTransaccion { get; private set; }
        public string LastNumeroAutorizacion { get; private set; }
        public string LastCodigoAfiliacion { get; private set; }
        public string LastCodigoAdquiriente { get; private set; }
        public string LastNroAutorizador { get; private set; }
        public string LastTipoTarjeta { get; private set; }
        public string LastTipoProducto { get; private set; }
        public string LastTipoCuenta { get; private set; }
        public int LastMedioPago { get; private set; }
        public double LastMontoTransaccion { get; private set; }
        public double LastMontoServicios { get; private set; }
        public double LastMontoDonativo { get; private set; }
        public string LastMoneda { get; private set; }
        public bool LastExisteCopiaVoucher { get; private set; }

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

        public int TestConnection()
        {
            var response = _service.TestConnection();
            LastJsonRequest = _service.LastJsonRequest ?? string.Empty;
            LastJsonResponse = _service.LastJsonResponse ?? (response ?? string.Empty);
            LastUrlRequest = _service.LastUrlRequest ?? string.Empty;

            if (string.IsNullOrWhiteSpace(response))
            {
                LastCode = "EMPTY_RESPONSE";
                LastMessage = "Sin respuesta del servidor";
                LastStatus = "ERROR";
                return -1;
            }

            var codRespuesta = ExtractJsonString(response, "codRespuesta");
            if (string.Equals(codRespuesta, "00", StringComparison.OrdinalIgnoreCase))
            {
                LastCode = "00";
                LastMessage = "Conexión exitosa";
                LastStatus = "CONNECTED";
                return 1;
            }

            LastCode = string.IsNullOrEmpty(codRespuesta) ? "UNKNOWN" : codRespuesta;
            LastMessage = ExtractJsonString(response, "mensajeRespuesta");
            LastStatus = "ERROR";
            return -1;
        }

        public void ClearLastResult()
        {
            LastCode = string.Empty;
            LastMessage = string.Empty;
            LastStatus = string.Empty;
            LastSequence = 0;
            LastVoucher = string.Empty;
            LastVoucherPath = string.Empty;
            LastOrderId = string.Empty;
            LastReference = string.Empty;
            LastRawResponse = string.Empty;
            LastJsonRequest = string.Empty;
            LastJsonResponse = string.Empty;
            LastUrlRequest = string.Empty;
            LastTerminal = string.Empty;
            LastTid = string.Empty;
            LastLote = 0;
            LastTipoTransaccion = 0;
            LastFechaTransaccion = string.Empty;
            LastHoraTransaccion = string.Empty;
            LastNumeroAutorizacion = string.Empty;
            LastCodigoAfiliacion = string.Empty;
            LastCodigoAdquiriente = string.Empty;
            LastNroAutorizador = string.Empty;
            LastTipoTarjeta = string.Empty;
            LastTipoProducto = string.Empty;
            LastTipoCuenta = string.Empty;
            LastMedioPago = -1;
            LastMontoTransaccion = 0.0;
            LastMontoServicios = 0.0;
            LastMontoDonativo = 0.0;
            LastMoneda = string.Empty;
            LastExisteCopiaVoucher = false;
        }

        public int PagarTarjetaDebito(double monto, string cedula)
        {
            return PagarTarjeta(monto, cedula);
        }

        public int PagarTarjetaCredito(double monto, string cedula)
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

        public int VerificarP2C(double monto, string cedula)
        {
            return VerificarMedioPago(monto, cedula, "MedioPagoP2C");
        }

        public int VerificarTransferencia(double monto, string cedula)
        {
            return VerificarMedioPago(monto, cedula, "MedioPagoTransferencia");
        }

        public int VerificarBiopago(double monto, string cedula)
        {
            return VerificarMedioPago(monto, cedula, "MedioPagoBiopago");
        }

        public int PagarConCambio(double monto, string cedula, string tipoMoneda)
        {
            var montoEnCentavos = ((long)Math.Round(monto * 100)).ToString();
            var json = BuildJson(
                Tuple.Create("accion", "cambio", false),
                Tuple.Create("montoTransaccion", montoEnCentavos, false),
                Tuple.Create("cedula", cedula, false),
                Tuple.Create("tipoMoneda", tipoMoneda, false));

            return ExecuteAndSetResult(json);
        }

        public int PagarBiopago(double monto, string cedula)
        {
            return VerificarBiopago(monto, cedula);
        }

        public string ObtenerMediosPago()
        {
            var json = BuildJson(Tuple.Create("accion", "obtenerMediosPago", false));
            var response = _service.ExecuteMetodo(json);
            LastJsonRequest = _service.LastJsonRequest ?? (json ?? string.Empty);
            LastJsonResponse = _service.LastJsonResponse ?? (response ?? string.Empty);
            LastUrlRequest = _service.LastUrlRequest ?? string.Empty;
            SetResultFromResponse(response);
            return ExtractMediosPagoCsv(response);
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

        private int PagarTarjeta(double monto, string cedula)
        {
            var montoEnCentavos = ((long)Math.Round(monto * 100)).ToString();
            var json = BuildJson(
                Tuple.Create("accion", "tarjeta", false),
                Tuple.Create("montoTransaccion", montoEnCentavos, false),
                Tuple.Create("cedula", cedula, false),
                Tuple.Create("medioPago", string.Empty, false));

            return ExecuteAndSetResult(json);
        }

        private int VerificarMedioPago(double monto, string cedula, string medioPago)
        {
            var montoEnCentavos = ((long)Math.Round(monto * 100)).ToString();
            var json = BuildJson(
                Tuple.Create("accion", "tarjeta", false),
                Tuple.Create("montoTransaccion", montoEnCentavos, false),
                Tuple.Create("cedula", cedula, false),
                Tuple.Create("medioPago", medioPago, false));

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
            LastSequence = 0;
            LastVoucher = string.Empty;
            LastVoucherPath = string.Empty;
            LastOrderId = string.Empty;
            LastReference = string.Empty;
            LastTerminal = string.Empty;
            LastTid = string.Empty;
            LastLote = 0;
            LastTipoTransaccion = 0;
            LastFechaTransaccion = string.Empty;
            LastHoraTransaccion = string.Empty;
            LastNumeroAutorizacion = string.Empty;
            LastCodigoAfiliacion = string.Empty;
            LastCodigoAdquiriente = string.Empty;
            LastNroAutorizador = string.Empty;
            LastTipoTarjeta = string.Empty;
            LastTipoProducto = string.Empty;
            LastTipoCuenta = string.Empty;
            LastMedioPago = -1;
            LastMontoTransaccion = 0.0;
            LastMontoServicios = 0.0;
            LastMontoDonativo = 0.0;
            LastMoneda = string.Empty;
            LastExisteCopiaVoucher = false;

            if (string.IsNullOrWhiteSpace(response))
            {
                LastCode = "EMPTY_RESPONSE";
                LastMessage = "Respuesta vacia";
                LastStatus = "ERROR";
                return -1;
            }

            LastCode = ExtractJsonString(response, "codRespuesta");
            LastMessage = ExtractJsonString(response, "mensajeRespuesta");
            LastSequence = ExtractJsonInt(response, "numSeq");
            if (LastSequence == 0)
            {
                LastSequence = ExtractJsonInt(response, "semJsd");
            }

            LastVoucher = ExtractJsonString(response, "nombreVoucher");
            LastVoucherPath = LastVoucher;
            LastReference = ExtractJsonString(response, "numeroReferencia");
            LastOrderId = ExtractJsonString(response, "idOrden");
            LastTerminal = ExtractJsonString(response, "terminal");
            LastTid = ExtractJsonString(response, "tid");
            LastLote = ExtractJsonInt(response, "numeroLote");
            LastTipoTransaccion = ExtractJsonInt(response, "tipoTransaccion");
            LastFechaTransaccion = ExtractJsonString(response, "fechaTransaccion");
            LastHoraTransaccion = ExtractJsonString(response, "horaTransaccion");
            LastNumeroAutorizacion = ExtractJsonString(response, "numeroAutorizacion");
            LastCodigoAfiliacion = ExtractJsonString(response, "codigoAfiliacion");
            LastCodigoAdquiriente = ExtractJsonString(response, "codigoAdquiriente");
            LastNroAutorizador = ExtractJsonString(response, "nroAutorizador");
            LastTipoTarjeta = ExtractJsonString(response, "tipoTarjeta");
            LastTipoProducto = ExtractJsonString(response, "tipoProducto");
            LastTipoCuenta = ExtractJsonString(response, "tipoCuenta");
            LastMedioPago = ExtractJsonInt(response, "medioPago");
            LastMontoTransaccion = ExtractJsonMonto(response, "montoTransaccion");
            LastMontoServicios = ExtractJsonMonto(response, "montoServicios");
            LastMontoDonativo = ExtractJsonMonto(response, "montoDonativo");
            LastMoneda = ExtractJsonString(response, "descrMoneda");
            LastExisteCopiaVoucher = ExtractJsonBool(response, "existeCopiaVoucher");

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
            var pattern = "\"" + Regex.Escape(key) + "\"\\s*:\\s*(\"([^\"]*)\"|[-]?[0-9]+(?:\\.[0-9]+)?|true|false|null)";
            var match = Regex.Match(response ?? string.Empty, pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            if (!match.Success) return string.Empty;

            var quotedValue = match.Groups[2].Value;
            if (!string.IsNullOrEmpty(quotedValue)) return quotedValue;

            var rawValue = match.Groups[1].Value.Trim('"');
            return string.Equals(rawValue, "null", StringComparison.OrdinalIgnoreCase) ? string.Empty : rawValue;
        }

        private static int ExtractJsonInt(string response, string key)
        {
            var value = ExtractJsonValue(response, key);
            return int.TryParse(value, out var result) ? result : 0;
        }

        private static double ExtractJsonDouble(string response, string key)
        {
            var value = ExtractJsonValue(response, key);
            return double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var result) ? result : 0.0;
        }

        private static double ExtractJsonMonto(string response, string key)
        {
            var value = ExtractJsonValue(response, key);
            if (string.IsNullOrWhiteSpace(value)) return 0.0;

            // Los montos vienen en centavos (ej: "112" = 1.12), dividir entre 100
            if (long.TryParse(value, out var centavos))
            {
                return centavos / 100.0;
            }
            return 0.0;
        }

        private static bool ExtractJsonBool(string response, string key)
        {
            var value = ExtractJsonValue(response, key);
            return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }

        private static string ExtractMediosPagoCsv(string response)
        {
            if (string.IsNullOrWhiteSpace(response)) return string.Empty;

            var mediosPagoMatch = Regex.Match(
                response,
                "\"mediosPago\"\\s*:\\s*\\{(.*?)\\}",
                RegexOptions.CultureInvariant | RegexOptions.Singleline);

            if (!mediosPagoMatch.Success) return string.Empty;

            var mediosPagoContent = mediosPagoMatch.Groups[1].Value;
            var itemMatches = Regex.Matches(
                mediosPagoContent,
                "\"[^\"]+\"\\s*:\\s*\"([^\"]*)\"",
                RegexOptions.CultureInvariant);

            if (itemMatches.Count == 0) return string.Empty;

            var values = new List<string>();
            foreach (Match item in itemMatches)
            {
                if (item.Groups.Count < 2) continue;
                var value = item.Groups[1].Value;
                if (string.IsNullOrWhiteSpace(value)) continue;
                values.Add(value);
            }

            return string.Join(",", values);
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
