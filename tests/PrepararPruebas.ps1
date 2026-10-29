param([switch]$Original)
$ErrorActionPreference = 'Stop'
# Usamos una instancia separada para no tocar los datos del proyecto.
$herramienta = Get-Command SqlLocalDB.exe -ErrorAction SilentlyContinue
if ($herramienta) { $localdb = $herramienta.Source }
else {
    $localdb = Get-ChildItem 'C:\Program Files\Microsoft SQL Server\*\Tools\Binn\SqlLocalDB.exe' | Sort-Object FullName -Descending | Select-Object -First 1 -ExpandProperty FullName
}
if (-not $localdb) { throw 'Instale SQL Server Express LocalDB para ejecutar las pruebas de integración.' }
$instancias = & $localdb info
if ($LASTEXITCODE -ne 0) { throw 'No se pudieron consultar las instancias LocalDB.' }
if ($instancias -contains 'Entregable3Pruebas') { throw 'La instancia Entregable3Pruebas ya existe. Conserve sus datos; use una máquina de pruebas limpia.' }
& $localdb create Entregable3Pruebas -s
if ($LASTEXITCODE -ne 0) { throw 'No se pudo crear la instancia exclusiva.' }
$cn = New-Object System.Data.SqlClient.SqlConnection 'Server=(localdb)\Entregable3Pruebas;Database=master;Integrated Security=True;'
try {
    $cn.Open()
    $archivos = @('01_CrearBaseDeDatos.sql','02_CrearTablas.sql','03_CrearRelaciones.sql','04_CrearProcedimientos.sql','05_CrearIndices.sql')
    if (-not $Original) { $archivos += '07_VerificacionCorreo.sql' }
    foreach ($archivo in $archivos) {
        $texto = Get-Content (Join-Path $PSScriptRoot "../database/$archivo") -Raw -Encoding UTF8
        if ($Original) {
            $texto = (& git show "6154e9751a534797313a17e6d71d38260cae63ad:database/$archivo") -join "`n"
            if ($LASTEXITCODE -ne 0) { throw "No se pudo leer el script original $archivo." }
        }
        foreach ($lote in [regex]::Split($texto, '(?im)^\s*GO\s*$')) {
            if ([string]::IsNullOrWhiteSpace($lote)) { continue }
            $cmd = $cn.CreateCommand()
            try { $cmd.CommandText = $lote; $cmd.CommandTimeout = 60; [void]$cmd.ExecuteNonQuery() }
            finally { $cmd.Dispose() }
        }
        Write-Host "Correcto: $archivo"
    }
} finally { $cn.Dispose() }
