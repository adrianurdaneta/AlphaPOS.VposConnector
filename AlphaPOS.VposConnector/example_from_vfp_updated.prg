* example_from_vfp.prg
* Fase 1 - REST local directo (sin INI, sin API key)
*
* Retornos:
*   1  = aprobada/procesada/conectado
*   0  = rechazada
*  -1  = error tecnico
*
* Propiedades de salida - Resultado:
*   LastCode, LastMessage, LastStatus
*
* Propiedades de salida - Transaccion:
*   LastRawResponse, LastSequence, LastVoucher, LastVoucherPath
*   LastReference, LastOrderId
*
* Propiedades de salida - Datos Operativos (NEW):
*   LastTerminal, LastTid, LastLote, LastTipoTransaccion
*   LastFechaTransaccion, LastHoraTransaccion
*
* Propiedades de salida - Autorizacion (NEW):
*   LastNumeroAutorizacion, LastCodigoAfiliacion, LastCodigoAdquiriente, LastNroAutorizador
*
* Propiedades de salida - Tarjeta (NEW):
*   LastTipoTarjeta, LastTipoProducto, LastTipoCuenta, LastMedioPago
*
* Propiedades de salida - Montos (NEW):
*   LastMontoTransaccion, LastMontoServicios, LastMontoDonativo, LastMoneda, LastExisteCopiaVoucher

LOCAL oConn, lnRc
oConn = CREATEOBJECT("AlphaPOS.VposConnector")

* Opcional: por defecto ya usa http://localhost:8085
oConn.SetBaseUrl("http://localhost:8085")
oConn.SetTimeout(120000)

*---------------------------------------------------------------------
* 0) Prueba de conexion (ahora devuelve int)
*---------------------------------------------------------------------
lnRc = oConn.TestConnection()
IF lnRc = 1
  ? "TestConnection OK => " + oConn.LastMessage
ELSE
  ? "TestConnection FALLO => " + oConn.LastMessage
ENDIF

*---------------------------------------------------------------------
* 1) Tarjetas
*---------------------------------------------------------------------
lnRc = oConn.PagarTarjetaDebito(100.00, "V16139601")
? "PagarTarjetaDebito rc=" + TRANSFORM(lnRc) + " / " + oConn.LastMessage
IF lnRc = 1
  ? "  Terminal: " + oConn.LastTerminal
  ? "  TID: " + oConn.LastTid
  ? "  Lote: " + oConn.LastLote
  ? "  Autorización: " + oConn.LastNumeroAutorizacion
  ? "  Fecha: " + oConn.LastFechaTransaccion + " " + oConn.LastHoraTransaccion
  ? "  Tipo Tarjeta: " + oConn.LastTipoTarjeta
  ? "  Tipo Producto: " + oConn.LastTipoProducto
  ? "  Monto: " + oConn.LastMontoTransaccion + " " + oConn.LastMoneda
  ? "  Montos - Servicios: " + oConn.LastMontoServicios + " / Donativo: " + oConn.LastMontoDonativo
  ? "  Copia Voucher: " + TRANSFORM(oConn.LastExisteCopiaVoucher)
ENDIF

*---------------------------------------------------------------------
* 2) Pago movil (P2C). La API actual recibe monto y cedula.
*---------------------------------------------------------------------
lnRc = oConn.VerificarP2C("0.01", "V16139601")
? "VerificarP2C rc=" + TRANSFORM(lnRc) + " / " + oConn.LastMessage

*---------------------------------------------------------------------
* 3) Cambio
*---------------------------------------------------------------------
lnRc = oConn.PagarConCambio(10.00, "V16139601", "VES")
? "PagarConCambio rc=" + TRANSFORM(lnRc) + " / " + oConn.LastMessage

*---------------------------------------------------------------------
* 4) Biopago
*---------------------------------------------------------------------
lnRc = oConn.PagarBiopago(5.00, "V16139601")
? "PagarBiopago rc=" + TRANSFORM(lnRc) + " / " + oConn.LastMessage

*---------------------------------------------------------------------
* 5) Consultas/cierre
*---------------------------------------------------------------------
lcMedios = oConn.ObtenerMediosPago()
? "ObtenerMediosPago => " + lcMedios + " / " + oConn.LastMessage

lnRc = oConn.ImprimirUltimoVoucherAprobado()
? "ImprimirUltimoVoucherAprobado rc=" + TRANSFORM(lnRc) + " / " + oConn.LastMessage

lnRc = oConn.EjecutarPrecierre()
? "EjecutarPrecierre rc=" + TRANSFORM(lnRc) + " / " + oConn.LastMessage

lnRc = oConn.EjecutarCierre()
? "EjecutarCierre rc=" + TRANSFORM(lnRc) + " / " + oConn.LastMessage

*---------------------------------------------------------------------
* 6) Anulacion por secuencia
*---------------------------------------------------------------------
lnRc = oConn.AnularTarjetaPorSecuencia("22", "V16139601")
? "AnularTarjetaPorSecuencia rc=" + TRANSFORM(lnRc) + " / " + oConn.LastMessage

RETURN
