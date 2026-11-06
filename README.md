AlphaPOS.VposConnector
=======================

Resumen
-------
Repositorio con la DLL COM `AlphaPOS.VposConnector` para integrar AlphaPOS (VFP9) con VPOS REST local.

Este repositorio contiene únicamente el conector: el runtime del proveedor (`Windows/`) no se
versiona ni se publica.

Estado actual del enfoque
-------------------------
La DLL usa REST local directo:
- Base URL por defecto: `http://localhost:8085`
- Endpoint: `POST /vpos/metodo`
- Sin INI, API key ni certificados en la implementación actual.

Estructura del repositorio
--------------------------
- `src/AlphaPOS.VposConnector/` -> código fuente de la DLL
- `tests/AlphaPOS.VposConnector.Tests/` -> pruebas MSTest
- `docs/` -> manuales del conector, revisión técnica y script de certificación
- `samples/` -> ejemplos de uso desde VFP

Las carpetas `_vendor/` (material del proveedor) y `_scratch/` (borradores) están ignoradas por git
y no forman parte de la publicación.

Métodos de Fase 1 expuestos por la DLL
--------------------------------------
- `PagarTarjetaDebito(monto, cedula)`
- `PagarTarjetaCredito(monto, cedula)`
- `VerificarP2C(monto, cedula)`
- `VerificarTransferencia(monto, cedula)`
- `VerificarBiopago(monto, cedula)`
- `PagarConCambio(monto, cedula, tipoMoneda)`
- `PagarBiopago(monto, cedula)`
- `ObtenerMediosPago()` retorna una lista CSV de valores de medios de pago.
- `ImprimirUltimoVoucherAprobado()`
- `EjecutarPrecierre()`
- `EjecutarCierre()`
- `AnularTarjetaPorSecuencia(secuencia, cedula)`

Los montos se reciben como `string` porque la API COM actual conserva el formato recibido por VFP.

Configuración en runtime
------------------------
- `SetBaseUrl(url)` cambia la URL base.
- `SetTimeout(ms)` cambia el timeout cuando `ms` es mayor que cero.
- `SetLogPath(path)` cambia el archivo de log.

Todas las operaciones usan una instancia local. No se debe compartir una instancia entre llamadas concurrentes porque las propiedades `Last*` representan el último resultado.

Compilar
--------
msbuild src\AlphaPOS.VposConnector\AlphaPOS.VposConnector.csproj /p:Configuration=Release /p:PlatformTarget=x86

Registro COM (x86)
------------------
C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe AlphaPOS.VposConnector.dll /codebase /tlb

Pruebas
-------
La DLL y el runner deben ejecutarse como x86:

`dotnet test tests\AlphaPOS.VposConnector.Tests\AlphaPOS.VposConnector.Tests.csproj --no-restore -p:PlatformTarget=x86`

El flag `-p:PlatformTarget=x86` no es opcional: sin él el runner arranca en 64 bits y la carga de la
DLL falla con `BadImageFormatException`.

Revisión técnica
----------------
Ver `docs/REVISION_2026-08-27.md`.
