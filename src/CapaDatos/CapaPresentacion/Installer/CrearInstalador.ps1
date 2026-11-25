$ErrorActionPreference = 'Stop'

$raizPresentacion = Split-Path -Parent $PSScriptRoot
$salidaRelease = Join-Path $raizPresentacion 'bin\Release'
$raizRepositorio = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..\..')).Path
$directorioPortable = Join-Path $raizRepositorio 'ProgramaConcentracion'
$salidaPortable = Join-Path $raizRepositorio 'ProgramaConcentracion-Portatil.zip'
$salidaInstalador = Join-Path $raizRepositorio 'ProgramaConcentracion-Instalador.zip'
$directorioTemporal = Join-Path $PSScriptRoot ('.build-' + [Guid]::NewGuid().ToString('N'))
$creado = $false

if (-not (Test-Path (Join-Path $salidaRelease 'ProgramaConcentracion.exe'))) {
    throw 'Primero compila CapaPresentacion en configuración Release.'
}

New-Item -ItemType Directory -Path $directorioTemporal -Force | Out-Null
try {
    $payload = Join-Path $directorioTemporal 'ProgramaConcentracionPayload.zip'
    $contenidoPayload = Join-Path $directorioTemporal 'payload'
    $contenidoPortable = Join-Path $directorioTemporal 'ProgramaConcentracion'
    New-Item -ItemType Directory -Path $contenidoPayload -Force | Out-Null
    New-Item -ItemType Directory -Path $contenidoPortable -Force | Out-Null
    $archivosAplicacion = Get-ChildItem -LiteralPath $salidaRelease -File -Recurse | Where-Object {
        $_.FullName -notmatch '\\app\.publish\\' -and
        $_.Name -ne 'ProgramaConcentracion-Instalador.zip' -and
        $_.Name -notlike '~*.DDF' -and
        $_.Extension -notin '.pdb', '.xml' -and
        $_.Name -notlike '*.vshost.*'
    }
    if (-not ($archivosAplicacion | Where-Object Name -eq 'ProgramaConcentracion.exe')) {
        throw 'No se encontró el ejecutable compilado.'
    }
    $raizArchivos = (Resolve-Path $salidaRelease).Path.TrimEnd('\') + '\'
    foreach ($archivo in $archivosAplicacion) {
        $rutaRelativa = $archivo.FullName.Substring($raizArchivos.Length)
        $rutaDestino = Join-Path $contenidoPayload $rutaRelativa
        $directorioDestino = Split-Path -Parent $rutaDestino
        New-Item -ItemType Directory -Path $directorioDestino -Force | Out-Null
        Copy-Item -LiteralPath $archivo.FullName -Destination $rutaDestino -Force

        $rutaPortable = Join-Path $contenidoPortable $rutaRelativa
        $directorioDestinoPortable = Split-Path -Parent $rutaPortable
        New-Item -ItemType Directory -Path $directorioDestinoPortable -Force | Out-Null
        Copy-Item -LiteralPath $archivo.FullName -Destination $rutaPortable -Force
    }
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'ActualizarInstalacion.ps1') `
        -Destination (Join-Path $contenidoPortable 'ActualizarInstalacion.ps1') -Force
    Set-Content -LiteralPath (Join-Path $contenidoPortable 'LEEME.txt') -Encoding UTF8 -Value @(
        'Programa de Concentración - versión portátil',
        '',
        'Mantén juntos todos los archivos de esta carpeta.',
        'Abre ProgramaConcentracion.exe para iniciar la aplicación.',
        'Requiere Windows y .NET Framework 4.8.'
    )
    Compress-Archive -Path (Join-Path $contenidoPayload '*') -DestinationPath $payload -Force
    Remove-Item -LiteralPath $contenidoPayload -Recurse -Force

    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Instalar.ps1') -Destination (Join-Path $directorioTemporal 'Instalar.ps1')
    Set-Content -LiteralPath (Join-Path $directorioTemporal 'Instalar.vbs') -Encoding ASCII -Value @(
        'Set shell = CreateObject("WScript.Shell")',
        'Set fso = CreateObject("Scripting.FileSystemObject")',
        'scriptPath = fso.BuildPath(fso.GetParentFolderName(WScript.ScriptFullName), "Instalar.ps1")',
        'command = "powershell.exe -NoLogo -NoProfile -WindowStyle Hidden -ExecutionPolicy Bypass -File """ & scriptPath & """"',
        'shell.Run command, 0, True'
    )
    Set-Content -LiteralPath (Join-Path $directorioTemporal 'LEEME.txt') -Encoding UTF8 -Value @(
        'Programa de Concentración - Instalador',
        '',
        'Descomprime el ZIP completo y ejecuta Instalar.vbs. La consola del instalador se ejecuta oculta.',
        'El instalador descarga .NET Framework 4.8 desde Microsoft si hace falta.',
        'Al finalizar, el programa aparece en el menú Inicio. Windows puede pedir autorización al abrirlo.'
    )
    Compress-Archive -Path (Join-Path $directorioTemporal '*') -DestinationPath $salidaInstalador -Force
    Compress-Archive -Path $contenidoPortable -DestinationPath $salidaPortable -Force
    if (-not (Test-Path -LiteralPath $salidaInstalador)) {
        throw 'No se pudo generar el paquete de instalación.'
    }
    if (-not (Test-Path -LiteralPath $salidaPortable)) {
        throw 'No se pudo generar el paquete portátil.'
    }

    $validador = Join-Path $PSScriptRoot 'ValidarInstalador.ps1'
    & $validador -Zip $salidaInstalador -PortableZip $salidaPortable `
        -Executable (Join-Path $salidaRelease 'ProgramaConcentracion.exe')

    if (Test-Path -LiteralPath $directorioPortable) {
        Remove-Item -LiteralPath $directorioPortable -Recurse -Force
    }
    Move-Item -LiteralPath $contenidoPortable -Destination $directorioPortable

    $destinoInstalado = Join-Path $env:ProgramFiles 'Programa de Concentración'
    if ($env:PC_SOLO_GENERAR_PAQUETE -ne '1' -and
        (Test-Path -LiteralPath (Join-Path $destinoInstalado 'ProgramaConcentracion.exe'))) {
        $actualizador = Join-Path $PSScriptRoot 'ActualizarInstalacion.ps1'
        if (-not (Test-Path -LiteralPath $actualizador)) {
            throw 'No se encontró el actualizador de la instalación existente.'
        }
        $argumentosActualizador = '-NoLogo -NoProfile -WindowStyle Hidden -ExecutionPolicy Bypass -File "' +
            $actualizador + '" -Origen "' + $salidaRelease + '"'
        $identidadActual = [Security.Principal.WindowsIdentity]::GetCurrent()
        $principalActual = New-Object Security.Principal.WindowsPrincipal($identidadActual)
        $opcionesProceso = @{
            FilePath = (Join-Path $PSHOME 'powershell.exe')
            ArgumentList = $argumentosActualizador + ' -Reiniciar'
            WindowStyle = 'Hidden'
        }
        if (-not $principalActual.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
            $opcionesProceso.Verb = 'RunAs'
        }
        Start-Process @opcionesProceso | Out-Null
    }

    $creado = $true
    Get-Item -LiteralPath $salidaInstalador | Select-Object FullName, Length
}
finally {
    if ($creado -and (Test-Path -LiteralPath $directorioTemporal)) {
        Remove-Item -LiteralPath $directorioTemporal -Recurse -Force -ErrorAction SilentlyContinue
    } elseif (Test-Path -LiteralPath $directorioTemporal) {
        Write-Host "Se conservaron archivos temporales para diagnóstico: $directorioTemporal"
    }
}
