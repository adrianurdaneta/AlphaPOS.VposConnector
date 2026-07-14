AlphaPOS.VposConnector — Manual técnico y de uso
================================================

Versión: 1.0
Fecha: 2026-04-20
Autor: Equipo de Integración

Índice
------
1. Resumen y propósito
2. Arquitectura y componentes
3. Requisitos y plataforma soportada
4. Compilación y artefactos
5. Registro COM y despliegue
6. Fichero de configuración (INI)
7. API pública (métodos COM) — referencia detallada
8. Flujo de autenticación (ApiKey + fallback Certificado TLS)
9. Logging, diagnósticos y trazas
10. Manejo de errores y valores de retorno
11. Consideraciones de hilos y rendimiento
12. Pruebas unitarias e integración
13. Despliegue en producción y recomendaciones operativas
14. Seguridad y manejo de secretos
15. Checklist para certificación con VPOS
16. Ejemplos (VFP & JSON)
17. Solución de problemas (FAQ)
18. Anexos: INI y payloads de ejemplo

1) Resumen y propósito
----------------------
AlphaPOS.VposConnector es una librería .NET (DLL) compilada para x86 y expuesta como COM-visible (ProgId: AlphaPOS.VposConnector). Su objetivo es proporcionar una interfaz simple y estable que AlphaPOS (Visual FoxPro 9 u otra app 32-bit) pueda usar para invocar la REST API de VPOS sin tener que implementar HTTP/SSL/PKI en VFP.

2) Arquitectura y componentes
-----------------------------
Diseño: Clean Architecture (Domain / Application / Infrastructure / Adapter)

Principales componentes:
- Adapter (COM): VposConnector — fachada COM, expone métodos que VFP consume.
- Application: TransactionService — orquesta escenarios de negocio (StartTransaction, PollStatus, etc.).
- Infrastructure:
  - HttpClientWrapper — manejo HTTP, inyección de headers, retry/fallback (testable).
  - ClientCertificateProvider — carga inyectable de certificados (PFX) para fallback TLS.
  - IniConfiguration — lector de INI.
  - FileLogger — logger simple a fichero (UTC timestamps).
- Domain: ITransactionService y modelos (contratos simples para la integración).

Flujo: VFP → VposConnector → TransactionService → HttpClientWrapper (+ ClientCertificateProvider) → VPOS REST

3) Requisitos y plataforma soportada
----------------------------------
- Windows (producción y pruebas).
- .NET Framework 4.8 instalado (la DLL está compilada para net48).
- Plataforma: x86 (32-bit) — requisito para compatibilidad con Visual FoxPro 9.
- Para registro COM: RegAsm.exe (v4 x86) y privilegios de administrador.
- Certificado cliente (PFX) opcional para fallback TLS.

4) Compilación y artefactos
---------------------------
- Usar Visual Studio o MSBuild.
- Compilar en Release, PlatformTarget: x86.
- Comando ejemplo:
  msbuild AlphaPOS.VposConnector\AlphaPOS.VposConnector.csproj /p:Configuration=Release
- Artefacto principal: bin\Release\net48\AlphaPOS.VposConnector.dll
- Archivo de registro y tests:
  - register_dll.bat (registrar/desregistrar)
  - AlphaPOS.VposConnector.Tests (MSTest) — usar runsettings para forzar x86

5) Registro COM y despliegue
----------------------------
Registro (administrador):
- Usar RegAsm x86:
  C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe AlphaPOS.VposConnector.dll /codebase /tlb
- Alternativa: ejecutar AlphaPOS.VposConnector\register_dll.bat install

Desregistro:
- RegAsm.exe /unregister AlphaPOS.VposConnector.dll
- o register_dll.bat uninstall

Notas:
- /codebase registra ruta del ensamblado. En producción considere firmar (strong-name) y/o instalar en GAC o usar instalador MSI/NSIS que ejecute RegAsm.
- Registrar con la misma arquitectura (x86) que la DLL.

