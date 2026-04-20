### Guía de Integración Técnica: AlphaPOS hacia VPOS REST Service

Esta guía técnica establece los lineamientos de arquitectura y desarrollo para integrar el sistema AlphaPOS con el componente  **VPOS Universal Jar (Versión 3.15.9)** . Como Arquitecto de Software, el objetivo es garantizar una comunicación robusta entre el entorno heredado (Visual FoxPro 9\) y el middleware moderno basado en Java, asegurando la integridad de las transacciones financieras y el cumplimiento de los estándares de Mega Soft.

#### 1\. Arquitectura de Integración y Modelo de Comunicación

El  **VPOS Universal Jar**  es un middleware desarrollado en Java que encapsula la lógica de negocio bancaria, la gestión de dispositivos PIN Pad y la comunicación con el Merchant Server.

##### 1.1 El Patrón Bridge (VFP9 \- Java REST)

Dado que Visual FoxPro 9 no puede ejecutar directamente código Java, la integración se basa en un modelo de  **Servicio REST** . El componente JAR se ejecuta como un proceso independiente que expone métodos públicos. La DLL en VFP9 debe actuar como un cliente HTTP (utilizando WinHTTP.WinHttpRequest.5.1 o MSXML2.ServerXMLHTTP) para enviar peticiones JSON y recibir respuestas estructuradas.

##### 1.2 Modalidades de Operación

* **Manejo de Métodos de Bajo Nivel / REST:**  Esta es la modalidad mandatoria para AlphaPOS. El sistema de caja invoca métodos específicos del JAR para iniciar flujos de datos sin depender de la interacción manual del usuario en la interfaz del VPOS, salvo en la captura de datos sensibles en el PIN Pad.  
* **Stand Alone:**  Utilizado exclusivamente para tareas de mantenimiento, diagnóstico y configuración inicial por parte del personal de soporte.

#### 2\. Configuración Técnica del Entorno

La parametrización se realiza en la pestaña "Configuración". Para una integración exitosa, los siguientes parámetros deben definirse con precisión:| Pestaña | Sección | Parámetro Clave | Función Técnica || \------ | \------ | \------ | \------ || **General** | **Merchant Server** | Punto de Conexión | URL del Endpoint del servidor central de Mega Soft. || **General** | **VTerminal** | ID de Terminal | Identificador lógico (TID) asignado al punto de venta. || **General** | **Seguridad** | Protocolos / Login Cards | Gestión de acceso y cifrado de tramas. || **Transacciones** | **Multiempresa** | Activación / Agregar | Habilita la adjudicación de pagos a múltiples RIFs (ID de Empresa). || **Dispositivos** | **PIN Pad** | Puerto / Protocolo | Configuración de comunicación con el hardware (Verifone/Ingenico). || **Vouchers** | **Rutas** | Logs, Temp, Sync | Definición de paths para auditoría y sincronización de archivos. || **Vouchers** | **Reporte Cierre** | Formato / Impresora | Parámetros para la generación del reporte consolidado de lote. |

#### 3\. Especificaciones de Desarrollo para la DLL en Visual FoxPro 9

La DLL debe diseñarse para manejar la asincronía inherente a las transacciones financieras y la serialización de datos.

##### 3.1 Lógica de Control y Manejo de Estados

1. **Serialización JSON:**  Se recomienda el uso de TEXT...ENDTEXT para construir las tramas de solicitud, asegurando que los tipos de datos (especialmente montos decimales) cumplan con el formato esperado por el JAR.  
2. **Monitoreo de la Barra de Mensajes:**  La DLL debe realizar un  *polling*  o esperar el retorno del método para interpretar el estado en la "Barra de Mensajes". El éxito se confirma únicamente ante el valor exacto  **'APROBADA'** .  
3. **Gestión de Timeouts:**  Debido a que el PIN Pad requiere interacción humana (ingreso de PIN/Chip), la DLL debe implementar un timeout extendido (60-120 segundos) antes de considerar una falla de comunicación.  
4. **Manejo de Errores:**  Cualquier respuesta distinta a 'APROBADA' debe ser capturada y devuelta a AlphaPOS para disparar el flujo de reversa o cancelación de factura.

#### 4\. Protocolos de Transacción y Estructuras JSON

Basado en el análisis de las pantallas de ingreso de datos, se deducen las siguientes estructuras de objetos JSON para el intercambio de datos:

##### 4.1 Compra con Tarjeta (EMV/Swipe/Contactless)

