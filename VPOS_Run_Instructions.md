VPOS REST - Instrucciones de ejecución (resumen)
===============================================

Resumen rápido
--------------
Hay un paquete Java listo en Windows\ (incluye jre). Los scripts relevantes están en:
C:\dev\Vpos Universal 3.15.9 Certificada\Windows

El servicio REST arranca con VposREST.bat y por defecto escucha en:
http://localhost:8085/vpos/**

Pasos para instalar/ejecutar, verificar y desinstalar
----------------------------------------------------

1) Archivos clave (ruta en el repo)
- Instalador: Windows\InstalacionVPOSREST.bat
- Desinstalador: Windows\DesinstalacionVPOSREST.bat
- Arranque manual: Windows\VposREST.bat
- Monitor (tarea programada): Windows\InstalarMonitorVposRest.bat / Windows\desinstalarMonitorVposRest.bat
- Pruebas/recursos: Windows\rest\html\TestVPosREST.html
- Config principal: Windows\conf\vposconf.ini
- Logs configurados: Windows\conf_visanet\log4j.xml (ejemplo apunta a C:\voucher\logs\vpos.log)

2) Requisitos previos
- Copiar la carpeta Windows al servidor de destino (por ejemplo: C:\Program Files\VPOSRest).
- Ejecutar acciones con privilegios de administrador cuando se registren tareas o se abran puertos.
- No es obligatorio tener Java instalado globalmente: el paquete incluye jre\.

3) Instalación básica (auto-start por usuario)
- Abrir CMD como Administrador.
- CD a la carpeta Windows.
- Ejecutar: InstalacionVPOSREST.bat
  - Crea VposREST.vbs y lo copia a la carpeta Startup del usuario (autoinicio).
  - Actualiza conf/reinicioVpos.properties con la ruta de instalación.

4) Iniciar/ejecutar manual
- Para lanzar en background: ejecutar VposREST.bat (doble clic o CMD):
  VposREST.bat
- Para ver salida en consola: editar VposREST.bat y usar java (no javaw) o ejecutar manualmente jre\bin\java.exe con el classpath.
- Parar proceso: taskkill /F /IM javaw.exe (cuidado, afecta todas las instancias javaw).

5) Monitor / tarea programada
- Crear monitor: InstalarMonitorVposRest.bat (registra tarea programada VposTest que ejecuta VposTest\vpostest.vbs cada 5 minutos).
- El XML del task está en Windows\VposTest\VposTest.xml: revisarlo y ajustar las rutas antes de registrar.

6) Verificar servicio y pruebas
- Abrir Windows\rest\html\TestVPosREST.html y ejecutar pruebas (usa http://localhost:8085).
- Ejemplo curl (pruebas locales):
  curl -X POST http://localhost:8085/vpos/metodo -H "Content-Type: application/json" -d '{"accion":"tarjeta","montoTransaccion":10.00,"cedula":"V12345678"}'
- Comprobar escucha: netstat -ano | findstr :8085
- Correlacionar PID → tasklist /FI "PID eq <pid>"

7) Configuración
- Editar Windows\conf\vposconf.ini para ajustar [server] host/port y otros parámetros (pinpad, paths, timeouts).
- Revisar log4j config en Windows\conf_visanet\log4j.xml para ubicación/niveles de log (ej.: C:\voucher\logs\vpos.log).

8) Desinstalación
- Ejecutar Windows\DesinstalacionVPOSREST.bat (elimina el VBS de Startup y finaliza javaw).
- Quitar monitor: Windows\desinstalarMonitorVposRest.bat

9) Recomendaciones operativas
- Para ejecutar como servicio de sistema, usar NSSM o un wrapper de servicio (recomendado) en lugar de Startup VBS.
- Abrir firewall solo si el puerto debe ser accesible desde la red (por ejemplo 8085).
- Proteger certificados y credenciales; revisar permisos de carpetas y logs.

Fin del resumen.