6) Fichero de configuración (INI)
--------------------------------
La DLL lee un INI en Init(iniPath) — sección [General].
Ejemplo mínimo:

[General]
Merchant_Server=https://sandbox.vpos.example/api
ApiKey=REEMPLAZAR_CON_TU_API_KEY
ApiKeyHeader=X-Api-Key
ClientCertPath=C:\certs\cliente.pfx
ClientCertPassword=miPasswordPfx
TimeoutMs=120000

Descripción de claves:
- Merchant_Server: URL base del servicio VPOS (p. ej. https://vpos.merchantsrv/api). Obligatorio.
- ApiKey: clave para autenticación en header (opcional si se usará certificado).
- ApiKeyHeader: nombre del header HTTP para ApiKey (por defecto X-Api-Key si no se especifica).
- ClientCertPath: ruta local a PFX (opcional). Recomendado usar almacén de certificados en producción.
- ClientCertPassword: contraseña del PFX (si aplica).
- TimeoutMs: timeout global (ms). StartTransaction puede requerir un valor alto (p. ej. 120000 ms) por interacción con PIN-pad.

7) API pública (métodos COM) — referencia detallada
--------------------------------------------------
ProgId: AlphaPOS.VposConnector
Clase COM: VposConnector

### 7.1 Enfoque recomendado

La API conserva métodos legacy para compatibilidad, pero para nuevas integraciones en VFP se recomienda usar los métodos específicos por caso de negocio (sin JSON de entrada).

### 7.2 Métodos base

- bool Init(string iniPath)
  - Inicializa configuración, HTTP y certificado (si aplica).

- void SetApiKey(string key)
- void SetApiKeyWithHeader(string key, string header)
- void SetClientCertificate(string path, string password)
- void SetLogPath(string path)
- void SetTimeout(int ms)
- void ClearLastResult()

- int Ping()
  - Verifica conectividad del servicio y devuelve estado simplificado.

- int TerminarServicio()
  - Intenta finalizar el servicio local VPOS REST.

### 7.3 Métodos específicos por negocio (sin JSON desde VFP)

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

Método de escape:

- int ExecuteRaw(string endpoint, string jsonRequest)
  - Permite invocar endpoints `metodo`, `cards`, `lysto` o `terminate` en escenarios no cubiertos por la API específica.

### 7.4 Propiedades de último resultado

- string LastCode
- string LastMessage
- string LastStatus
- string LastSequence
- string LastVoucher
- string LastOrderId
- string LastReference
- string LastRawResponse

Convención de retorno para métodos `int`:

- `1`: operación aprobada/procesada.
- `0`: operación rechazada/no aprobada.
- `-1`: error técnico.

### 7.5 Métodos legacy (compatibilidad)

- string TestConnection()
- string StartTransaction(string jsonRequest)
- string PollStatus(string transactionId)
- string GetVoucher(string transactionId)
- string CancelTransaction(string transactionId)

Comportamiento especial legacy:

- Si Init no fue invocado correctamente, los métodos legacy string retornan `"NO_INIT"`.

8) Flujo de autenticación (ApiKey + fallback Certificado TLS)
-------------------------------------------------------------
Objetivo: Soportar dos mecanismos de autenticación según configuración y respuesta del servidor.

1. Preferencia: ApiKey
   - Si ApiKey está configurada (INI o SetApiKey), HttpClientWrapper incluye el header especificado (por defecto X-Api-Key) en las peticiones.
   - Si la respuesta del servidor es 401/403 (o se produce UnauthorizedException), y si existe certificado cliente cargado, se realiza un nuevo intento usando el certificado TLS.

2. Fallback: Certificado TLS (PFX)
   - ClientCertificateProvider carga PFX desde disco (o se inyecta en tests).
   - Si el primer intento con ApiKey falla por autorización y la carga del certificado fue correcta, se realizará la petición con el certificado cliente agregado al handler SSL/TLS.

