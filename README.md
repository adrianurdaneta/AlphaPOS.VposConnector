Vpos Universal 3.15.9 Certificada
=================================

Resumen
-------
Repositorio que contiene la implementación del conector COM "AlphaPOS.VposConnector" (C# / .NET Framework 4.8, x86) para integrar AlphaPOS (Visual FoxPro 9) con la REST API de VPOS.

Estructura relevante
--------------------
- AlphaPOS.VposConnector/                         -> Proyecto de la DLL (código fuente)
  - AlphaPOS.VposConnector.csproj
  - VposConnector.cs
  - README.md (documentación específica del proyecto)
  - AlphaPOS.VposConnector_Manual_ES.md (manual completo en español)
  - register_dll.bat (script para registrar/desregistrar la DLL)
  - bin/Release/net48/AlphaPOS.VposConnector.dll (artefacto compilado)

- example_from_vfp.prg                             -> Ejemplos de uso desde Visual FoxPro (VFP9)

- AlphaPOS.VposConnector.Tests/                    -> Proyecto de pruebas unitarias (MSTest)

Cómo compilar
-------------
Requisitos:
- .NET Framework 4.8
- Visual Studio o MSBuild
- Compilar en Release con PlatformTarget=x86

Ejemplo MSBuild:
msbuild AlphaPOS.VposConnector\AlphaPOS.VposConnector.csproj /p:Configuration=Release

Registro COM
------------
- Usar la versión x86 de RegAsm (desde un prompt con privilegios de administrador):
  C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe AlphaPOS.VposConnector.dll /codebase /tlb

- Alternativa: ejecutar el script incluid o (necesita privilegios):
  AlphaPOS.VposConnector\register_dll.bat install
  AlphaPOS.VposConnector\register_dll.bat uninstall

Archivos clave
--------------
- example_from_vfp.prg: varios ejemplos (Init, TestConnection, StartTransaction, PollStatus, GetVoucher, CancelTransaction, SetApiKey, SetClientCertificate, SetLogPath, SetTimeout)
- AlphaPOS.VposConnector_Manual_ES.md: manual completo con configuración INI, descripción de métodos COM, autenticación y recomendaciones de despliegue.
- register_dll.bat: script para registrar/desregistrar la DLL con RegAsm.

Pruebas
-------
Proyecto de pruebas: AlphaPOS.VposConnector.Tests (MSTest). El test runner debe ejecutarse en x86 (se incluye test.runsettings con TargetPlatform=x86).

Notas importantes
-----------------
- Autenticación: ApiKey con fallback a certificado TLS (PFX) si el servidor responde 401/403.
- Diseño: Clean Architecture (Domain/Application/Infrastructure/Adapter) para facilitar pruebas y mantenimiento.
- Timeout StartTransaction: por defecto preparado para operaciones largas (p. ej. interacción con PIN-pad) — configurar en el INI o vía SetTimeout.
- No incluir secretos (API keys o contraseñas) en repositorios públicos.

Siguientes pasos sugeridos
-------------------------
- Proveer endpoints y esquema JSON reales para ajustar TransactionService a los paths exactos de VPOS.
- Preparar paquete instalable y documentación de certificación.
- En producción, usar almacenamiento seguro para ApiKeys y certificados (no PFX en disco sin protección).

Contacto
--------
Para soporte en la integración, proporcionar los endpoints sandbox/producción, credenciales de prueba y requisitos de certificación: logs, escenarios y muestras de payload.

Documentos y manuales
---------------------
- Instrucciones de ejecución de VPOS REST (resumen): [VPOS_Run_Instructions.md](./VPOS_Run_Instructions.md)
- Pasos de despliegue para producción (copy/paste): [VPOS_Production_Steps.md](./VPOS_Production_Steps.md)
- Manual técnico de la DLL (español): [AlphaPOS.VposConnector/AlphaPOS.VposConnector_Manual_Technical_ES.md](./AlphaPOS.VposConnector/AlphaPOS.VposConnector_Manual_Technical_ES.md)

