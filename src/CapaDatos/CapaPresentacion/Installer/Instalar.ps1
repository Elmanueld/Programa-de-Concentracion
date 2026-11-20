$ErrorActionPreference = 'Stop'

function Mostrar-Mensaje([string]$texto, [string]$titulo, [int]$tipo) {
    $shell = New-Object -ComObject WScript.Shell
    $null = $shell.Popup($texto, 0, $titulo, $tipo)
}

function Obtener-VersionFrameworkRelease {
    foreach ($vista in @([Microsoft.Win32.RegistryView]::Registry64, [Microsoft.Win32.RegistryView]::Registry32)) {
        $base = $null
        $clave = $null
        try {
            $base = [Microsoft.Win32.RegistryKey]::OpenBaseKey(
                [Microsoft.Win32.RegistryHive]::LocalMachine, $vista)
            $clave = $base.OpenSubKey('SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full')
            if ($clave -and $clave.GetValue('Release')) {
                return [int]$clave.GetValue('Release')
            }
        }
        catch { }
        finally {
            if ($clave) { $clave.Dispose() }
            if ($base) { $base.Dispose() }
        }
    }
    return 0
}

function Esperar-CierrePrograma {
    while ($true) {
        $procesos = @(Get-Process -Name 'ProgramaConcentracion' -ErrorAction SilentlyContinue)
        if ($procesos.Count -eq 0) { return }

        $respuesta = (New-Object -ComObject WScript.Shell).Popup(
            'El Programa de Concentración sigue abierto (posiblemente minimizado en la bandeja). Guarda lo que estés haciendo, elige Salir en el menú del icono de la bandeja y pulsa Reintentar. Pulsa Cancelar para detener la instalación.',
            0, 'Cierra el programa para continuar', 5 + 48)

        if ($respuesta -eq 2) { throw 'Instalación cancelada porque el programa sigue abierto.' }
    }
}

$instaladorFramework = $null
$temporal = Join-Path $env:TEMP ('ProgramaConcentracion-' + [Guid]::NewGuid().ToString('N'))

try {
    $identidad = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($identidad)
    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        $argumentos = '-NoLogo -NoProfile -ExecutionPolicy Bypass -File "' + $PSCommandPath + '"'
        $procesoElevado = Start-Process -FilePath (Join-Path $PSHOME 'powershell.exe') -Verb RunAs -WindowStyle Hidden -ArgumentList $argumentos -Wait -PassThru
        exit $procesoElevado.ExitCode
    }

    Esperar-CierrePrograma

    $release = Obtener-VersionFrameworkRelease

    if ($release -lt 528040) {
        Mostrar-Mensaje 'Se necesita .NET Framework 4.8. Se descargará desde Microsoft; Windows puede pedir autorización para instalarlo.' 'Programa de Concentración' 64
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
        $instaladorFramework = Join-Path $env:TEMP 'NDP48-x86-x64-AllOS-ENU.exe'
        Invoke-WebRequest -Uri 'https://download.microsoft.com/download/f/3/a/f3a6af84-da23-40a5-8d1c-49cc10c8e76f/NDP48-x86-x64-AllOS-ENU.exe' `
            -OutFile $instaladorFramework -UseBasicParsing -TimeoutSec 300
        $firma = Get-AuthenticodeSignature -LiteralPath $instaladorFramework
        if ($firma.Status -ne 'Valid' -or $firma.SignerCertificate.Subject -notmatch 'Microsoft') {
            throw 'No se pudo validar el instalador de .NET Framework descargado desde Microsoft.'
        }
        $proceso = Start-Process -FilePath $instaladorFramework -ArgumentList '/passive /norestart' -Verb RunAs -Wait -PassThru
        if ($proceso.ExitCode -eq 3010) {
            Mostrar-Mensaje 'Windows instaló .NET Framework 4.8 y necesita reiniciarse. Reinicia el equipo y vuelve a ejecutar este instalador.' 'Reinicio necesario' 48
            exit 3010
        }
        if ($proceso.ExitCode -ne 0) {
            throw "El instalador de .NET Framework 4.8 terminó con el código $($proceso.ExitCode)."
        }
    }

    $archivoPayload = Join-Path $PSScriptRoot 'ProgramaConcentracionPayload.zip'
    if (-not (Test-Path -LiteralPath $archivoPayload)) { throw 'No se encontraron los archivos de instalación.' }

    New-Item -ItemType Directory -Path $temporal -Force | Out-Null
    Expand-Archive -LiteralPath $archivoPayload -DestinationPath $temporal -Force

    $destino = Join-Path $env:ProgramFiles 'Programa de Concentración'
    New-Item -ItemType Directory -Path $destino -Force | Out-Null
    Copy-Item -Path (Join-Path $temporal '*') -Destination $destino -Recurse -Force

    $accesoDirecto = Join-Path ([Environment]::GetFolderPath('CommonPrograms')) 'Programa de Concentración.lnk'
    $wsh = New-Object -ComObject WScript.Shell
    $atajo = $wsh.CreateShortcut($accesoDirecto)
    $atajo.TargetPath = Join-Path $destino 'ProgramaConcentracion.exe'
    $atajo.WorkingDirectory = $destino
    $atajo.Description = 'Programa de Concentración'
    $atajo.Save()

    Mostrar-Mensaje 'La instalación terminó. Encontrarás el programa en el menú Inicio.' 'Instalación completada' 64
    Start-Process -FilePath $atajo.TargetPath -WorkingDirectory $destino
}
catch {
    $detalle = $_.Exception.Message
    $rutaLog = Join-Path $env:LOCALAPPDATA 'Programa-de-Concentracion\Logs\instalacion.log'
    try {
        New-Item -ItemType Directory -Path (Split-Path -Parent $rutaLog) -Force | Out-Null
        $entrada = '[{0:O}] {1}{2}' -f [DateTime]::Now, $detalle, [Environment]::NewLine
        [IO.File]::AppendAllText($rutaLog, $entrada, [Text.Encoding]::UTF8)
    }
    catch { $rutaLog = '(no se pudo escribir el log)' }
    Mostrar-Mensaje ("No se pudo completar la instalación.`r`n`r`nDetalle: $detalle`r`n`r`nRegistro: $rutaLog") 'Error de instalación' 16
    exit 1
}
finally {
    if (Test-Path -LiteralPath $temporal) { Remove-Item -LiteralPath $temporal -Recurse -Force -ErrorAction SilentlyContinue }
    if ($instaladorFramework -and (Test-Path -LiteralPath $instaladorFramework)) { Remove-Item -LiteralPath $instaladorFramework -Force -ErrorAction SilentlyContinue }
}