3. Último recurso: intento sin autenticación
   - Si ambos modos anteriores fallan y se desea aún intentar, el wrapper podría ejecutar un intento sin autenticación; esto es configurable por la lógica de TransactionService.

Recomendaciones de seguridad:
- No escribir ApiKey ni contraseña del PFX en logs.
- En producción preferir el uso del almacén de certificados de Windows (cert store) en lugar de guardar PFX con contraseña en disco.

9) Logging, diagnósticos y trazas
--------------------------------
- Logger: FileLogger escribe líneas con timestamp UTC.
- Ruta por defecto: AppDomain.CurrentDomain.BaseDirectory\AlphaPOS.VposConnector.log
- Cambiar ruta: SetLogPath(string path).
- Mensajes importantes: Init, TestConnection, StartTransaction requests/responses (respuestas completas), fallback auth attempts, errores y excepciones.
- Recomendación: habilitar logs durante la fase de integración y rotarlos en producción.

10) Manejo de errores y valores de retorno
-----------------------------------------
API específica (`int`):

- `1`: operación aprobada/procesada.
- `0`: operación rechazada/no aprobada.
- `-1`: error técnico.

El detalle de cada resultado se expone en propiedades `Last*`:

- `LastCode`, `LastMessage`, `LastStatus`, `LastRawResponse` y, según el caso, `LastSequence`, `LastVoucher`, `LastOrderId`.

API legacy (`string`):

- `"NO_INIT"`: devuelto por métodos string si Init no fue ejecutado con éxito.
- Respuesta JSON de error: el servidor puede retornar estructuras con claves "error", "message" o códigos.
- Errores locales: timeout, red, certificado.
- Si la respuesta es vacía, tratarla como error y revisar log.

11) Consideraciones de hilos y rendimiento
-----------------------------------------
- El adaptador COM expone ClassInterfaceType.AutoDual; el uso con VFP (single-threaded) es habitual.
- StartTransaction puede bloquear por tiempos largos (PIN-pad). Ejecutar en hilo/Proceso separado para no congelar UI de VFP.
- Para llamadas concurrentes, instancie un objeto VposConnector por hilo/proceso; evitar compartir una instancia mutable entre hilos.
- Para producción, considerar un servicio/exe intermediario que reciba peticiones de VFP y gestione la asincronía/cola.

12) Pruebas unitarias e integración
----------------------------------
- Test project: AlphaPOS.VposConnector.Tests (MSTest).
- Nota sobre Bitness: ejecutar test runner en x86 (test.runsettings incluido con TargetPlatform=x86).
- Tests incluidos: TransactionService y HttpClientWrapper con mocks/secuencias para simular 401 + certificado éxito.
- Ejecutar tests desde Visual Studio Test Explorer o con vstest.console.exe apuntando al .dll de tests y usando el runsettings.

13) Despliegue en producción y recomendaciones operativas
---------------------------------------------------------
- Empaquetar en instalador (MSI/NSIS) que copie archivos, registre la DLL con RegAsm, coloque el INI y fije permisos sobre carpetas y certificados.
- No usar Startup VBS para procesos críticos: preferir servicio de Windows (NSSM o wrapper nativo) para mayor robustez.
- Asegurar backups y rotación de logs; definir ACLs sobre ficheros sensibles (PFX, INI con claves).

Comandos útiles (ejemplos):
- Registrar DLL: RegAsm.exe AlphaPOS.VposConnector.dll /codebase /tlb
- Unregister: RegAsm.exe /unregister AlphaPOS.VposConnector.dll

14) Seguridad y manejo de secretos
---------------------------------
- No almacenar ApiKey en texto plano en repositorios.
- Preferir el uso de:
  - Windows Certificate Store para certificados.
  - DPAPI/ProtectedData para cifrar valores sensibles.
