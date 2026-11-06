Local lStatus, lError
LOCAL lRespuesta,lRespuesta2

SET PATH TO C:\test
SET DEFAULT TO C:\test

lRespuesta=0

***********************************************
DECLARE INTEGER  OpenFpctrl IN TFHKAIF.DLL  String lpPortName
DECLARE INTEGER  CloseFpctrl IN TFHKAIF.DLL  
DECLARE INTEGER  CheckFprinter IN TFHKAIF.DLL  
DECLARE INTEGER  ReadFpStatus IN TFHKAIF.DLL  LONG @lStatus ,LONG @lError
DECLARE INTEGER  SendCmd IN TFHKAIF.DLL  LONG @lStatus ,LONG @lError,STRING @cmd
DECLARE INTEGER  SendNCmd IN TFHKAIF.DLL  LONG @lStatus ,LONG @lError,STRING @bufferCmd
DECLARE INTEGER  SendFileCmd IN TFHKAIF.DLL  LONG @lStatus ,LONG @lError,STRING @fileCmd
DECLARE INTEGER  UploadStatusCmd IN TFHKAIF.DLL  LONG @lStatus ,LONG @lError,STRING @cmd,string @fileCmd
*************************************************
lRespuesta=0
lRespuesta2=0

lRespuesta=openfpctrl("COM1")
If lRespuesta#1	
	lRespuesta=closefpctrl()
	lRespuesta2=openfpctrl("COM1")
	If lRespuesta2#1	
		=MESSAGEBOX("No se pudo abrir el puerto.",16,"Error")
		Return .F.
	ENDIF	
ENDIF

lCadena = "800"
lRespuesta=sendCmd(@lStatus,@lError,lCadena)
If lRespuesta#1
	Wait WINDOW "Status:" + ALLTRIM(STR(lStatus)) +", "+;
		"Error:" + ALLTRIM(STR(lError))
	lRespuesta=closefpctrl()
	Return .F.
Endif

LOCAL lcArchivo, lnManejador, lcLinea

lcArchivo = "C:\voucher\cierres\Cierre_Caja_GRUPOIDB01_20260716.txt"

&& Reemplaza con tu ruta exacta
lnManejador = FOPEN(lcArchivo, 12)

IF lnManejador > -1
	DO WHILE NOT FEOF(lnManejador)
		lcLinea = FGETS(lnManejador)
		* AQUÍ PROCESAS LA LÍNEA
		* Por ejemplo, mostrarla en pantalla:
		? lcLinea
		lCadena = "80*" + lcLinea
		lRespuesta = sendCmd(@lStatus,@lError,lCadena)
	ENDDO
	
	FCLOSE(lnManejador)
ELSE
	MESSAGEBOX("No se pudo abrir el archivo. Error: " + ALLTRIM(STR(FERROR())))
ENDIF

lCadena = "810"
lRespuesta=sendCmd(@lStatus,@lError,lCadena)

lRespuesta=closefpctrl()

RETURN .T.