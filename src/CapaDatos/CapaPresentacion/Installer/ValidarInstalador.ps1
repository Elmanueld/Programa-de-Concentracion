param(
    [Parameter(Mandatory = $true)]
    [string]$Zip,
    [Parameter(Mandatory = $true)]
    [string]$Executable
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

$archivoZip = (Resolve-Path -LiteralPath $Zip).Path
$archivoExe = (Resolve-Path -LiteralPath $Executable).Path
$hashEsperado = (Get-FileHash -LiteralPath $archivoExe -Algorithm SHA256).Hash
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
}
finally {
    if ($contenidoPayload) { $contenidoPayload.Dispose() }
    if ($memoriaPayload) { $memoriaPayload.Dispose() }
    $paquete.Dispose()
}