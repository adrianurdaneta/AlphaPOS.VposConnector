# Changelog

All notable changes to this project.

## [2026-05-11] - API específica VFP + alineamiento VPOS

### Added
- Nueva API COM específica por caso de negocio para uso desde Visual FoxPro sin construir JSON de entrada:
  - Tarjetas: `PagarTarjetaCredito`, `PagarTarjetaDebito`, `PagarTarjetaOtras`, `AnularTarjetaPorSecuencia`
  - Verificaciones: `VerificarP2C`, `VerificarTransferencia`, `VerificarDeposito`
  - Cards: `CardsConsultarSaldo`, `CardsPagarConOtp`
  - Lysto: `LystoSolicitarOrden`, `LystoCrearOrden`, `LystoPagarCuotaInicialC2P`, `LystoConfirmarOrden`, `LystoCancelarOrden`
  - Cashea: `CasheaCrearOrden`, `CasheaConfirmarOrden`, `CasheaCancelarOrden`
  - Otros medios: `PagarCrixto`, `PagarXcapit`, `PagarAccessPay`
  - Voucher/servicio: `ImprimirUltimoVoucherAprobado`, `ImprimirUltimoVoucherProcesado`, `Ping`, `TerminarServicio`
- Propiedades de resultado rápido para evitar parseo JSON en VFP:
  - `LastCode`, `LastMessage`, `LastStatus`, `LastSequence`, `LastVoucher`, `LastOrderId`, `LastReference`, `LastRawResponse`
- Método de escape avanzado:
  - `ExecuteRaw(string endpoint, string jsonRequest)`
- Soporte interno explícito para endpoints observados de VPOS local:
  - `/vpos/metodo`, `/vpos/metodo_cards`, `/vpos/metodo_lysto`, `/vpos/metodo_terminate`

### Changed
- Se mantiene compatibilidad con métodos legacy, pero el enfoque recomendado para nuevas integraciones es API específica por negocio.
- Documentación actualizada para reflejar arquitectura observada de VPOS local y flujo de certificación:
  - `README.md`
  - `AlphaPOS.VposConnector_Manual_ES.md`
  - `AlphaPOS.VposConnector_Manual_Technical_ES.md`
  - `example_from_vfp.prg` (ejemplos completos de API específica + legacy)
  - `CONTEXTO_VPOS_Y_PROYECTO.md`
  - `PLAN_ALINEAMIENTO_VPOS.md`

### Legacy / Compatibility Notes
- Métodos legacy que continúan disponibles por compatibilidad:
  - `TestConnection`, `StartTransaction`, `PollStatus`, `GetVoucher`, `CancelTransaction`
- En integraciones nuevas se recomienda migrar progresivamente a métodos específicos.
- Comportamiento especial legacy:
  - métodos string devuelven `"NO_INIT"` cuando no se ha ejecutado `Init(...)` correctamente.

### Return Contract (API específica)
- `1` = aprobada/procesada
- `0` = rechazada/no aprobada
- `-1` = error técnico

### Migration Guidance
1. Mantener llamadas legacy en producción estable.
2. Introducir en paralelo métodos específicos para nuevos casos.
3. Usar propiedades `Last*` para diagnóstico y control de flujo en VFP.
4. Validar casos contra el script de certificación: `Script_Certificacion_GrupoIDB_Vpos3.15.10.xlsx`.

## [Unreleased] - 2026-04-20

### Added
- COM-visible .NET Framework 4.8 DLL AlphaPOS.VposConnector (x86) with methods: Init, TestConnection, StartTransaction, PollStatus, GetVoucher, CancelTransaction.
- Clean Architecture refactor (Domain/Application/Infrastructure/Adapter).
- ApiKey authentication with TLS client-certificate fallback (PFX).
- Configurable INI support and FileLogger.
- Unit tests (MSTest) for Application and HttpClientWrapper (x86 test host).
- Examples: example_from_vfp.prg and VPOS REST test HTML.
- Documentation: AlphaPOS.VposConnector_Manual_ES.md and AlphaPOS.VposConnector_Manual_Technical_ES.md.
- VPOS runtime scripts and resources under the WINDOWS folder (ignored by .gitignore).

### Fixed
- Resolved test runner bitness (x86) and duplicate type collisions after refactor.

