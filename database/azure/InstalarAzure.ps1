# Instala la base de datos en Azure SQL Database usando los scripts originales de la carpeta database.
# La base se crea antes desde el portal de Azure. Azure no permite USE ni CREATE DATABASE con archivos,
# por eso se salta el script 01 y se quita la línea USE en memoria (los archivos no se modifican).
param(
    [Parameter(Mandatory = $true)]
    [string] $Servidor,      # Ejemplo: miservidor.database.windows.net
    [Parameter(Mandatory = $true)]
    [string] $BaseDatos,
    [Parameter(Mandatory = $true)]
    [string] $UsuarioAdmin,
    [string] $UsuarioApp = 'pc_app'
)

$ErrorActionPreference = 'Stop'

$claveAdmin = Read-Host 'Contraseña del administrador' -AsSecureString
$claveApp = Read-Host "Contraseña nueva para el usuario $UsuarioApp" -AsSecureString
$claveAdminTexto = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($claveAdmin))
$claveAppTexto = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($claveApp))

$cadena = "Server=tcp:$Servidor,1433;Database=$BaseDatos;User ID=$UsuarioAdmin;Password=$claveAdminTexto;Encrypt=True;TrustServerCertificate=False;Connect Timeout=60;"
$archivos = @('02_CrearTablas.sql', '04_CrearProcedimientos.sql', '05_CrearIndices.sql',
    '06_MigracionSincronizacionOffline.sql', '07_MigracionPlantillaPredeterminada.sql', '08_MigracionTareasDetenidas.sql')

function Ejecutar($conexion, $texto) {
    foreach ($lote in [regex]::Split($texto, '(?im)^\s*GO\s*$')) {
        if ([string]::IsNullOrWhiteSpace($lote)) { continue }
        $comando = $conexion.CreateCommand()
        try { $comando.CommandText = $lote; $comando.CommandTimeout = 120; [void]$comando.ExecuteNonQuery() }
        finally { $comando.Dispose() }
    }
}

$conexion = New-Object System.Data.SqlClient.SqlConnection $cadena
try {
    $conexion.Open()
    $existe = $conexion.CreateCommand()
    $existe.CommandText = "SELECT OBJECT_ID('dbo.Usuarios', 'U')"
    if ($existe.ExecuteScalar() -isnot [DBNull]) {
        throw 'La base ya tiene las tablas del programa. No se ejecutó nada.'
    }
    foreach ($archivo in $archivos) {
        $ruta = Join-Path $PSScriptRoot "../$archivo"
        $texto = Get-Content -LiteralPath $ruta -Raw -Encoding UTF8
        $texto = $texto -replace '(?im)^\s*USE\s+ProgramaConcentracion;\s*$', ''
        Ejecutar $conexion $texto
        Write-Host "Correcto: $archivo"
    }
    # Usuario del programa: solo puede usar los datos, no puede crear ni borrar objetos
    $claveSql = $claveAppTexto.Replace("'", "''")
    Ejecutar $conexion "CREATE USER [$UsuarioApp] WITH PASSWORD = '$claveSql';
GO
GRANT EXECUTE, SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO [$UsuarioApp];"
    Write-Host "Usuario $UsuarioApp creado con permisos limitados."
}
finally { $conexion.Dispose() }
