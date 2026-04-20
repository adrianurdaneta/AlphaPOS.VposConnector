* example_from_vfp.prg
* Ejemplos de uso de AlphaPOS.VposConnector (Visual FoxPro 9)
*
* Notas:
* - ProgId: AlphaPOS.VposConnector
* - Llamar Init(pathIni) antes de invocar otros métodos.
* - Si Init falla, los métodos devuelven la cadena "NO_INIT".
* - StartTransaction recibe un JSON como cadena y devuelve una respuesta JSON/string.
*
*---------------------------------------------------------------------
* Ejemplo 1: Inicializar y probar conexión (sencillo)
*---------------------------------------------------------------------
LOCAL oConn, lOk, lcResp
oConn = CREATEOBJECT("AlphaPOS.VposConnector")
* Ruta al INI (ajustar a su instalación)
lcIni = "C:\\vpos_config\\vpos.ini"

lOk = oConn.Init(lcIni)
IF lOk
    * TestConnection devuelve "OK" o mensaje de error
    lcResp = oConn.TestConnection()
    MESSAGEBOX("TestConnection: " + lcResp, 64, "VPOS Test")
ELSE
    MESSAGEBOX("Error: Init falló. Verificar ruta INI y parámetros.", 16, "VPOS Init")
ENDIF

*---------------------------------------------------------------------
* Ejemplo 2: StartTransaction básico (envía JSON y muestra respuesta)
*---------------------------------------------------------------------
IF lOk
    lcRequest = '{"merchantTransactionId":"T-0001","amount":125.50,"currency":"USD","cardHolderId":"V12345678"}'
    lcStartResp = oConn.StartTransaction(lcRequest)
    ? "StartTransaction response:" + CHR(13)+lcStartResp
ENDIF

*---------------------------------------------------------------------
* Ejemplo 3: Flujo con polling (Start -> PollStatus -> GetVoucher)
* Nota: StartTransaction puede devolver un JSON con transactionId.
* Se muestra una forma simple de extraer transactionId si la respuesta
* incluye la clave "transactionId" (usar un parser JSON en producción).
*---------------------------------------------------------------------
IF lOk
    lcRequest = '{"merchantTransactionId":"T-0002","amount":50.00,"currency":"USD"}'
    lcStartResp = oConn.StartTransaction(lcRequest)
    ? "StartResp:" + lcStartResp

    * Intentar extraer transactionId (búsqueda simple, reemplazar por parser robusto)
    lnPos = AT('"transactionId"', lcStartResp)
    IF lnPos > 0
        * buscar el siguiente ':' y las comillas del valor
        lnStart = AT(':', lcStartResp, lnPos) + 1
        lnQ1 = AT('"', lcStartResp, lnStart)
        lnQ2 = AT('"', lcStartResp, lnQ1+1)
        lcTranId = SUBSTR(lcStartResp, lnQ1+1, lnQ2-lnQ1-1)
    ELSE
        * Si el servicio devuelve directamente el id, adaptarlo aquí
        lcTranId = lcStartResp
    ENDIF

    IF EMPTY(lcTranId)
        ? "No se obtuvo transactionId. Revisar respuesta: " + lcStartResp
    ELSE
        * Polling (ejemplo: máximo 120 iteraciones, 1s cada una -> 120s)
        lnMax = 120
        lnI = 0
        DO WHILE lnI < lnMax
            WAIT WINDOW "Esperando estado... (" + STR(lnI) + ")" TIMEOUT 1
            lcStatusResp = oConn.PollStatus(lcTranId)
            ? "PollStatus: " + lcStatusResp
            * Comprobar estado en la respuesta (ejemplo: buscar "COMPLETED")
            IF AT('"status":"COMPLETED"', lcStatusResp) > 0
                EXIT
            ENDIF
            lnI = lnI + 1
        ENDDO

        * Obtener voucher cuando esté completado
        lcVoucher = oConn.GetVoucher(lcTranId)
        ? "Voucher: " + lcVoucher
    ENDIF
ENDIF

*---------------------------------------------------------------------
* Ejemplo 4: Cancelar transacción (ejecutarlo si es necesario)
*---------------------------------------------------------------------
IF lOk AND NOT EMPTY(lcTranId)
    lcCancelResp = oConn.CancelTransaction(lcTranId)
    ? "CancelResponse: " + lcCancelResp
ENDIF

*---------------------------------------------------------------------
* Ejemplo 5: Configuración dinámica desde VFP (ApiKey / Certificado / Log / Timeout)
*---------------------------------------------------------------------
* Establecer ApiKey (si se quiere omitir INI)
oConn.SetApiKey("MI_API_KEY_DE_PRUEBA")
* Establecer header personalizado
oConn.SetApiKeyWithHeader("MI_API_KEY_DE_PRUEBA","X-My-ApiKey")
* Establecer certificado cliente (PFX)
oConn.SetClientCertificate("C:\\certs\\cliente.pfx","miPasswordPfx")
* Establecer ruta de log
oConn.SetLogPath("C:\\AlphaPOS\\logs\\vpos.log")
* Establecer timeout en milisegundos (por ejemplo 120 segundos)
oConn.SetTimeout(120000)

*---------------------------------------------------------------------
* Ejemplo 6: Manejo de errores simple
*---------------------------------------------------------------------
IF lOk
    lcResp = oConn.StartTransaction('{"merchantTransactionId":"T-ERR-1","amount":1.00}')
    IF EMPTY(lcResp)
        MESSAGEBOX("Respuesta vacía del servicio",16)
    ELSEIF lcResp == "NO_INIT"
        MESSAGEBOX("El conector no fue inicializado",16)
    ELSEIF AT('"error"', lcResp) > 0
        MESSAGEBOX("Error devuelto por la API: " + lcResp,16)
    ELSE
        ? "Respuesta: " + lcResp
    ENDIF
ENDIF

*---------------------------------------------------------------------
* Recomendaciones:
* - No ejecutar StartTransaction en el hilo principal de la UI si espera
*   interacciones de dispositivos (PIN-pad). Mejor usar un proceso auxiliar o
*   ejecutar la llamada en segundo plano.
* - Usar un parser JSON en VFP (biblioteca externa) para respuestas complejas.
* - Revisar el log configurado con SetLogPath para diagnosticar fallos.
*---------------------------------------------------------------------
RETURN
