param(
    [Parameter(Mandatory = $true)]
    [string] $Servidor
)

$ErrorActionPreference = 'Stop'

$carpetaScripts = $PSScriptRoot
$orden = @(
    '01_CrearBaseDeDatos.sql'
    '02_CrearTablas.sql'
    '03_CrearRelaciones.sql'
    '04_CrearProcedimientos.sql'
    '05_CrearIndices.sql'
)

$sqlcmd = Get-Command 'sqlcmd' -ErrorAction SilentlyContinue
if (-not $sqlcmd) {
    throw 'No se encontró sqlcmd. Instala las herramientas de línea de comandos de SQL Server y vuelve a intentarlo.'
}

foreach ($archivo in $orden) {
    $ruta = Join-Path $carpetaScripts $archivo
    if (-not (Test-Path -LiteralPath $ruta -PathType Leaf)) {
        throw "No se encontró el script requerido: $ruta"
    }

    Write-Host "Ejecutando $archivo ..."
    & $sqlcmd.Source -S $Servidor -E -b -i $ruta

    if ($LASTEXITCODE -ne 0) {
        throw "Falló $archivo (código $LASTEXITCODE). Se detuvo la instalación."
    }
}

Write-Host 'Base de datos ProgramaConcentracion instalada correctamente.'
