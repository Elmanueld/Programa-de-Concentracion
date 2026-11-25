param(
    [Parameter(Mandatory = $true)]
    [string]$Zip,
    [Parameter(Mandatory = $false)]
    [string]$PortableZip,
    [Parameter(Mandatory = $false)]
    [string]$InstallerExe,
    [Parameter(Mandatory = $true)]
    [string]$Executable
)

$ErrorActionPreference = 'Stop'
function Get-Sha256Hex([string]$Path) {
    $sha = [Security.Cryptography.SHA256]::Create()
    $stream = [IO.File]::OpenRead($Path)
    try { return [BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-', '') }
    finally { $stream.Dispose(); $sha.Dispose() }
}
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

if ($InstallerExe) {
    $archivoInstaladorExe = (Resolve-Path -LiteralPath $InstallerExe).Path
    $bytesInstalador = [IO.File]::ReadAllBytes($archivoInstaladorExe)
    if ($bytesInstalador.Length -lt 65536 -or
        [Text.Encoding]::ASCII.GetString($bytesInstalador, 0, 2) -ne 'MZ') {
        throw 'El instalador directo no es un paquete autoextraíble de Windows válido.'
    }
    $firmaCab = [Text.Encoding]::ASCII.GetBytes('MSCF')
    $offsetCab = -1
    for ($indice = 0; $indice -le $bytesInstalador.Length - $firmaCab.Length; $indice++) {
        if ($bytesInstalador[$indice] -eq $firmaCab[0] -and
            $bytesInstalador[$indice + 1] -eq $firmaCab[1] -and
            $bytesInstalador[$indice + 2] -eq $firmaCab[2] -and
            $bytesInstalador[$indice + 3] -eq $firmaCab[3]) {
            $offsetCab = $indice
            break
        }
    }
    if ($offsetCab -lt 0) { throw 'El instalador directo no contiene su paquete de instalación.' }
    $longitudCab = [BitConverter]::ToUInt32($bytesInstalador, $offsetCab + 8)
    if ($longitudCab -lt 1 -or $longitudCab -gt ($bytesInstalador.Length - $offsetCab)) {
        throw 'El paquete incluido en el instalador directo está incompleto.'
    }
    $directorioValidacion = Join-Path $env:TEMP ('pc-setup-validation-' + [Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $directorioValidacion -Force | Out-Null
    try {
        $rutaCab = Join-Path $directorioValidacion 'paquete.cab'
        [IO.File]::WriteAllBytes($rutaCab,
            [byte[]]$bytesInstalador[$offsetCab..($offsetCab + $longitudCab - 1)])
        $expand = Join-Path $env:WINDIR 'System32\expand.exe'
        if (-not (Test-Path -LiteralPath $expand)) { throw 'No se encontró la herramienta de Windows para validar el instalador.' }
        $archivosIncluidos = (& $expand -D $rutaCab 2>&1 | Out-String)
        if ($LASTEXITCODE -ne 0) { throw 'Windows no pudo leer los archivos del instalador directo.' }
        foreach ($nombre in @('ProgramaConcentracionPayload.zip', 'Instalar.ps1', 'Instalar.vbs', 'LEEME.txt')) {
            if ($archivosIncluidos -notmatch ('(?m):\s*' + [Regex]::Escape($nombre) + '\s*$')) {
                throw "El instalador directo no incluye el archivo necesario $nombre."
            }
        }
    }
    finally {
        if (Test-Path -LiteralPath $directorioValidacion) {
            Remove-Item -LiteralPath $directorioValidacion -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
    Write-Host "Instalador directo verificado: $([IO.Path]::GetFileName($archivoInstaladorExe))"
}

$archivoZip = (Resolve-Path -LiteralPath $Zip).Path
$archivoExe = (Resolve-Path -LiteralPath $Executable).Path
$hashEsperado = Get-Sha256Hex $archivoExe
$paquete = [IO.Compression.ZipFile]::OpenRead($archivoZip)
$memoriaPayload = $null
$contenidoPayload = $null
try {
    $entradasEsperadas = @(
        'ProgramaConcentracionPayload.zip',
        'Instalar.ps1',
        'Instalar.vbs',
        'LEEME.txt'
    )
    $nombresEntradas = @($paquete.Entries | ForEach-Object FullName)
    if ($nombresEntradas.Count -ne $entradasEsperadas.Count -or
        @($nombresEntradas | Where-Object { $_ -notin $entradasEsperadas }).Count -gt 0) {
        throw 'El instalador ZIP debe contener solo los archivos necesarios para instalar la aplicación.'
    }
    $entradaPayload = $paquete.GetEntry('ProgramaConcentracionPayload.zip')
    if (-not $entradaPayload) { throw 'El instalador no contiene ProgramaConcentracionPayload.zip.' }

    $memoriaPayload = New-Object IO.MemoryStream
    $flujoPayload = $entradaPayload.Open()
    try { $flujoPayload.CopyTo($memoriaPayload) }
    finally { $flujoPayload.Dispose() }

    $memoriaPayload.Position = 0
    $contenidoPayload = New-Object IO.Compression.ZipArchive($memoriaPayload, [IO.Compression.ZipArchiveMode]::Read, $true)
    $entradaExe = $contenidoPayload.GetEntry('ProgramaConcentracion.exe')
    if (-not $entradaExe) { throw 'El payload no contiene ProgramaConcentracion.exe.' }

    $flujoExe = $entradaExe.Open()
    $sha = [Security.Cryptography.SHA256]::Create()
    try { $hashReal = [BitConverter]::ToString($sha.ComputeHash($flujoExe)).Replace('-', '') }
    finally { $flujoExe.Dispose(); $sha.Dispose() }

    if ($hashReal -ne $hashEsperado) {
        throw "El EXE del instalador no coincide con el Release. Instalador: $hashReal; Release: $hashEsperado."
    }
    Write-Host "Instalador verificado: ProgramaConcentracion.exe $hashReal"

    if ($PortableZip) {
        $archivoPortable = (Resolve-Path -LiteralPath $PortableZip).Path
        $paquetePortable = [IO.Compression.ZipFile]::OpenRead($archivoPortable)
        try {
            $entradaExePortable = $paquetePortable.GetEntry('ProgramaConcentracion/ProgramaConcentracion.exe')
            if (-not $entradaExePortable) { throw 'El ZIP portátil no contiene ProgramaConcentracion/ProgramaConcentracion.exe.' }
            $flujoExePortable = $entradaExePortable.Open()
            $shaPortable = [Security.Cryptography.SHA256]::Create()
            try { $hashPortable = [BitConverter]::ToString($shaPortable.ComputeHash($flujoExePortable)).Replace('-', '') }
            finally { $flujoExePortable.Dispose(); $shaPortable.Dispose() }
            if ($hashPortable -ne $hashEsperado) {
                throw "El EXE portátil no coincide con Release. Portátil: $hashPortable; Release: $hashEsperado."
            }
            Write-Host "ZIP portátil verificado: ProgramaConcentracion.exe $hashPortable"
        }
        finally { $paquetePortable.Dispose() }
    }
}
finally {
    if ($contenidoPayload) { $contenidoPayload.Dispose() }
    if ($memoriaPayload) { $memoriaPayload.Dispose() }
    $paquete.Dispose()
}
