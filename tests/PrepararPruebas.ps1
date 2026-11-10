$ErrorActionPreference = 'Stop'

# Esta instancia es exclusiva para las pruebas. Nunca se elimina ni se reutiliza
# automáticamente una instancia existente.
$herramienta = Get-Command SqlLocalDB.exe -ErrorAction SilentlyContinue
if ($herramienta) {
    $localdb = $herramienta.Source
}
else {
    $candidatos = Get-ChildItem 'C:\Program Files\Microsoft SQL Server\*\Tools\Binn\SqlLocalDB.exe' -ErrorAction SilentlyContinue |
        Sort-Object FullName -Descending
    $localdb = $candidatos | Select-Object -First 1 -ExpandProperty FullName
}

if (-not $localdb) {
    throw 'Instala SQL Server Express LocalDB para ejecutar las pruebas de integración.'
}

$instancias = & $localdb info
if ($LASTEXITCODE -ne 0) {
    throw 'No se pudieron consultar las instancias LocalDB.'
}
if ($instancias -contains 'Entregable3Pruebas') {
    throw 'La instancia Entregable3Pruebas ya existe. No se modificó; usa una instancia limpia para preparar las pruebas.'
}

& $localdb create Entregable3Pruebas -s
if ($LASTEXITCODE -ne 0) {
    throw 'No se pudo crear la instancia exclusiva de pruebas.'
}

$conexion = New-Object System.Data.SqlClient.SqlConnection 'Server=(localdb)\Entregable3Pruebas;Database=master;Integrated Security=True;'
try {
    $conexion.Open()
    $archivos = @(
        '01_CrearBaseDeDatos.sql'
        '02_CrearTablas.sql'
        '03_CrearRelaciones.sql'
        '04_CrearProcedimientos.sql'
        '05_CrearIndices.sql'
    )

    foreach ($archivo in $archivos) {
        $ruta = Join-Path $PSScriptRoot "../database/$archivo"
        if (-not (Test-Path -LiteralPath $ruta -PathType Leaf)) {
            throw "No se encontró el script requerido: $ruta"
        }

        $texto = Get-Content -LiteralPath $ruta -Raw -Encoding UTF8
        foreach ($lote in [regex]::Split($texto, '(?im)^\s*GO\s*$')) {
            if ([string]::IsNullOrWhiteSpace($lote)) { continue }

            $comando = $conexion.CreateCommand()
            try {
                $comando.CommandText = $lote
                $comando.CommandTimeout = 60
                [void]$comando.ExecuteNonQuery()
            }
            finally {
                $comando.Dispose()
            }
        }
        Write-Host "Correcto: $archivo"
    }
}
finally {
    $conexion.Dispose()
}
