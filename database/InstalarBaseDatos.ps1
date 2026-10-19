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

# Estos scripts crean objetos desde cero y no se pueden volver a ejecutar sobre
# una base existente. Comprueba el estado antes de ejecutar el primer archivo.
& $sqlcmd.Source -S $Servidor -E -b -d master -Q "IF DB_ID(N'ProgramaConcentracion') IS NOT NULL RAISERROR (N'La base de datos ProgramaConcentracion ya existe. No se ejecutaron los scripts. Verifica su estado antes de continuar.', 16, 1);"

if ($LASTEXITCODE -ne 0) {
    throw 'No se pudo validar la base de datos o ya existe. No se inició la instalación.'
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
