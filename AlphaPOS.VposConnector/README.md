AlphaPOS.VposConnector (COM-visible .NET Framework 4.8)
=====================================================

Resumen
-------
Conector COM para Visual FoxPro 9 orientado a **VPOS REST local directo** en `http://localhost:8085`, sin carga de INI ni manejo de API key en la DLL.

Enfoque operativo
-----------------
- Endpoint principal: `POST /vpos/metodo`
- Modo de uso esperado: crear objeto COM y llamar métodos de negocio simples.
- Base URL por defecto: `http://localhost:8085`
- Configuración opcional en runtime:
  - `SetBaseUrl(url)`
  - `SetTimeout(ms)`
  - `SetLogPath(path)`

Métodos expuestos (Fase 1)
--------------------------
- `PagarTarjetaDebito(monto, cedula)`
- `PagarTarjetaCredito(monto, cedula)`
- `VerificarP2C(monto, telefono, banco)`
- `PagarConCambio(monto, cedula, tipoMoneda)`
- `PagarBiopago(monto, cedula)`
- `ObtenerMediosPago()`
- `ImprimirUltimoVoucherAprobado()`
- `EjecutarPrecierre()`
- `EjecutarCierre()`
- `AnularTarjetaPorSecuencia(secuencia, cedula)`

Estado de último resultado
--------------------------
- `LastCode`
- `LastMessage`
- `LastStatus`
- `LastSequence`
- `LastVoucher`
- `LastReference`
- `LastOrderId`
- `LastRawResponse`

Compilación
-----------
msbuild AlphaPOS.VposConnector.csproj /p:Configuration=Release

Registro COM (x86)
------------------
C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe AlphaPOS.VposConnector.dll /codebase /tlb

Ejemplo VFP
-----------
Ver `example_from_vfp.prg`.
