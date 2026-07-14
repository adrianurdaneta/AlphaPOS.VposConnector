AlphaPOS.VposConnector - Manual de la DLL (ES)
=================================================

Versión: 1.0
Fecha: 2026-04-20
Autor: Equipo de Integración

Resumen
-------
Este documento describe en detalle la DLL COM "AlphaPOS.VposConnector" diseñada para conectar AlphaPOS (Visual FoxPro 9) con la API REST de VPOS. La DLL está implementada en C# sobre .NET Framework 4.8 y está compilada para plataforma x86 (requisito de VFP9).

Contenido
--------
- Visión general y arquitectura
- Requisitos previos
- Compilación y registro (RegAsm / register_dll.bat)
- Fichero de configuración (INI) – formato y claves
- API COM (referencia de métodos)
- Ejemplos en Visual FoxPro (varios)
- Comportamiento de autenticación (ApiKey + fallback a certificado TLS)
- Logging y localización de logs
- Manejo de errores y consejos de despliegue
- Anexos: INI de ejemplo, JSON de ejemplo

1) Visión general y arquitectura
--------------------------------
La DLL expone una fachada COM (ProgId: AlphaPOS.VposConnector) que actúa como adaptador entre aplicaciones 32-bit (ej. AlphaPOS en VFP 9) y la REST API de VPOS. Internamente aplica principios de Clean Architecture (separación entre Domain/Application/Infrastructure/Adapter) para facilitar pruebas y mantenimiento.

Capacidades principales:
- Inicializar con un fichero INI que contiene la URL base y parámetros
- Autenticación por ApiKey (encabezado configurable) y fallback a certificado cliente (PFX)
- StartTransaction / PollStatus / GetVoucher / CancelTransaction
- Configuración en tiempo de ejecución (SetApiKey, SetClientCertificate, SetLogPath, SetTimeout)
- Archivo de log (FileLogger) con ubicación configurable

2) Requisitos previos
---------------------
- .NET Framework 4.8 instalado en la máquina (x86)
- Visual FoxPro 9 u otra aplicación 32-bit que consuma COM
- Permisos de administrador para registrar la DLL en COM (RegAsm)
- Opcional: certificado cliente formato PFX si se usa autenticación por certificado

3) Compilación y registro
-------------------------
Compilar:
- Abrir la solución/proyecto en Visual Studio o usar MSBuild.
- Asegurar Configuration=Release y PlatformTarget=x86.
- msbuild AlphaPOS.VposConnector.csproj /p:Configuration=Release

Registrar COM (opciones):
- Usar el script register_dll.bat incluido:
  - Para registrar: register_dll.bat install
  - Para desregistrar: register_dll.bat uninstall
- Alternativa (línea de comandos):
  C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe AlphaPOS.VposConnector.dll /codebase /tlb

Notas: Registrar en el equipo de destino requiere privilegios de administrador.

4) Fichero de configuración (INI)
---------------------------------
La DLL usa un INI con una sección [General]. Ejemplo:

[General]
Merchant_Server=https://sandbox.vpos.example/api
ApiKey=MI_API_KEY_DE_PRUEBA
ApiKeyHeader=X-Api-Key
ClientCertPath=C:\certs\cliente.pfx
ClientCertPassword=miPasswordPfx
TimeoutMs=120000

Descripción de claves:
- Merchant_Server: URL base del REST (sin barra final preferible). Obligatorio para Init.
- ApiKey: (Opcional) clave para usar en header con nombre ApiKeyHeader.
- ApiKeyHeader: Nombre del header para la ApiKey. Por defecto "X-Api-Key" si no se provee.
- ClientCertPath: Ruta al archivo PFX (opcional). Si se configura, se carga certificado para fallback.
- ClientCertPassword: Contraseña del PFX si aplica.
- TimeoutMs: Tiempo por defecto en milisegundos para operaciones que lo usan (StartTransaction puede anular internamente). Ej.: 120000 (120s).

5) API COM - Referencia de métodos
----------------------------------
ProgId: AlphaPOS.VposConnector
Clase: VposConnector

La DLL mantiene métodos legacy para compatibilidad, pero la integración recomendada en VFP es usar la API específica por caso de negocio (sin JSON de entrada).

### 5.1 Métodos base

- bool Init(string iniPath)
  - Inicializa el conector leyendo la configuración desde iniPath.

- void SetApiKey(string key)
- void SetApiKeyWithHeader(string key, string header)
- void SetClientCertificate(string path, string password)
- void SetLogPath(string path)
- void SetTimeout(int ms)
- void ClearLastResult()

- int Ping()
  - Prueba conectividad de forma simplificada.

- int TerminarServicio()
  - Invoca finalización del servicio local VPOS.

### 5.2 Métodos específicos por negocio (recomendados para VFP)

Tarjetas:

- int PagarTarjetaCredito(double monto, string cedula, string referencia, string terminalVirtual, double montoDonativo)
- int PagarTarjetaDebito(double monto, string cedula, string referencia, string terminalVirtual, double montoDonativo)
- int PagarTarjetaOtras(double monto, string cedula, string medioPago, string referencia, string terminalVirtual, double montoDonativo)
- int AnularTarjetaPorSecuencia(string secuencia, string cedula, string terminalVirtual)

Verificaciones:

- int VerificarP2C(double monto, string telefono, string banco, string terminalVirtual)
- int VerificarTransferencia(double monto, string cuenta, string banco, string terminalVirtual)
- int VerificarDeposito(double monto, string cuenta, string banco, string terminalVirtual)

Cards:

- int CardsConsultarSaldo(string cedula)
- int CardsPagarConOtp(string cedula, double monto)

Lysto:

