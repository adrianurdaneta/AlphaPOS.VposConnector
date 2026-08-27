# AlphaPOS.VposConnector - Manual de la DLL

Versión: API actual del código fuente

## Resumen

`AlphaPOS.VposConnector` es una DLL COM para Visual FoxPro 9, compilada para .NET Framework 4.8 y plataforma x86. Se conecta directamente al servicio REST local de VPOS.

## Requisitos

- Windows.
- .NET Framework 4.8.
- Aplicación consumidora de 32 bits, como Visual FoxPro 9.
- Servicio VPOS disponible en `http://localhost:8085`, salvo que se configure otra URL.

## Configuración

La implementación actual no lee INI ni usa API keys o certificados.

```text
SetBaseUrl("http://localhost:8085")
SetTimeout(120000)
SetLogPath("C:\\logs\\AlphaPOS.VposConnector.log")
```

`SetTimeout` sólo cambia el valor cuando recibe un número mayor que cero. El endpoint usado es `POST {baseUrl}/vpos/metodo`.

## Registro COM

Compilar para x86 y registrar con la versión x86 de `RegAsm`:

```text
msbuild AlphaPOS.VposConnector.csproj /p:Configuration=Release /p:PlatformTarget=x86
C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe AlphaPOS.VposConnector.dll /codebase /tlb
```

ProgId: `AlphaPOS.VposConnector`.

## Métodos públicos

Todos los montos se reciben como `string` y las operaciones que retornan `int` usan esta convención:

- `1`: aprobada o procesada.
- `0`: rechazada o no aprobada.
- `-1`: error técnico.

### Operaciones

```text
string TestConnection()
void SetBaseUrl(string baseUrl)
void SetLogPath(string path)
void SetTimeout(int ms)
void ClearLastResult()

int PagarTarjetaDebito(string monto, string cedula)
int PagarTarjetaCredito(string monto, string cedula)
int VerificarP2C(string monto, string cedula)
int VerificarTransferencia(string monto, string cedula)
int VerificarBiopago(string monto, string cedula)
int PagarConCambio(string monto, string cedula, string tipoMoneda)
int PagarBiopago(string monto, string cedula)
string ObtenerMediosPago()
int ImprimirUltimoVoucherAprobado()
int EjecutarPrecierre()
int EjecutarCierre()
int AnularTarjetaPorSecuencia(string secuencia, string cedula)
```

`TestConnection` ejecuta la acción `obtenerMediosPago`. `ObtenerMediosPago` también ejecuta esa acción y devuelve los valores encontrados en `mediosPago` separados por comas.

## Propiedades de resultado

Después de cada operación se actualizan las propiedades `Last*`:

```text
LastCode, LastMessage, LastStatus, LastSequence
LastVoucher, LastVoucherPath, LastOrderId, LastReference
LastRawResponse, LastJsonRequest, LastJsonResponse, LastUrlRequest
LastTerminal, LastTid, LastLote, LastTipoTransaccion
LastFechaTransaccion, LastHoraTransaccion, LastNumeroAutorizacion
LastCodigoAfiliacion, LastCodigoAdquiriente, LastNroAutorizador
LastTipoTarjeta, LastTipoProducto, LastTipoCuenta, LastMedioPago
LastMontoTransaccion, LastMontoServicios, LastMontoDonativo
LastMoneda, LastExisteCopiaVoucher
```

`LastStatus` puede ser `APPROVED`, `REJECTED` o `ERROR`.

## Ejemplo mínimo en VFP

```text
LOCAL oConn, lnRc
oConn = CREATEOBJECT("AlphaPOS.VposConnector")
oConn.SetBaseUrl("http://localhost:8085")
oConn.SetTimeout(120000)

lnRc = oConn.PagarTarjetaDebito("100.00", "V16139601")
? lnRc, oConn.LastStatus, oConn.LastMessage

lcMedios = oConn.ObtenerMediosPago()
? lcMedios
```

## Logging y concurrencia

Por defecto el log se escribe como `AlphaPOS.VposConnector.log` en el directorio base de la aplicación. La instancia mantiene el último resultado en propiedades mutables; no se debe compartir una misma instancia entre llamadas concurrentes.

## Pruebas

El proyecto de pruebas debe ejecutarse como x86:

```text
dotnet test ..\AlphaPOS.VposConnector.Tests\AlphaPOS.VposConnector.Tests.csproj --no-restore -p:PlatformTarget=x86
```

El informe técnico de la revisión está en `REVISION_2026-08-27.md`.
