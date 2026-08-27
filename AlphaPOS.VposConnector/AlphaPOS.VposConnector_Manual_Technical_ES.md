# AlphaPOS.VposConnector - Manual técnico

## Arquitectura implementada

```text
VFP/COM -> VposConnector -> TransactionService -> HttpClientWrapper -> VPOS REST local
```

Componentes:

- `VposConnector.cs`: fachada COM, construcción de solicitudes y extracción de resultados.
- `Application/TransactionService.cs`: URL base, endpoint y timeout.
- `Infrastructure/Http/HttpClientWrapper.cs`: solicitudes HTTP síncronas.
- `Infrastructure/Logging/FileLogger.cs`: log de texto con timestamp UTC.
- `Domain/ITransactionService.cs`: contrato del servicio de transacciones.

## Contrato HTTP

- URL base inicial: `http://localhost:8085`.
- Ruta: `/vpos/metodo`.
- Método: `POST`.
- Content-Type: `application/json`.
- Timeout inicial: `120000` ms.

Cada operación de negocio construye un JSON con `accion` y los parámetros correspondientes. La respuesta se conserva completa en `LastRawResponse` y se interpreta para establecer el código y estado simplificados.

## Acciones implementadas

| Método | Acción o comportamiento |
| --- | --- |
| `PagarTarjetaDebito` | `tarjeta`, sin `medioPago` |
| `PagarTarjetaCredito` | `tarjeta`, sin `medioPago` |
| `VerificarP2C` | `tarjeta`, `medioPago=MedioPagoP2C` |
| `VerificarTransferencia` | `tarjeta`, `medioPago=MedioPagoTransferencia` |
| `VerificarBiopago` | `tarjeta`, `medioPago=MedioPagoBiopago` |
| `PagarBiopago` | Delegado a `VerificarBiopago` |
| `PagarConCambio` | `cambio` |
| `AnularTarjetaPorSecuencia` | `anulacion` |
| `ObtenerMediosPago` | `obtenerMediosPago`, devuelve CSV |
| `ImprimirUltimoVoucherAprobado` | `imprimeUltimoVoucher` |
| `EjecutarPrecierre` | `precierre` |
| `EjecutarCierre` | `cierre` |

## Resultado

La clasificación se realiza en `SetResultFromResponse`:

- `APPROVED` y retorno `1`: código `00` o mensaje que contiene `APROBADA`.
- `REJECTED` y retorno `0`: código distinto de `00` o mensaje de rechazo.
- `ERROR` y retorno `-1`: respuesta vacía, error técnico, excepción o respuesta no reconocida.

Los campos conocidos se extraen a propiedades `Last*`. La implementación actual usa expresiones regulares, no un parser JSON general.

## Plataforma y pruebas

La DLL está fijada a x86 para compatibilidad con VFP9. El proyecto de tests debe ejecutarse con el mismo target:

```text
dotnet test AlphaPOS.VposConnector.Tests\AlphaPOS.VposConnector.Tests.csproj --no-restore -p:PlatformTarget=x86
```

## Compatibilidad COM

La clase usa ProgId `AlphaPOS.VposConnector`, GUID de clase fijo y `ClassInterfaceType.AutoDual`. Los cambios de firmas o tipos de retorno requieren especial cuidado porque pueden romper consumidores VFP existentes. Consulte `REVISION_2026-08-27.md` antes de distribuir cambios.

## Límites operativos

- Las llamadas son síncronas.
- La instancia conserva estado mutable en `Last*`; use una instancia por flujo u operación.
- La URL base y la ruta de log se pueden cambiar en runtime.
- No hay configuración de autenticación en el código actual.