- int LystoSolicitarOrden(double monto, string terminalVirtual)
- int LystoCrearOrden(double monto, string cedula, string tipoFinanciamiento, string terminalVirtual)
- int LystoPagarCuotaInicialC2P(double monto, string idOrden, string terminalVirtual)
- int LystoConfirmarOrden(double monto, string idOrden, string medioPago, string terminalVirtual)
- int LystoCancelarOrden(string numSeqOrden, string terminalVirtual)

Cashea:

- int CasheaCrearOrden(double monto, string cedula, string otp, string terminalVirtual)
- int CasheaConfirmarOrden(string idOrden, double monto, string terminalVirtual)
- int CasheaCancelarOrden(string idOrden, string terminalVirtual)

Otros medios:

- int PagarCrixto(double monto, string terminalVirtual)
- int PagarXcapit(double monto, string telefono, string otp, string terminalVirtual)
- int PagarAccessPay(double monto, string terminalVirtual)

Voucher:

- int ImprimirUltimoVoucherAprobado()
- int ImprimirUltimoVoucherProcesado()

Escape avanzado:

- int ExecuteRaw(string endpoint, string jsonRequest)
  - Útil solo para casos no cubiertos por métodos específicos.

### 5.3 Propiedades de último resultado (sin parsear JSON)

- string LastCode
- string LastMessage
- string LastStatus
- string LastSequence
- string LastVoucher
- string LastOrderId
- string LastReference
- string LastRawResponse

Convención de retorno de métodos `int`:

- `1`: operación procesada/aprobada.
- `0`: operación rechazada/no aprobada.
- `-1`: error técnico.

Métodos legacy (compatibilidad):

- string TestConnection()
- string StartTransaction(string jsonRequest)
- string PollStatus(string transactionId)
- string GetVoucher(string transactionId)
- string CancelTransaction(string transactionId)

Valor especial legacy:

- Si Init no fue llamado, los métodos legacy que devuelven string retornan `"NO_INIT"`.

6) Comportamiento de autenticación
----------------------------------
- Flujo preferido: enviar ApiKey en encabezado (header). Si el servidor devuelve 401/403 y existe certificado cliente, se intentará la conexión usando el certificado (método de fallback).
- Si no hay ApiKey y no hay certificado, las llamadas estarán sin autenticación.
- No se registra la ApiKey en logs (evitar exponer secretos) — aun así, revise el código si requiere mayor protección.

7) Logging
----------
- Por defecto el log se escribe en el directorio base de la aplicación (donde está la DLL) con el nombre AlphaPOS.VposConnector.log.
- Cambiar ruta con SetLogPath(path).
- El logger escribe líneas con timestamps UTC. El formato es simple texto y está pensado para diagnóstico.

8) Ejemplos (resumen)
---------------------
- Ver el archivo example_from_vfp.prg en la raíz del repo para ejemplos legacy.
- Para nuevas integraciones en VFP, usar preferiblemente métodos específicos por negocio.

Ejemplo rápido (VFP) con API específica:

LOCAL oConn, lnRc
oConn = CREATEOBJECT("AlphaPOS.VposConnector")

IF oConn.Init("C:\\vpos_config\\vpos.ini")
    lnRc = oConn.PagarTarjetaDebito(10100.51, "V12345678", "REF-001", "", 0)
    IF lnRc = 1
        ? "Aprobada"
        ? "Secuencia: " + oConn.LastSequence
    ELSEIF lnRc = 0
        ? "Rechazada: " + oConn.LastMessage
    ELSE
        ? "Error técnico: " + oConn.LastMessage
        ? "Raw: " + oConn.LastRawResponse
    ENDIF
ENDIF

9) Recomendaciones de implementación en VFP
-------------------------------------------
- No bloquear la UI: StartTransaction puede bloquear; ejecutar en proceso/servicio externo o tarea en background.
- Usar un parser JSON en VFP para extraer campos (transactionId, status). Evitar parseos frágiles con SUBSTR/AT en producción.
- Manejar timeouts y retries con lógica de backoff para errores transitorios.
- No incluir secretos en repositorios; usar almacenamiento seguro para ApiKeys y certificados.

10) Certificación y pruebas
---------------------------
Para certificación con VPOS siga las siguientes sugerencias:
- Preparar un entorno de pruebas (sandbox) con Merchant_Server apuntando al endpoint de pruebas.
- Capturar logs (archivo) y traza HTTP para cada caso de prueba.
- Ejecutar escenarios: autorización, rechazo, cancelación, timeouts, fallos de red, reintentos.

11) Solución de problemas comunes
---------------------------------
- "NO_INIT": Llamar Init(iniPath) con ruta correcta y permisos de lectura.
- Errores 401/403: configurar ApiKey correcto o certificado cliente. Revisar ApiKeyHeader si backend usa nombre distinto.
- Problemas de registro COM: usar la versión x86 de RegAsm y ejecutar con privilegios de administrador.
- BadImageFormatException en tests: asegurarse que test runner y DLL sean x86.

12) Anexos
---------
INI de ejemplo (copiar y adaptar):

[General]
Merchant_Server=https://sandbox.vpos.example/api
ApiKey=REEMPLAZAR_CON_TU_API_KEY
ApiKeyHeader=X-Api-Key
ClientCertPath=C:\certs\cliente.pfx
ClientCertPassword=miPasswordPfx
TimeoutMs=120000

JSON de ejemplo (StartTransaction):
{
  "merchantTransactionId": "T-0001",
  "amount": 125.50,
  "currency": "USD",
  "cardHolderId": "V12345678"
}

Contacto / Soporte
------------------
Si necesita asistencia para integrar o personalizar la DLL, proporcionar los endpoints exactos (paths) y el esquema de JSON requerido por VPOS para adaptar TransactionService.

Fin del manual.
