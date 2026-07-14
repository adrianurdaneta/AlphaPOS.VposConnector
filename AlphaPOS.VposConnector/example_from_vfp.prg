* example_from_vfp.prg
* Fase 1 - REST local directo (sin INI, sin API key)
*
* Retornos:
*   1  = aprobada/procesada
*   0  = rechazada
*  -1  = error tecnico
*
* Propiedades de salida:
*   LastCode, LastMessage, LastStatus, LastRawResponse, LastSequence, LastVoucher

LOCAL oConn, lnRc
oConn = CREATEOBJECT("AlphaPOS.VposConnector")

* Opcional: por defecto ya usa http://localhost:8085
oConn.SetBaseUrl("http://localhost:8085")
oConn.SetTimeout(120000)

? "TestConnection => " + oConn.TestConnection()

*---------------------------------------------------------------------
* 1) Tarjetas
*---------------------------------------------------------------------
lnRc = oConn.PagarTarjetaDebito(100.00, "V16139601")
? "PagarTarjetaDebito rc=" + TRANSFORM(lnRc) + " / " + oConn.LastMessage

*---------------------------------------------------------------------
* 2) Pago movil (P2C)
*---------------------------------------------------------------------
lnRc = oConn.VerificarP2C(0.01, "04121234567", "Mercantil")
? "VerificarP2C rc=" + TRANSFORM(lnRc) + " / " + oConn.LastMessage

*---------------------------------------------------------------------
* 3) C@mbio
*---------------------------------------------------------------------
lnRc = oConn.PagarConCambio(10.00, "V16139601", "VES")
? "PagarConCambio rc=" + TRANSFORM(lnRc) + " / " + oConn.LastMessage

*---------------------------------------------------------------------
* 4) Biopago (serviciosExternos)
*---------------------------------------------------------------------
lnRc = oConn.PagarBiopago(5.00, "V16139601")
? "PagarBiopago rc=" + TRANSFORM(lnRc) + " / " + oConn.LastMessage

*---------------------------------------------------------------------
* 5) Consultas/cierre
*---------------------------------------------------------------------
lnRc = oConn.ObtenerMediosPago()
? "ObtenerMediosPago rc=" + TRANSFORM(lnRc) + " / " + oConn.LastMessage

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
