$ErrorActionPreference = 'Stop'

function Mostrar-Mensaje([string]$texto, [string]$titulo, [int]$tipo) {
    $shell = New-Object -ComObject WScript.Shell
    $null = $shell.Popup($texto, 0, $titulo, $tipo)
}

$instaladorFramework = $null
$temporal = Join-Path $env:TEMP ('ProgramaConcentracion-' + [Guid]::NewGuid().ToString('N'))

try {
    $identidad = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($identidad)
    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        $argumentos = '-NoLogo -NoProfile -ExecutionPolicy Bypass -File "' + $PSCommandPath + '"'
        $procesoElevado = Start-Process -FilePath (Join-Path $PSHOME 'powershell.exe') -Verb RunAs -ArgumentList $argumentos -Wait -PassThru
        exit $procesoElevado.ExitCode
    }

    $release = 0
    $clave = Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full' -ErrorAction SilentlyContinue
    if ($clave -and $clave.Release) { $release = [int]$clave.Release }

    if ($release -lt 528040) {
        Mostrar-Mensaje 'Se necesita .NET Framework 4.8. Se descargará desde Microsoft; Windows puede pedir autorización para instalarlo.' 'Programa de Concentración' 64
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
        $instaladorFramework = Join-Path $env:TEMP 'ndp48-web.exe'
        Invoke-WebRequest -Uri 'https://go.microsoft.com/fwlink/?LinkId=2085155' -OutFile $instaladorFramework -UseBasicParsing
        $firma = Get-AuthenticodeSignature -LiteralPath $instaladorFramework
        if ($firma.Status -ne 'Valid' -or $firma.SignerCertificate.Subject -notmatch 'Microsoft') {
            throw 'No se pudo validar el instalador de .NET Framework descargado desde Microsoft.'
        }
        $proceso = Start-Process -FilePath $instaladorFramework -ArgumentList '/passive /norestart' -Verb RunAs -Wait -PassThru
        if ($proceso.ExitCode -eq 3010) {
            Mostrar-Mensaje 'Windows instaló .NET Framework 4.8 y necesita reiniciarse. Reinicia el equipo y vuelve a ejecutar este instalador.' 'Reinicio necesario' 48
            exit 3010
        }
        if ($proceso.ExitCode -ne 0) { throw 'No se pudo instalar .NET Framework 4.8.' }
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
    Mostrar-Mensaje 'No se pudo completar la instalación. Comprueba tu conexión a Internet y que tengas permisos para instalar componentes de Windows; luego vuelve a intentarlo.' 'Error de instalación' 16
    exit 1
}
finally {
    if (Test-Path -LiteralPath $temporal) { Remove-Item -LiteralPath $temporal -Recurse -Force -ErrorAction SilentlyContinue }
    if ($instaladorFramework -and (Test-Path -LiteralPath $instaladorFramework)) { Remove-Item -LiteralPath $instaladorFramework -Force -ErrorAction SilentlyContinue }
}
