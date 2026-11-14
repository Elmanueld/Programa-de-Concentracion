# Revisión de acceso, cuenta y verificación

Esta revisión describe el código actual de `main`. El formulario de acceso vive en `src/CapaDatos/CapaPresentacion/frmLogin.cs` y sus controles visuales en `frmLogin.Designer.cs`; no se usa el antiguo `Form1`.

## Login

El procedimiento de login recibe el correo y devuelve la cuenta autenticada. El lector tolera procedimientos anteriores sin la columna `Nombre`; recupera el nombre usando el `IdUsuario` autenticado. Nunca se hace una búsqueda general de cuentas.

Las contraseñas se verifican con PBKDF2-SHA256 y no se exponen a la presentación después del inicio de sesión. Las lecturas y escrituras de datos privados se limitan al `IdUsuario`.

## Código de correo

El flujo genera códigos de seis dígitos, guarda su resumen con sal, aplica expiración y límite de intentos, y consume el código tras verificar. SMTP real requiere configuración externa. El valor actual `ModoDemostracion=true` mantiene el registro/login de demostración sin enviar correo ni marcar la cuenta como verificada automáticamente.

## Verificación

La prueba de interfaz comprueba navegación de login/registro/verificación. Las pruebas de integración usan solo `(localdb)\Entregable3Pruebas`. El [reporte actual](REPORTE_DE_PRUEBAS.md) contiene resultados y límites de la ejecución.
