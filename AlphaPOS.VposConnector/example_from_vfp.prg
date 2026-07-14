* example_from_vfp.prg
* Fase 1 - Ejemplos simples para AlphaPOS.VposConnector desde Visual FoxPro 9
*
* Retornos de metodos especificos:
*   1  = aprobada/procesada
*   0  = rechazada
*  -1  = error tecnico
*
* Propiedades de salida:
*   LastCode, LastMessage, LastStatus, LastRawResponse, LastSequence, LastVoucher

LOCAL oConn, lOk, lnRc
oConn = CREATEOBJECT("AlphaPOS.VposConnector")

* Se usa configuracion por archivo INI (vposconf.ini)
* Ajustar la ruta a su instalacion real.
lOk = oConn.Init("C:\\vpos\\vposconf.ini")
IF NOT lOk
    MESSAGEBOX("Init fallo. Verifique Merchant_Server y parametros del INI.", 16, "VPOS")
    RETURN
ENDIF

? "TestConnection: " + oConn.TestConnection()

*---------------------------------------------------------------------
* 1) Tarjetas (Mercantil, Plaza, BDV, Banesco, BNC)
*---------------------------------------------------------------------
lnRc = oConn.PagarTarjetaDebito(10100.51, "V12345678", "REF-001", "", 0)
? "PagarTarjetaDebito rc=" + TRANSFORM(lnRc) + " / " + oConn.LastMessage

*---------------------------------------------------------------------
* 2) Pago movil (P2C) - Mercantil / Plaza
*---------------------------------------------------------------------
lnRc = oConn.VerificarP2C(0.01, "04121234567", "Mercantil", "")
? "VerificarP2C rc=" + TRANSFORM(lnRc) + " / " + oConn.LastMessage

*---------------------------------------------------------------------
* 3) C@mbio - Mercantil / Plaza
*---------------------------------------------------------------------
lnRc = oConn.PagarConCambio(10.00, "V12345678", "VES", "")
? "PagarConCambio rc=" + TRANSFORM(lnRc) + " / " + oConn.LastMessage

*---------------------------------------------------------------------
* 4) Biopago (serviciosExternos) - BDV
*---------------------------------------------------------------------
lnRc = oConn.PagarBiopago(5.00, "V12345678", "")
? "PagarBiopago rc=" + TRANSFORM(lnRc) + " / " + oConn.LastMessage

*---------------------------------------------------------------------
* 5) Consultas/operaciones de cierre
*---------------------------------------------------------------------
lnRc = oConn.ImprimirUltimoVoucherAprobado()
? "ImprimirUltimoVoucherAprobado rc=" + TRANSFORM(lnRc) + " / " + oConn.LastMessage

lnRc = oConn.EjecutarPrecierre()
? "EjecutarPrecierre rc=" + TRANSFORM(lnRc) + " / " + oConn.LastMessage

lnRc = oConn.EjecutarCierre()
? "EjecutarCierre rc=" + TRANSFORM(lnRc) + " / " + oConn.LastMessage

*---------------------------------------------------------------------
* 6) Anulacion por secuencia (si aplica)
*---------------------------------------------------------------------
lnRc = oConn.AnularTarjetaPorSecuencia("123456", "V12345678", "")
? "AnularTarjetaPorSecuencia rc=" + TRANSFORM(lnRc) + " / " + oConn.LastMessage

RETURN
