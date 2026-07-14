Vpos Universal 3.15.9 Certificada
=================================

Resumen
-------
Repositorio con la DLL COM `AlphaPOS.VposConnector` para integrar AlphaPOS (VFP9) con VPOS REST local.

Estado actual del enfoque
-------------------------
Se adoptó enfoque **REST local directo**:
- Base URL por defecto: `http://localhost:8085`
- Endpoint principal: `POST /vpos/metodo`
- Sin uso de INI ni API key en la DLL.

Proyecto principal
------------------
- `AlphaPOS.VposConnector/` -> código fuente de la DLL
- `AlphaPOS.VposConnector.Tests/` -> pruebas MSTest
- `example_from_vfp.prg` -> ejemplo de uso desde VFP

Métodos de Fase 1 expuestos por la DLL
--------------------------------------
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

Compilar
--------
msbuild AlphaPOS.VposConnector\AlphaPOS.VposConnector.csproj /p:Configuration=Release

Registro COM (x86)
------------------
C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe AlphaPOS.VposConnector.dll /codebase /tlb
