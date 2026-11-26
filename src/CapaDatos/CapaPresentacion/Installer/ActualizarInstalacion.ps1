param(
    [Parameter(Mandatory = $true)]
    [string]$Origen,
    [Parameter(Mandatory = $false)]
    [string]$Destino,
    [Parameter(Mandatory = $false)]
    [string]$Ejecutable,
    [switch]$Reiniciar
)

$ErrorActionPreference = 'Stop'
$identidad = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = New-Object Security.Principal.WindowsPrincipal($identidad)
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw 'El actualizador necesita permisos de administrador para reemplazar los archivos instalados.'
}

$raizOrigen = (Resolve-Path -LiteralPath $Origen).Path.TrimEnd('\')
if ([string]::IsNullOrWhiteSpace($Destino)) {
    $destino = Join-Path $env:ProgramFiles 'Programa de Concentración'
} else {
    $destino = [System.IO.Path]::GetFullPath($Destino)
}
if ([string]::IsNullOrWhiteSpace($Ejecutable)) {
    $Ejecutable = Join-Path $destino 'ProgramaConcentracion.exe'
} else {
    $Ejecutable = [System.IO.Path]::GetFullPath($Ejecutable)
}
if (-not (Test-Path -LiteralPath (Join-Path $destino 'ProgramaConcentracion.exe')) -and
    [string]::IsNullOrWhiteSpace($Destino)) { exit 0 }
New-Item -ItemType Directory -Path $destino -Force | Out-Null

$carpetaLog = Join-Path $env:LOCALAPPDATA 'Programa-de-Concentracion\Logs'
New-Item -ItemType Directory -Path $carpetaLog -Force | Out-Null
$rutaLog = Join-Path $carpetaLog 'actualizaciones.log'
[IO.File]::AppendAllText($rutaLog, ('[{0:O}] Actualización preparada en {1}; esperando que cierre la aplicación.{2}' -f [DateTime]::Now, $destino, [Environment]::NewLine), [Text.Encoding]::UTF8)

$mutex = New-Object System.Threading.Mutex($false, 'Local\ProgramaConcentracionActualizador')
if (-not $mutex.WaitOne(0)) { exit 0 }
try {
while (Get-Process -Name 'ProgramaConcentracion' -ErrorAction SilentlyContinue) {
    Start-Sleep -Seconds 2
}

$archivos = Get-ChildItem -LiteralPath $raizOrigen -File -Recurse | Where-Object {
    $_.Extension -notin '.pdb', '.xml' -and
    $_.Name -ne 'ProgramaConcentracion-Instalador.zip' -and
    $_.Name -notlike '~*.DDF'
}
foreach ($archivo in $archivos) {
    $relativa = $archivo.FullName.Substring($raizOrigen.Length).TrimStart('\')
    $rutaDestino = Join-Path $destino $relativa
    $carpetaDestino = Split-Path -Parent $rutaDestino
    New-Item -ItemType Directory -Path $carpetaDestino -Force | Out-Null
    $copiado = $false
    for ($intento = 1; $intento -le 5 -and -not $copiado; $intento++) {
        try {
            Copy-Item -LiteralPath $archivo.FullName -Destination $rutaDestino -Force
            $copiado = $true
        }
        catch {
            if ($intento -eq 5) { throw }
            Start-Sleep -Seconds 1
        }
    }
}

[IO.File]::AppendAllText($rutaLog, ('[{0:O}] Se actualizaron {1} archivos desde Release.{2}' -f [DateTime]::Now, $archivos.Count, [Environment]::NewLine), [Text.Encoding]::UTF8)
$accesoDirecto = Join-Path ([Environment]::GetFolderPath('CommonPrograms')) 'Programa de Concentración.lnk'
if (Test-Path -LiteralPath $accesoDirecto) {
    $shell = New-Object -ComObject WScript.Shell
    $atajo = $shell.CreateShortcut($accesoDirecto)
    $atajo.TargetPath = $Ejecutable
    $atajo.Arguments = '--mostrar'
    $atajo.WorkingDirectory = $destino
    $atajo.Save()
}
if ($Reiniciar -and (Test-Path -LiteralPath $Ejecutable)) {
    Start-Process -FilePath $Ejecutable -ArgumentList '--mostrar' -WorkingDirectory $destino
}
}
finally {
    $mutex.ReleaseMutex()
    $mutex.Dispose()
}
