AlphaPOS.VposConnector (COM-visible .NET Framework 4.8)
=====================================================

Resumen
-------
Proyecto COM-visible que expone métodos para que AlphaPOS (VFP9) invoque la REST API de VPOS.

Diseño recomendado
------------------
- Framework: .NET Framework 4.8 (Compatibilidad COM con VFP9)
- Plataforma: x86 (Visual FoxPro 9 es 32-bit)
- Registro COM: RegAsm.exe (/codebase)

Archivos creados
----------------
- AlphaPOS.VposConnector.csproj  (proyecto)
- AssemblyInfo.cs                  (atributos COM)
- VposConnector.cs                 (fachada COM con API simple para VFP)
- README.md                        (esta documentación)
- example_from_vfp.prg             (ejemplo de uso desde VFP)

Construcción y registro
-----------------------
1. Abrir Visual Studio (Developer Command Prompt) o MSBuild.
2. Compilar en Release (PlatformTarget: x86).
   - Con MSBuild: msbuild AlphaPOS.VposConnector.csproj /p:Configuration=Release
3. Registrar la DLL para COM (usar la versión de RegAsm de .NET Framework 4.x x86):
   C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe AlphaPOS.VposConnector.dll /codebase /tlb

Notas sobre configuracion
-------------------------
- El metodo Init(iniPath) lee la seccion [General] del INI (ejemplo: `vposconf.ini`).
- Clave obligatoria: `Merchant_Server` (base URL del servicio local VPOS).
- Claves opcionales: `ApiKey`, `ApiKeyHeader`, `ClientCertPath`, `ClientCertPassword`, `TimeoutMs`.
- Endpoints usados por la DLL:
  - `POST /vpos/metodo`
  - `POST /vpos/metodo_cards`
  - `POST /vpos/metodo_lysto`
  - `GET  /vpos/metodo_terminate`

Metodos simples (Fase 1)
------------------------
- Tarjetas: `PagarTarjetaDebito`, `PagarTarjetaCredito`, `AnularTarjetaPorSecuencia`
- Pago movil: `VerificarP2C`
- C@mbio: `PagarConCambio`
- Biopago: `PagarBiopago`
- Consultas/cierre: `ImprimirUltimoVoucherAprobado`, `EjecutarPrecierre`, `EjecutarCierre`
- Estado ultimo resultado: `LastCode`, `LastMessage`, `LastStatus`, `LastRawResponse`

Ejemplo en VFP
--------------
LOCAL oConn, lnRc
oConn = CREATEOBJECT("AlphaPOS.VposConnector")
IF oConn.Init("C:\\vpos\\vposconf.ini")
  lnRc = oConn.PagarTarjetaDebito(10100.51, "V12345678", "REF-001", "", 0)
  ? "rc=", lnRc, " msg=", oConn.LastMessage
ENDIF

Recursos del proyecto
---------------------
- Ejemplo de uso en Visual FoxPro: [example_from_vfp.prg](../example_from_vfp.prg)
  Ruta absoluta: C:\dev\Vpos Universal 3.15.9 Certificada\example_from_vfp.prg
- Manual de uso (ES): [AlphaPOS.VposConnector_Manual_ES.md](./AlphaPOS.VposConnector_Manual_ES.md)
- Manual técnico (ES): [AlphaPOS.VposConnector_Manual_Technical_ES.md](./AlphaPOS.VposConnector_Manual_Technical_ES.md)
- Instrucciones de ejecución de VPOS REST (resumen): [VPOS_Run_Instructions.md](../VPOS_Run_Instructions.md)
- Pasos de despliegue para producción (copy/paste): [VPOS_Production_Steps.md](../VPOS_Production_Steps.md)

Pasos recomendados:
- Revisar example_from_vfp.prg para patrones de integración con VFP y adaptar un parser JSON robusto.
- Leer los manuales listados para detalles de configuración, autenticación y certificación.
- Actualizar el README con endpoints reales y ejemplos que el proveedor VPOS proporcione.
