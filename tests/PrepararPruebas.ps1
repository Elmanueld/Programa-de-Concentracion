$ErrorActionPreference = 'Stop'

# La instancia es exclusiva para las pruebas; nunca se reutiliza una existente.
$herramienta = Get-Command SqlLocalDB.exe -ErrorAction SilentlyContinue
if ($herramienta) { $localdb = $herramienta.Source }
else {
    $localdb = Get-ChildItem 'C:\Program Files\Microsoft SQL Server\*\Tools\Binn\SqlLocalDB.exe' -ErrorAction SilentlyContinue |
        Sort-Object FullName -Descending | Select-Object -First 1 -ExpandProperty FullName
}
if (-not $localdb) { throw 'Instala SQL Server Express LocalDB para ejecutar las pruebas de integración.' }
$instancias = & $localdb info
if ($LASTEXITCODE -ne 0) { throw 'No se pudieron consultar las instancias LocalDB.' }
if ($instancias -contains 'Entregable3Pruebas') {
    throw 'La instancia Entregable3Pruebas ya existe. No se modificó; prepara las pruebas en una instancia limpia.'
}
& $localdb create Entregable3Pruebas -s
if ($LASTEXITCODE -ne 0) { throw 'No se pudo crear la instancia exclusiva de pruebas.' }

$conexion = New-Object System.Data.SqlClient.SqlConnection 'Server=(localdb)\Entregable3Pruebas;Database=master;Integrated Security=True;'
try {
    $conexion.Open()
    $archivos = @('01_CrearBaseDeDatos.sql','02_CrearTablas.sql','04_CrearProcedimientos.sql','05_CrearIndices.sql',
        '06_MigracionSincronizacionOffline.sql','07_MigracionPlantillaPredeterminada.sql','08_MigracionTareasDetenidas.sql')
    foreach ($archivo in $archivos) {
        $ruta = Join-Path $PSScriptRoot "../database/$archivo"
        if (-not (Test-Path -LiteralPath $ruta -PathType Leaf)) { throw "No se encontró el script requerido: $ruta" }
        $texto = Get-Content -LiteralPath $ruta -Raw -Encoding UTF8
        if ($archivo -eq '01_CrearBaseDeDatos.sql') {
            $carpetaDatos = Join-Path $env:LOCALAPPDATA 'ProgramaConcentracion\Pruebas\Entregable3Pruebas'
            New-Item -ItemType Directory -Force -Path $carpetaDatos | Out-Null
            $archivoMdf = Join-Path $carpetaDatos 'ProgramaConcentracion.mdf'
            $archivoLdf = Join-Path $carpetaDatos 'ProgramaConcentracion_log.ldf'
            if ((Test-Path -LiteralPath $archivoMdf) -or (Test-Path -LiteralPath $archivoLdf)) {
                throw "Ya existen archivos de base de datos en $carpetaDatos; no se sobrescribieron."
            }
            $archivoMdfSql = $archivoMdf.Replace("'", "''")
            $archivoLdfSql = $archivoLdf.Replace("'", "''")
            $texto = "CREATE DATABASE ProgramaConcentracion ON PRIMARY (NAME=N'ProgramaConcentracion', FILENAME=N'$archivoMdfSql') LOG ON (NAME=N'ProgramaConcentracion_log', FILENAME=N'$archivoLdfSql');`nGO`nUSE ProgramaConcentracion;"
        }
        foreach ($lote in [regex]::Split($texto, '(?im)^\s*GO\s*$')) {
            if ([string]::IsNullOrWhiteSpace($lote)) { continue }
            $comando = $conexion.CreateCommand()
            try { $comando.CommandText = $lote; $comando.CommandTimeout = 60; [void]$comando.ExecuteNonQuery() }
            finally { $comando.Dispose() }
        }
        Write-Host "Correcto: $archivo"
    }
} finally { $conexion.Dispose() }