- Si usa PFX en disco, asegurar que carpeta sea accesible solo a la cuenta del servicio (ACL restrictiva) y que la contraseña no permanezca en logs.

15) Checklist para certificación con VPOS
-----------------------------------------
- Provisión de sandbox / endpoints y credenciales de prueba.
- Ejecutar escenarios obligatorios: aprovisionamiento, autorización, rechazo, anulación, reimpresión voucher, timeout y reconexión.
- Recolectar logs y traza HTTP (timestamps, request/response completos) para cada caso.
- Entregar pasos reproducibles y muestras de payload para auditoría.

16) Ejemplos (VFP y JSON)
-------------------------
Ejemplo INI mínimo:
[General]
Merchant_Server=https://sandbox.vpos.example/api
ApiKey=MI_API_KEY
ApiKeyHeader=X-Api-Key
ClientCertPath=C:\certs\cliente.pfx
ClientCertPassword=miPasswordPfx
TimeoutMs=120000

Ejemplo VFP (API específica recomendada):
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

Ejemplo JSON (legacy StartTransaction):
{
  "merchantTransactionId": "T-0001",
  "amount": 125.50,
  "currency": "USD",
  "cardHolderId": "V12345678"
}

Ejemplo VFP (legacy sincrónico, simple):
LOCAL oConn, lOk, lcResp
oConn = CREATEOBJECT("AlphaPOS.VposConnector")
IF oConn.Init("C:\\vpos_config\\vpos.ini")
  lcResp = oConn.StartTransaction('{"merchantTransactionId":"T-0001","amount":125.50}')
  ? lcResp
ELSE
  MESSAGEBOX("Init falló",16)
ENDIF

Ejemplo VFP (legacy asincrónico simple - esquema):
* 1) Llamar a proceso/tarea para StartTransaction y guardar resultado
* 2) PollStatus desde UI para no bloquear

17) Solución de problemas (FAQ)
-------------------------------
Q: PagarTarjetaDebito / PagarTarjetaCredito devuelve -1. ¿Qué revisar?
A: Revisar `LastMessage`, `LastRawResponse`, conectividad local VPOS (`Ping`) y log de la DLL.

Q: StartTransaction bloquea la UI. ¿Qué hacer?
A: Es método legacy. Preferir API específica; si se usa legacy, ejecutar en segundo plano y ajustar timeout.

Q: Recibo "NO_INIT" al llamar métodos legacy.
A: Invocar Init(iniPath) y validar ruta/permisos del INI, además de Merchant_Server.

Q: 401/403 persistent e intento con certificado falla.
A: Validar ApiKey (si aplica), comprobar si el servidor exige solo certificado cliente, revisar que PFX y contraseña son correctos y que CN/SAN del certificado es aceptado por el servidor.

Q: Problemas registrando la DLL (RegAsm)
A: Ejecutar RegAsm desde la carpeta Framework x86 y con privilegios de administrador. Asegurarse de la bitness (x86).

Q: Tests fallan con BadImageFormatException
A: Ejecutar runner en x86 (usar test.runsettings o Visual Studio con Test settings targeting x86).

18) Anexos
---------
- Rutas de interés en el proyecto:
  - example_from_vfp.prg (ejemplos de VFP)
  - AlphaPOS.VposConnector_Manual_ES.md (manual de uso general ya incluido)
  - AlphaPOS.VposConnector_Manual_Technical_ES.md (este fichero técnico)
- Contacto: proveer endpoints y credenciales sandbox para soporte en integración.

Notas finales
-------------
- Este manual recoge el comportamiento actual del conector implementado. Ajustes pueden ser necesarios cuando el proveedor VPOS publique paths/JSON exactos o cambios de política de seguridad.
- Para cambios significativos (token-based auth, OAuth, HSM para certificados), revisar TransactionService y ClientCertificateProvider para extender la integración.

Fin del manual técnico.
