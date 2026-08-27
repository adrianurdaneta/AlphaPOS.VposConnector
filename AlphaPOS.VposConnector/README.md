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
- `VerificarP2C(monto, cedula)`
- `VerificarTransferencia(monto, cedula)`
- `VerificarBiopago(monto, cedula)`
- `PagarConCambio(monto, cedula, tipoMoneda)`
- `PagarBiopago(monto, cedula)`
- `ObtenerMediosPago()` retorna un CSV con los medios de pago.
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
- `LastVoucherPath`
- `LastTerminal`, `LastTid`, `LastLote`
- `LastTipoTransaccion`, `LastFechaTransaccion`, `LastHoraTransaccion`
- `LastNumeroAutorizacion`, `LastCodigoAfiliacion`, `LastCodigoAdquiriente`, `LastNroAutorizador`
- `LastTipoTarjeta`, `LastTipoProducto`, `LastTipoCuenta`, `LastMedioPago`
- `LastMontoTransaccion`, `LastMontoServicios`, `LastMontoDonativo`, `LastMoneda`, `LastExisteCopiaVoucher`

Las propiedades `Last*` pertenecen a la última llamada y no deben leerse desde una instancia compartida entre operaciones concurrentes.

Compilación
-----------
msbuild AlphaPOS.VposConnector.csproj /p:Configuration=Release /p:PlatformTarget=x86

Registro COM (x86)
------------------
C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe AlphaPOS.VposConnector.dll /codebase /tlb

Ejemplo VFP
-----------
Ver `example_from_vfp.prg`.

Pruebas
-------
Ejecutar con un runner x86:

`dotnet test ..\AlphaPOS.VposConnector.Tests\AlphaPOS.VposConnector.Tests.csproj --no-restore -p:PlatformTarget=x86`
