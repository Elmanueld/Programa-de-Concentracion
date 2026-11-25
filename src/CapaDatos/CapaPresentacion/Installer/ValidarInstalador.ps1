param(
    [Parameter(Mandatory = $true)]
    [string]$Zip,
    [Parameter(Mandatory = $false)]
    [string]$PortableZip,
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

$archivoZip = (Resolve-Path -LiteralPath $Zip).Path
$archivoExe = (Resolve-Path -LiteralPath $Executable).Path
$hashEsperado = Get-Sha256Hex $archivoExe
$paquete = [IO.Compression.ZipFile]::OpenRead($archivoZip)
$memoriaPayload = $null
$contenidoPayload = $null
try {
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
