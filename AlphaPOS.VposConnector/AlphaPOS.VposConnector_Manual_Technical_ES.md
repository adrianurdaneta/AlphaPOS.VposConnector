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

Firmas y comportamiento:

- bool Init(string iniPath)
  - Descripción: Inicializa el conector leyendo iniPath, carga configuración, certificado (si aplica) y compone servicios.
  - Retorno: true = init correcta; false = error (suponer que se ha escrito en log).
  - Notas: Debe llamarse antes de otros métodos.

- void SetApiKey(string key)
  - Establece ApiKey en memoria (sobrescribe el valor del INI).

- void SetApiKeyWithHeader(string key, string header)
  - Establece ApiKey y el nombre de header.

- void SetClientCertificate(string path, string password)
  - Carga certificado PFX en memoria para uso en fallback TLS.

- void SetLogPath(string path)
  - Cambia la ruta del archivo de log (si no existe intenta crearla).

- void SetTimeout(int ms)
  - Ajusta timeout global en milisegundos para operaciones que lo respetan.

- string TestConnection()
  - Ejecuta request simple al endpoint para comprobar conectividad.
  - Retorna "OK" o mensaje/JSON de error.

- string StartTransaction(string jsonRequest)
  - Envía petición de inicio de transacción. jsonRequest es cadena JSON con payload requerido por VPOS.
  - Retorna respuesta del servidor como string JSON o mensaje de error.
  - Nota: Operación potencialmente larga (PIN-pad). Ajustar timeout y ejecutar en background si la UI no debe bloquearse.

- string PollStatus(string transactionId)
  - Consulta estado de una transacción.

- string GetVoucher(string transactionId)
  - Obtiene voucher asociado a la transacción (texto o JSON según implementación del servidor).

- string CancelTransaction(string transactionId)
  - Solicita cancelación y devuelve respuesta.

Comportamiento especial:
- Si Init no fue invocado correctamente, los métodos que devuelven string retornan la cadena literal "NO_INIT".
- Los métodos atrapan excepciones internas, registran el error y devuelven mensajes descriptivos en lugar de lanzar excepciones a VFP.

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
- "NO_INIT": devuelto por métodos string si Init no fue ejecutado con éxito.
- Respuesta JSON de error: el servidor puede retornar estructuras JSON con claves "error", "message" o códigos — parsear y actuar en cliente.
- Errores locales: timeout, problemas de red, fallo de carga del certificado — se registran y se devuelven como texto descriptivo.
- Si la respuesta es vacía, trate como error y consulte logs.

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

Ejemplo JSON (StartTransaction):
{
  "merchantTransactionId": "T-0001",
  "amount": 125.50,
  "currency": "USD",
  "cardHolderId": "V12345678"
}

Ejemplo VFP (sincrónico, simple):
LOCAL oConn, lOk, lcResp
oConn = CREATEOBJECT("AlphaPOS.VposConnector")
IF oConn.Init("C:\\vpos_config\\vpos.ini")
  lcResp = oConn.StartTransaction('{"merchantTransactionId":"T-0001","amount":125.50}')
  ? lcResp
ELSE
  MESSAGEBOX("Init falló",16)
ENDIF

Ejemplo VFP (recomendado — asincrónico simple - esquema):
* 1) Llamar a un proceso externo o crear tarea para invocar StartTransaction y almacenar resultado en fichero/BD
* 2) PollStatus desde UI para no bloquear

17) Solución de problemas (FAQ)
-------------------------------
Q: StartTransaction bloquea la UI. ¿Qué hacer?
A: Ejecutar la llamada en segundo plano (otro proceso o tarea), aumentar timeout si el PIN-pad requiere interacción.

Q: Recibo "NO_INIT" al llamar StartTransaction.
A: Asegurarse de invocar Init(iniPath) con la ruta correcta, permisos de lectura y que Merchant_Server esté configurado.

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
