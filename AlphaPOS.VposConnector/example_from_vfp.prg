* Ejemplo mínimo en Visual FoxPro para consumir la DLL COM-visible
LOCAL oConn, lcResp
oConn = CREATEOBJECT("AlphaPOS.VposConnector")
IF oConn.Init("C:\\ruta\\a\\config.ini")
  * Test de comunicación (devuelve HTTP status o mensaje de error)
  lcResp = oConn.TestConnection()
  ? "TestConnection => ", lcResp
  * Ejemplo de inicio de transacción (json simple)
  lcResp = oConn.StartTransaction('{"monto":100.00,"cedula":"V12345678"}')
  ? "StartTransaction => ", lcResp
ELSE
  ? "No se pudo leer config.ini o Merchant_Server no está configurado"
ENDIF