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
- VposConnector.cs                 (implementación básica HTTP/INI)
- README.md                        (esta documentación)
- example_from_vfp.prg             (ejemplo de uso desde VFP)

Construcción y registro
-----------------------
1. Abrir Visual Studio (Developer Command Prompt) o MSBuild.
2. Compilar en Release (PlatformTarget: x86).
   - Con MSBuild: msbuild AlphaPOS.VposConnector.csproj /p:Configuration=Release
3. Registrar la DLL para COM (usar la versión de RegAsm de .NET Framework 4.x x86):
   C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe AlphaPOS.VposConnector.dll /codebase /tlb

Notas sobre configuración
-------------------------
- El método Init(iniPath) lee la clave [General] Merchant_Server del archivo .ini. Se espera que Merchant_Server contenga la URL base (por ejemplo: https://merchant.megasoft.com/api).
- Los métodos StartTransaction, PollStatus, GetVoucher y CancelTransaction construyen rutas relativas simples a partir de esa URL. Ajustar si su endpoint usa rutas diferentes.

Ejemplo en VFP
--------------
LOCAL oConn, lcResp
oConn = CREATEOBJECT("AlphaPOS.VposConnector")
IF oConn.Init("C:\\ruta\\a\\config.ini")
  =MESSAGEBOX(oConn.TestConnection())
  lcResp = oConn.StartTransaction('{"monto":125.50,"cedula":"V12345678"}')
  ? lcResp
ENDIF

Siguientes pasos sugeridos
-------------------------
1. Confirmar endpoints exactos (paths) para transacciones, status, voucher y cancel.
2. Añadir manejo de autenticación (API keys, certificados) si el Merchant Server lo requiere.
3. Implementar logging, retries y soporte de timeouts configurables.
4. Crear pruebas de integración y preparar paquete para certificación.

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