Campo,Tipo,Obligatorio,Deducción de Objeto JSON  
monto,Numeric,Sí,"{ ""monto"": 1250.50,"  
cedula\_cliente,String,Sí,"""cedula"": ""V12345678"","  
ultimos\_4\_digitos,String,Sí\*,"""last4"": ""4567"","  
cvv,String,Sí\*,"""cvv"": ""123"","  
tipo\_cuenta,String,Sí (TDD),"""account\_type"": ""corriente"","  
adjudicar\_a,String,Condicional,"""merchant\_id"": ""ID\_001"" }"  
\*Nota: Requeridos para Swipe/Banda. En EMV se capturan directamente del chip.,,,

##### 4.2 Biopago BDV

El sistema transfiere el control a la aplicación BiopagoBDV.

* **JSON Sugerido:**  {"metodo": "biopago", "identificacion": "V", "cedula": "12345678", "monto": 500.00}  
* **Retorno:**  El JAR devolverá el código del emisor y el número de cuenta/tarjeta tras la validación biométrica exitosa.

##### 4.3 C2P (Comercio a Persona) y P2C (Persona a Comercio)

Transacción,Estructura Deducida de Campos Clave  
C2P,"{""cedula"": ""V.."", ""monto"": 0.00, ""banco"": ""0102"", ""telefono"": ""0412.."", ""otp"": ""123456""}"  
P2C,"{""monto"": 0.00, ""tel\_comercio"": ""0414.."", ""banco\_cli"": ""0134"", ""tel\_cli"": ""0416.."", ""moneda"": ""VES""}"

##### 4.4 Anulación y Cierre de Caja

Función,Requerimientos Técnicos  
Anulación,Requiere referencia y monto.  Restricción BNC:  Las preautorizaciones de BNC tienen flujos distintos para EMV vs. Teclado Abierto; la DLL debe validar el banco emisor antes de invocar el método de anulación.  
Cierre de Caja,Ejecución del cierre de lote. AlphaPOS debe capturar el Reporte de Cierre para conciliación interna antes de liberar la sesión.

#### 5\. Gestión de Hardware y Modalidades de Lectura

La DLL debe ser capaz de procesar las respuestas de error del hardware y guiar al usuario:

1. **EMV (Chip):**  Método primario. La DLL debe esperar la inserción y la identificación automática del producto (TDD/TDC).  
2. **Contactless (NFC):**  Se activa solo si el monto es superior al  **"Límite de Piso"**  (Floor Limit) configurado. Para montos menores, el PIN Pad puede no solicitar PIN. Requiere la opción "Captura de Datos Contactless" activa en el JAR.  
3. **Fallback (Banda Magnética):**  Ante el mensaje de error físico  **"ERROR EN CHIP RETIRE TARJETA USE BANDA MAGNÉTICA"** , la DLL debe permitir un reintento enviando el flag de modalidad fallback=true en la trama JSON.

#### 6\. Manejo de Vouchers y Auditoría

La DLL debe proveer funciones de recuperación de datos para prevenir discrepancias financieras:

* **Último Voucher Aprobado:**  Se invoca únicamente para reimprimir transacciones exitosas confirmadas por el banco.  
* **Último Voucher Procesado:**  Crucial para auditoría técnica. Incluye intentos fallidos, declives bancarios o errores de comunicación. AlphaPOS debe registrar este log en caso de excepciones durante la fase de "PROCESANDO".

#### 7\. Procedimiento de Certificación y Mantenimiento

Para que AlphaPOS sea certificado en la plataforma MS, el menú de administración del sistema debe integrar las siguientes tareas de mantenimiento:

1. **Test de Comunicación:**  Validación de latencia y visibilidad con el Merchant Server.  
2. **Sincronizar Archivos:**  Descarga obligatoria de tablas de bines, parámetros de bancos y versiones de software.  
3. **Intercambio de Llaves Públicas EMV:**  Proceso crítico para la seguridad de transacciones con Chip; debe ejecutarse durante la instalación y periódicamente según políticas de seguridad.  
4. **Validación de Ciclo:**  Ejecución de transacciones de prueba (Compra \+ Anulación \+ Cierre) verificando que el lote cierre correctamente en el MS.

#### 8\. Restricciones y Reglas de Negocio

* **Confidencialidad:**  Toda la documentación y protocolos son propiedad de  **Mega Soft Computación C.A.** . El código de la DLL no debe exponer llaves ni credenciales en texto plano.  
* **Seguridad PCI:**  Está prohibido el almacenamiento de datos sensibles (PIN, CVV, Track 2\) en las tablas de AlphaPOS.  
* **Integridad de Datos:**  La DLL debe validar la presencia de campos obligatorios antes de disparar la petición REST para evitar el rechazo preventivo por parte del JAR.

**Documentación técnica para uso exclusivo de integradores de AlphaPOS bajo licencia de Mega Soft (2026).**