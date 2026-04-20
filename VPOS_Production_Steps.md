VPOS REST - Instalación y despliegue (comandos listos para copiar/pegar)
=======================================================================

IMPORTANTE: Ejecutar todos los comandos en un Símbolo del sistema o PowerShell con privilegios de Administrador.
Sustituir rutas de origen si el repositorio está en otra ubicación.

1) Crear carpeta de instalación y copiar archivos
------------------------------------------------
:: En CMD (Administrador)
mkdir "C:\Program Files\VPOSRest"
robocopy "C:\dev\Vpos Universal 3.15.9 Certificada\Windows" "C:\Program Files\VPOSRest" /MIR /R:3 /W:5

:: Establecer variable de entorno (opcional, para la sesión y persistente):
set "VPOS_HOME=C:\Program Files\VPOSRest"
setx VPOS_HOME "C:\Program Files\VPOSRest" /M

2) Configurar archivos de configuración
--------------------------------------
:: Modificar vposconf.ini (PowerShell - reemplazo host y port)
powershell -Command "$ini='C:\Program Files\VPOSRest\conf\vposconf.ini'; (Get-Content $ini) -replace '(?m)^(host=).*$', 'host=0.0.0.0' -replace '(?m)^(port=).*$', 'port=8085' | Set-Content -Path $ini -Encoding ASCII"

:: Actualizar reinicioVpos.properties (reemplazar rutaAux por la ruta de instalación)
powershell -Command "$prop='C:\Program Files\VPOSRest\conf\reinicioVpos.properties'; (Get-Content $prop) -replace 'rutaAux','C:\Program Files\VPOSRest' | Set-Content -Path $prop -Encoding ASCII"

:: Alternativa rápida: editar con Notepad
notepad "C:\Program Files\VPOSRest\conf\vposconf.ini"

3) Preparar carpeta de logs y permisos
-------------------------------------
powershell -Command "New-Item -Path 'C:\voucher\logs' -ItemType Directory -Force"
icacls "C:\voucher\logs" /grant "BUILTIN\Administrators":(OI)(CI)F /T
icacls "C:\voucher\logs" /grant "Users":(OI)(CI)M /T

(Ajustar cuentas/privilegios según política de seguridad: puede usarse NETWORK SERVICE o la cuenta específica del servicio.)

4) Abrir puerto en el firewall (si será accesible externamente)
---------------------------------------------------------------
netsh advfirewall firewall add rule name="VPOS REST" dir=in action=allow protocol=TCP localport=8085

5) Registrar arranque automático (tarea programada al inicio)
-------------------------------------------------------------
:: Crear tarea que ejecute el script en cada arranque como SYSTEM (sin contraseña)
schtasks /Create /TN "VPOS REST" /TR "\"C:\Program Files\VPOSRest\VposREST.bat\"" /SC ONSTART /RL HIGHEST /RU "SYSTEM" /F

:: Iniciar inmediatamente la tarea (opcional)
schtasks /Run /TN "VPOS REST"

6) Iniciar manualmente (si no se usa tarea)
-------------------------------------------
start "" "C:\Program Files\VPOSRest\VposREST.bat"

7) Verificar proceso y puerto
-----------------------------
netstat -ano | findstr :8085
:: Identificar PID en la salida y luego:
tasklist /FI "PID eq <PID>"

8) Probar endpoint (ejemplo)
----------------------------
:: Usando curl (Windows 10+ incluye curl):
curl -X POST http://localhost:8085/vpos/metodo -H "Content-Type: application/json" -d "{\"accion\":\"tarjeta\",\"montoTransaccion\":10.00,\"cedula\":\"V12345678\"}"

:: O con PowerShell:
powershell -Command "Invoke-RestMethod -Method Post -Uri 'http://localhost:8085/vpos/metodo' -Body '{\"accion\":\"tarjeta\",\"montoTransaccion\":10.00,\"cedula\":\"V12345678\"}' -ContentType 'application/json'"

9) Ver logs
----------
:: Revisar ubicación definida en Windows\conf_visanet\log4j.xml (ejemplo: C:\voucher\logs\vpos.log)
notepad "C:\voucher\logs\vpos.log"

10) Desinstalación / rollback (comandos)
---------------------------------------
:: Eliminar tarea programada
schtasks /Delete /TN "VPOS REST" /F

:: Quitar regla de firewall
netsh advfirewall firewall delete rule name="VPOS REST"

:: Detener proceso java
taskkill /IM javaw.exe /F

:: Eliminar carpeta de instalación (si procede)
rd /S /Q "C:\Program Files\VPOSRest"

11) (Opcional) Ejecutar como servicio (NSSM - recomendado para producción)
---------------------------------------------------------------------------
:: Descargar NSSM (https://nssm.cc/) y copiar nssm.exe a C:\Windows\System32 
:: Instalar servicio que ejecute el batch (ejemplo):
:: nssm install VposRestService "C:\Windows\System32\cmd.exe" /c "C:\Program Files\VPOSRest\VposREST.bat"
:: Luego:
:: nssm start VposRestService

(Nota: ajustar la cuenta en la que corre el servicio y las rutas. Alternativamente usar un servicio Java wrapper que ejecute directamente java.exe con el classpath en classpathRest.txt.)

Notas finales
-------------
- Revisar y ajustar conf/vposconf.ini, conf_visanet/log4j.xml, y permisos antes de exponer el puerto en la red.
- Probar ampliamente en un entorno de staging antes de producción.
- No dejar credenciales o PFX sin protección en discos de producción; use almacenes de certificados si aplica.

Fin de pasos copy/paste.
