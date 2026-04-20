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

Métodos públicos (firmas y comportamiento):

- bool Init(string iniPath)
  - Inicializa el conector leyendo la configuración desde iniPath.
  - Devuelve true si la inicialización fue correcta.
  - Si falla, la DLL registra el error y devuelve false.

- void SetApiKey(string key)
  - Establece (en tiempo de ejecución) la ApiKey que se enviará en el header.

- void SetApiKeyWithHeader(string key, string header)
  - Establece ApiKey y el nombre del header a usar (p.ej. "X-Api-Key").

- void SetClientCertificate(string path, string password)
  - Carga el certificado cliente desde un PFX (ruta local). No verifica permisos especiales.

- void SetLogPath(string path)
  - Cambia la ruta del archivo de log. Si no se establece, usa: <BaseDir>\AlphaPOS.VposConnector.log

- void SetTimeout(int ms)
  - Cambia el timeout global (ms) utilizado por algunas operaciones.

- string TestConnection()
  - Realiza un request simple al endpoint para verificar conectividad. Devuelve "OK" o mensaje de error.

- string StartTransaction(string jsonRequest)
  - Envía la petición de inicio de transacción. jsonRequest es una cadena JSON con los campos que su backend espera.
  - Devuelve la respuesta del servidor como cadena (normalmente JSON). Puede incluir transactionId.
  - Nota: Esta operación puede demorar (PIN-pad, interacción) — ajustar timeout.

- string PollStatus(string transactionId)
  - Consulta el estado de la transacción identificada por transactionId.
  - Devuelve respuesta JSON/string con campos de estado.

- string GetVoucher(string transactionId)
  - Descarga o devuelve la información del voucher asociado a la transacción.

- string CancelTransaction(string transactionId)
  - Solicita la cancelación de la transacción. Devuelve respuesta del servidor.

Valores especiales:
- Si Init no fue llamado, los métodos que devuelven string retornan la cadena literal: "NO_INIT".

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
- Ver el archivo example_from_vfp.prg en la raíz del repo para ejemplos completos:
  - Inicialización y TestConnection
  - StartTransaction básico
  - Flujo con Polling y GetVoucher
  - CancelTransaction
  - Configuración dinámica desde VFP (ApiKey y Cert)

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
