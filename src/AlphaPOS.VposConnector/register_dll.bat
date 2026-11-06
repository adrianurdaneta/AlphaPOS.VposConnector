@echo off
REM Registrar/Anular registro de AlphaPOS.VposConnector.dll (COM)
REM Uso: register_dll.bat [install|uninstall]

SETLOCAL
n:: Ruta del proyecto (carpeta donde reside este .bat)
SET "PROJDIR=%~dp0"
SET "DLL=%PROJDIR%bin\Release\net48\AlphaPOS.VposConnector.dll"
SET "REGASM=%windir%\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe"

:: Comprobar privilegios (aviso)
net session >nul 2>&1
IF %ERRORLEVEL% NEQ 0 (
  echo Advertencia: Este script puede requerir privilegios de administrador para completar el registro.
)

:: Acción por defecto: install
IF "%1"=="" SET "ACTION=install"
IF /I "%1"=="install" SET "ACTION=install"
IF /I "%1"=="uninstall" SET "ACTION=uninstall"

echo Using RegAsm: "%REGASM%"
echo DLL: "%DLL%"
echo Action: %ACTION%

IF NOT EXIST "%DLL%" (
  echo ERROR: DLL no encontrada: "%DLL%"
  pause
  EXIT /B 1
)

IF NOT EXIST "%REGASM%" (
  echo ERROR: RegAsm no encontrado en: "%REGASM%"
  echo Asegúrate de tener .NET Framework instalado o edita la variable REGASM en este script.
  pause
  EXIT /B 1
)

IF /I "%ACTION%"=="install" (
  echo Registrando...
  "%REGASM%" "%DLL%" /codebase /tlb
  IF %ERRORLEVEL% EQU 0 (
    echo Registro completado correctamente.
  ) ELSE (
    echo Registro falló con código %ERRORLEVEL%.
  )
) ELSE IF /I "%ACTION%"=="uninstall" (
  echo Anulando registro...
  "%REGASM%" /unregister "%DLL%"
  IF %ERRORLEVEL% EQU 0 (
    echo Anulación completada correctamente.
  ) ELSE (
    echo Anulación falló con código %ERRORLEVEL%.
  )
) ELSE (
  echo Acción desconocida: %ACTION%. Usa install o uninstall.
  pause
  EXIT /B 1
)

echo Hecho.
ENDLOCAL
pause
