### Guía Técnica de Integración: AlphaPOS & VPOS Universal Jar (v3.15.9)

#### 1\. Ficha Técnica y Control de Versiones

Esta guía técnica está dirigida a desarrolladores responsables de la integración de AlphaPOS con el middleware VPOS Universal. El componente, desarrollado en Java, está diseñado para la gestión de transacciones financieras mediante dos modalidades:  **interfaz de caja independiente**  y  **manejo de métodos públicos de bajo nivel** , siendo esta última la recomendada para la integración vía DLL/Middleware.| Atributo | Detalle || \------ | \------ || **Versión del Componente** | 3.15.9 || **Fecha de Publicación** | 05/01/2026 || **Responsable Técnico** | José Silvera || **Código de Documento** | MAUS-VPOSJ-00\_FEB.2026 |

##### Cambios Clave en la Versión 3.15.9

* **Agencia Virtual Biopago:**  Implementación de transacciones de servicios externos (Transferencias, Depósitos, Retiros).  
* **Débito Inmediato:**  Incorporación del campo "Nombre del Cliente" para trazabilidad.  
* **Teclado Virtual:**  Optimización de renderizado para captura de datos en TDD/TDC (Ticket \#56625).  
* **Access Pay:**  Integración del nuevo medio de pago dentro del ecosistema transaccional.  
* **Analizadores Biopago:**  Inclusión de nuevos módulos de análisis para la plataforma de validación biométrica.

#### 2\. Configuración de Entorno (.ini y Parámetros)

La lógica del middleware se rige por un archivo de configuración jerárquico. Para la integración con AlphaPOS, es imperativo configurar las coordenadas de ubicación, ya que estas determinan el comportamiento de la  **UI Modal**  cuando es invocada por la DLL.  
\[General\]  
; Conectividad y lógica de negocio  
Merchant\_Server \= \[URL o IP del Servidor de Megasoft\]  
VTerminal \= \[ID de Terminal Virtual asignado\]  
Formato\_Monto \= \[Configuración de decimales, ej: 2\]  
Seguridad \= \[Cifrado de tramas y llaves de acceso\]

\[Dispositivos\]  
; Configuración de periféricos y automatización  
Pinpad\_Model \= Verifone  
Lector\_Cheques \= \[Habilitado/Inhabilitado\]  
Teclado\_Virtual \= \[Habilitado para captura de PIN/Datos en pantalla\]  
Test\_Automatico \= \[True/False \- Ejecución de diagnóstico al inicio\]

\[Vouchers\]  
; Control de impresión (VPOS gestiona la cola de impresión)  
Ruta\_Impresion \= \[Path local o de red\]  
Encabezado\_Personalizado \= \[Texto legal o comercial\]  
Original\_Copia \= \[True/False \- Emisión de soporte para el cliente\]

\[Multiempresa\]  
; Gestión de múltiples RIFs  
Activacion \= \[True/False\]  
Gestion\_Empresas \= \[IDs de adjudicación separados por comas\]

\[Ubicacion\]  
; Vital para el posicionamiento de ventanas modales sobre AlphaPOS  
Coordenadas\_X \= \[Valor numérico\]  
Coordenadas\_Y \= \[Valor numérico\]

\[Captura\_Firma\]  
; Parámetros para dispositivos con panel de firma  
Activacion \= \[True/False\]

#### 3\. Modalidades de Procesamiento y Lectura de Tarjetas

El integrador debe manejar la lógica de estados del PIN Pad según la modalidad de lectura.| Modalidad | Descripción Técnica | Comportamiento del PIN Pad / Lógica de PIN || \------ | \------ | \------ || **EMV (Chip)** | Lectura de tarjetas crédito/débito inteligentes. | Inserción obligatoria. PIN requerido según perfil de la tarjeta. || **EMV Contactless** | Lectura NFC por proximidad. | Acercamiento al lector. Si el monto supera el  **Límite de Piso** , el PIN es obligatorio. || **Fallback** | Flujo de contingencia por fallo de lectura de chip. | El sistema arroja: "ERROR EN CHIP RETIRE TARJETA USE BANDA MAGNÉTICA". Se habilita el deslizamiento. || **Banda Magnética** | Lectura de tarjetas convencionales (No EMV). | Deslizamiento lateral. Requiere ingreso manual de datos complementarios en UI. |

#### 4\. Diccionario de Campos para Estructuras JSON (Request/Response)

A continuación, se definen los tipos de datos y obligatoriedad. El símbolo  **(©)**  indica campos obligatorios para transacciones EMV.

* monto\_bs  **(©)** :  *String (Decimal 99999.99).*  Monto de la operación.  **Nota:**  Si AlphaPOS envía este valor, el campo aparecerá inhabilitado (read-only) en la UI de VPOS.  
* cedula\_cliente  **(©)** :  *String (Numérico).*  Cédula del pagador. Si es enviada por AlphaPOS, se precarga pero permite edición.  
* tipo\_cuenta  **(©)** :  *Integer (Corriente=0, Ahorro=1, Otra=2).*  Requerido estrictamente para Débito.  
* id\_adjudicar  **(©)** :  *String.*  Requerido solo si Multiempresa=True.  
* ultimos\_4\_digitos:  *String (4 caracteres).*  Presente en EMV y Banda para el enmascaramiento de seguridad.  
* cvv:  *String (3 o 4 dígitos).*  Requerido en transacciones manuales/banda. 4 dígitos para American Express.  
* codigo\_operacion:  *String.*  Específico para flujos C2P o validación OTP en P2C.  
* banco\_emisor:  *String (ID Banco).*  Requerido para transferencias y pagos móviles.  
* numero\_telefono:  *String.*  Teléfono asociado al emisor del pago (C2P/P2C).

#### 5\. Endpoints y Flujos por Medio de Pago

##### Pago con Tarjeta y Selección de Producto

Al detectar la tarjeta, el middleware identifica productos asociados. La DLL debe estar preparada para recibir una selección de sub-transacción, especialmente con  **Banesco**  (ej: Compra, Consulta Extracrédito, Autorizar Extracrédito).

##### Flujo Biopago (Control Externo)

* AlphaPOS invoca el método Biopago.  
* VPOS transfiere el control a BiopagoBDV.  
* **La DLL debe esperar el payload de retorno:**  
* *Éxito:*  Retorna "Código de Emisor" y "Número de tarjeta/cuenta".  
* *Fallo:*  Retorna código de error técnico para ser parseado por AlphaPOS.

##### Flujo Criptomonedas (Ciclo de Vida del QR)

* Generación y despliegue del QR en la UI Modal.  
* Inicio de cronómetro de espera (Configurable en .ini).  
* **Lógica de Ramificación (Branching):**  
* Si se recibe confirmación del Wallet \-\> Transacción Aprobada.  
* Si ocurre  **Timeout** , se despliega Aviso: "¿Desea continuar esperando?".  
* Si el usuario selecciona "Terminar" o se agotan los reintentos \-\> TRANSACCION CANCELADA.

##### P2C / C2P Tokenizador

Si la modalidad está activa, el sistema verifica la cédula. Si no hay cuentas, despliega el formulario de registro. Tras la aprobación, la cuenta queda tokenizada para futuras consultas.

#### 6\. Gestión de Transacciones Administrativas

Operación,Tipo / Caso,Requerimiento Técnico  
Anulación,Caso 1 (Chip/Swipe),Requiere presencia de tarjeta para lectura de track/chip.  
Anulación,Caso 2 (Teclado),Permite ingreso manual de datos de la transacción original.  
Anulación BNC,Caso Especial,ADVERTENCIA:  Las preautorizaciones de BNC tienen un menú de anulación diferenciado e independiente.  
Cierre de Caja,Pre-Cierre,Reporte de totales en pantalla/impresora sin borrado de memoria.  
Cierre de Caja,Cierre Definitivo,Transmisión de lotes y reinicio de contadores transaccionales.  
Misceláneos,Sincronización,"Obligatorio:  Ejecutar ""Sincronizar Archivos"" e ""Intercambio de Llaves Públicas EMV"" tras cada cierre o cambio de PIN Pad."

#### 7\. Códigos de Respuesta y Estados de la Barra de Mensajes

La DLL debe interpretar los strings retornados en la barra de mensajes para actualizar el estado del pedido en AlphaPOS.| Mensaje Retornado | Significado | Acción sugerida en AlphaPOS || \------ | \------ | \------ || APROBADA | Éxito total. | Cerrar factura y emitir comprobante. || TRANSACCION CANCELADA | Aborto manual (Esc/Cancelar). | Liberar interfaz y permitir reintento. || ERROR EN CHIP | Fallo de hardware. | Disparar flujo de Fallback (Banda). || REINTENTE PIN | Error de captura de clave. | Solo si la modalidad de reintentos está activa. |  
**Nota sobre Vouchers:**  El middleware retorna la estructura de texto completa del voucher. VPOS controla directamente la cola de impresión para asegurar original y copia.

#### 8\. Restricciones Técnicas y Formatos de Salida

* **Renderizado de UI:**  La DLL no debe intentar renderizar elementos gráficos (diagramas o iconos) fuera de los proporcionados por el jar. Toda interacción de datos se maneja mediante bloques de texto simple y objetos JSON.  
* **Gestión de Impresión:**  AlphaPOS debe delegar la responsabilidad de la integridad del voucher a VPOS. La impresión se realiza en modo bloque para garantizar la validez legal del soporte físico.  
* **Estado de la DLL:**  Se recomienda realizar un "Test de Comunicación" como rutina de inicio (Handshake) para verificar la disponibilidad del Merchant Server antes de procesar ventas.

