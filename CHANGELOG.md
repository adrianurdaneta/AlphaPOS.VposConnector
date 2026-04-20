# Changelog

All notable changes to this project.

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

