using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace CapaNegocio
{
    /// <summary>Fuerza a los navegadores conocidos a usar el proxy local.</summary>
    internal sealed class N_FiltroWebWfp : IDisposable
    {
        private static readonly Guid LayerV4 = new Guid("c38d57d1-05a7-4c33-904f-7fbceee60e82");
        private static readonly Guid LayerV6 = new Guid("4a72393b-319f-44bc-84c3-ba54dcb3b6b4");
        private static readonly Guid ConditionAppId = new Guid("d78e1e87-8644-4ea5-9437-d809ecefc971");
        private static readonly Guid ConditionRemoteAddress = new Guid("b235ae9a-1d64-49b8-a44c-5ff3d9095045");
        private static readonly Guid ConditionRemotePort = new Guid("c35a604d-d22b-4e1a-91b4-68f674ee674b");
        private static readonly Guid SublayerId = new Guid("0ddf71a4-6c50-4e45-a370-b3163cc546f7");

        private const uint SessionDynamic = 1;
        private const uint AuthnWinnt = 10;
        private const uint FwpUint8 = 1;
        private const uint FwpUint16 = 2;
        private const uint FwpUint32 = 3;
        private const uint FwpByteBlob = 12;
        private const uint MatchEqual = 0;
        private const uint ActionBlock = 0x00001001;
        private const uint ActionPermit = 0x00001002;
        private const uint LoopbackV4 = 0x7f000001;

        private readonly object _sync = new object();
        private readonly List<ulong> _filterIds = new List<ulong>();
        private IntPtr _engine;
        private bool _sublayerCreated;

        public void Actualizar(int puertoProxy)
        {
            if (puertoProxy < 1 || puertoProxy > UInt16.MaxValue)
                throw new ArgumentOutOfRangeException("puertoProxy");
            if (!EsAdministrador())
                throw new InvalidOperationException("Las reglas WFP requieren permisos de administrador.");

            lock (_sync)
            {
                bool sesionNueva = false;
                bool transaccion = false;
                var idsNuevos = new List<ulong>();
                try
                {
                    if (_engine == IntPtr.Zero)
                    {
                        AbrirSesion();
                        sesionNueva = true;
                    }
                    Comprobar(FwpmTransactionBegin0(_engine, 0), "FwpmTransactionBegin0");
                    transaccion = true;
                    foreach (ulong id in _filterIds)
                        Comprobar(FwpmFilterDeleteById0(_engine, id), "FwpmFilterDeleteById0");
                    if (_sublayerCreated)
                    {
                        Guid sublayer = SublayerId;
                        Comprobar(FwpmSubLayerDeleteByKey0(_engine, ref sublayer), "FwpmSubLayerDeleteByKey0");
                    }

                    List<byte[]> appIds = BuscarNavegadores().Select(ObtenerAppId).Where(id => id != null).ToList();
                    if (appIds.Count == 0)
                        throw new InvalidOperationException("No se encontraron navegadores compatibles para aplicar el filtro WFP.");

                    CrearSublayer();
                    foreach (byte[] appId in appIds)
                    {
                        AgregarFiltroBloqueo(LayerV4, appId, idsNuevos);
                        AgregarFiltroBloqueo(LayerV6, appId, idsNuevos);
                        AgregarFiltroProxy(LayerV4, appId, (ushort)puertoProxy, idsNuevos);
                    }

                    Comprobar(FwpmTransactionCommit0(_engine), "FwpmTransactionCommit0");
                    transaccion = false;
                    _filterIds.Clear();
                    _filterIds.AddRange(idsNuevos);
                    _sublayerCreated = true;
                }
                catch
                {
                    if (transaccion) FwpmTransactionAbort0(_engine);
                    if (sesionNueva) CerrarSesion();
                    throw;
                }
            }
        }

        public void Limpiar()
        {
            lock (_sync) CerrarSesion();
        }

        private void AbrirSesion()
        {
            IntPtr nombre = Marshal.StringToHGlobalUni("Concentracion - proxy local");
            var session = new Session
            {
                displayData = new DisplayData { name = nombre, description = IntPtr.Zero },
                flags = SessionDynamic,
                txnWaitTimeoutInMSec = 5000
            };
            int hr = FwpmEngineOpen0(null, AuthnWinnt, IntPtr.Zero, ref session, out _engine);
            Marshal.FreeHGlobal(nombre);
            Comprobar(hr, "FwpmEngineOpen0");
        }

        private void CrearSublayer()
        {
            IntPtr nombre = Marshal.StringToHGlobalUni("Concentracion");
            var sublayer = new Sublayer
            {
                subLayerKey = SublayerId,
                displayData = new DisplayData { name = nombre, description = IntPtr.Zero },
                weight = UInt16.MaxValue
            };
            int hr = FwpmSubLayerAdd0(_engine, ref sublayer, IntPtr.Zero);
            Marshal.FreeHGlobal(nombre);
            Comprobar(hr, "FwpmSubLayerAdd0");
        }

        private void AgregarFiltroBloqueo(Guid layer, byte[] appId, List<ulong> ids)
        {
            AgregarFiltro(layer, appId, null, ActionBlock, 1, ids);
        }

        private void AgregarFiltroProxy(Guid layer, byte[] appId, ushort port, List<ulong> ids)
        {
            const int count = 3;
            int size = Marshal.SizeOf(typeof(FilterCondition));
            IntPtr conditions = Marshal.AllocHGlobal(size * count);
            IntPtr appBytes = Marshal.AllocHGlobal(appId.Length);
            IntPtr blobPointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ByteBlob)));
            IntPtr name = Marshal.StringToHGlobalUni("Permitir proxy local");
            try
            {
                Marshal.Copy(appId, 0, appBytes, appId.Length);
                Marshal.StructureToPtr(new ByteBlob { size = (uint)appId.Length, data = appBytes }, blobPointer, false);
                Marshal.StructureToPtr(new FilterCondition
                {
                    fieldKey = ConditionAppId,
                    matchType = MatchEqual,
                    conditionValue = new ConditionValue { type = FwpByteBlob, value = new ConditionUnion { pointer = blobPointer } }
                }, conditions, false);
                Marshal.StructureToPtr(new FilterCondition
                {
                    fieldKey = ConditionRemoteAddress,
                    matchType = MatchEqual,
                    conditionValue = new ConditionValue { type = FwpUint32, value = new ConditionUnion { uint32 = LoopbackV4 } }
                }, IntPtr.Add(conditions, size), false);
                Marshal.StructureToPtr(new FilterCondition
                {
                    fieldKey = ConditionRemotePort,
                    matchType = MatchEqual,
                    conditionValue = new ConditionValue { type = FwpUint16, value = new ConditionUnion { uint16 = port } }
                }, IntPtr.Add(conditions, size * 2), false);

                Filter filter = CrearFiltro(layer, name, ActionPermit, 15, conditions, count);
                ulong id;
                Comprobar(FwpmFilterAdd0(_engine, ref filter, IntPtr.Zero, out id), "FwpmFilterAdd0 (proxy local)");
                ids.Add(id);
            }
            finally
            {
                for (int i = 0; i < count; i++)
                    Marshal.DestroyStructure(IntPtr.Add(conditions, size * i), typeof(FilterCondition));
                Marshal.FreeHGlobal(conditions);
                Marshal.FreeHGlobal(appBytes);
                Marshal.FreeHGlobal(blobPointer);
                Marshal.FreeHGlobal(name);
            }
        }

        private void AgregarFiltro(Guid layer, byte[] appId, string host, uint action, byte weight, List<ulong> ids)
        {
            IntPtr conditions = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(FilterCondition)));
            IntPtr appBytes = Marshal.AllocHGlobal(appId.Length);
            IntPtr blobPointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ByteBlob)));
            IntPtr name = Marshal.StringToHGlobalUni(host == null ? "Bloquear conexión directa" : "Permitir " + host);
            try
            {
                Marshal.Copy(appId, 0, appBytes, appId.Length);
                Marshal.StructureToPtr(new ByteBlob { size = (uint)appId.Length, data = appBytes }, blobPointer, false);
                Marshal.StructureToPtr(new FilterCondition
                {
                    fieldKey = ConditionAppId,
                    matchType = MatchEqual,
                    conditionValue = new ConditionValue { type = FwpByteBlob, value = new ConditionUnion { pointer = blobPointer } }
                }, conditions, false);
                Filter filter = CrearFiltro(layer, name, action, weight, conditions, 1);
                ulong id;
                Comprobar(FwpmFilterAdd0(_engine, ref filter, IntPtr.Zero, out id), "FwpmFilterAdd0 (bloqueo navegador)");
                ids.Add(id);
            }
            finally
            {
                Marshal.DestroyStructure(conditions, typeof(FilterCondition));
                Marshal.FreeHGlobal(conditions);
                Marshal.FreeHGlobal(appBytes);
                Marshal.FreeHGlobal(blobPointer);
                Marshal.FreeHGlobal(name);
            }
        }

        private Filter CrearFiltro(Guid layer, IntPtr name, uint action, byte weight, IntPtr conditions, int count)
        {
            return new Filter
            {
                displayData = new DisplayData { name = name, description = IntPtr.Zero },
                layerKey = layer,
                subLayerKey = SublayerId,
                weight = new Value { type = FwpUint8, value = new ValueUnion { uint8 = weight } },
                numFilterConditions = (uint)count,
                filterCondition = conditions,
                action = new FilterAction { type = action }
            };
        }

        private static byte[] ObtenerAppId(string path)
        {
            IntPtr pointer;
            int hr = FwpmGetAppIdFromFileName0(path, out pointer);
            if (hr != 0 || pointer == IntPtr.Zero)
            {
                Trace.TraceWarning("WFP no obtuvo la identidad de " + path + ": " + hr);
                return null;
            }
            try
            {
                ByteBlob blob = (ByteBlob)Marshal.PtrToStructure(pointer, typeof(ByteBlob));
                byte[] result = new byte[blob.size];
                Marshal.Copy(blob.data, result, 0, result.Length);
                return result;
            }
            finally { FwpmFreeMemory0(ref pointer); }
        }

        private static IEnumerable<string> BuscarNavegadores()
        {
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string pf86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string[][] candidates =
            {
                new[] { "Google", "Chrome", "Application", "chrome.exe" },
                new[] { "Microsoft", "Edge", "Application", "msedge.exe" },
                new[] { "BraveSoftware", "Brave-Browser", "Application", "brave.exe" },
                new[] { "Mozilla Firefox", "firefox.exe" },
                new[] { "Opera", "opera.exe" }, new[] { "Opera GX", "opera.exe" },
                new[] { "Vivaldi", "Application", "vivaldi.exe" },
                new[] { "Waterfox", "waterfox.exe" }, new[] { "LibreWolf", "librewolf.exe" },
                new[] { "Floorp", "floorp.exe" }, new[] { "Arc", "Arc.exe" }
            };
            foreach (string[] parts in candidates)
            {
                AddPath(paths, Combine(pf, parts));
                AddPath(paths, Combine(pf86, parts));
                AddPath(paths, Combine(local, parts));
                AddPath(paths, Combine(Path.Combine(local, "Programs"), parts));
            }
            foreach (string process in new[] { "chrome", "msedge", "brave", "firefox", "opera", "vivaldi", "waterfox", "librewolf", "floorp", "arc" })
            {
                foreach (Process running in Process.GetProcessesByName(process))
                {
                    using (running) { try { AddPath(paths, running.MainModule.FileName); } catch { } }
                }
            }
            return paths;
        }

        private static string Combine(string root, IEnumerable<string> parts) { return Path.Combine(new[] { root }.Concat(parts).ToArray()); }
        private static void AddPath(HashSet<string> paths, string path)
        {
            if (!String.IsNullOrWhiteSpace(path) && File.Exists(path)) paths.Add(Path.GetFullPath(path));
        }

        private void CerrarSesion()
        {
            if (_engine == IntPtr.Zero) return;
            int hr = FwpmEngineClose0(_engine);
            _engine = IntPtr.Zero;
            _filterIds.Clear();
            _sublayerCreated = false;
            if (hr != 0) Trace.TraceWarning("FwpmEngineClose0 devolvió " + hr);
        }

        private static bool EsAdministrador()
        {
            using (var identity = System.Security.Principal.WindowsIdentity.GetCurrent())
                return new System.Security.Principal.WindowsPrincipal(identity).IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }

        private static void Comprobar(int hr, string api)
        {
            if (hr != 0) throw new InvalidOperationException(api + " falló con el código " + hr + ".");
        }

        public void Dispose() { Limpiar(); }

        [StructLayout(LayoutKind.Sequential)] private struct DisplayData { public IntPtr name; public IntPtr description; }
        [StructLayout(LayoutKind.Sequential)] private struct Session { public Guid sessionKey; public DisplayData displayData; public uint flags; public uint txnWaitTimeoutInMSec; public ulong processId; public IntPtr sid; public IntPtr username; public int kernelMode; }
        [StructLayout(LayoutKind.Sequential)] private struct Sublayer { public Guid subLayerKey; public DisplayData displayData; public uint flags; public IntPtr providerKey; public ByteBlob providerData; public ushort weight; }
        [StructLayout(LayoutKind.Sequential)] private struct ByteBlob { public uint size; public IntPtr data; }
        [StructLayout(LayoutKind.Sequential)] private struct ConditionValue { public uint type; public ConditionUnion value; }
        [StructLayout(LayoutKind.Explicit)] private struct ConditionUnion { [FieldOffset(0)] public uint uint32; [FieldOffset(0)] public ushort uint16; [FieldOffset(0)] public IntPtr pointer; }
        [StructLayout(LayoutKind.Sequential)] private struct FilterCondition { public Guid fieldKey; public uint matchType; public ConditionValue conditionValue; }
        [StructLayout(LayoutKind.Explicit)] private struct ValueUnion { [FieldOffset(0)] public byte uint8; [FieldOffset(0)] public IntPtr pointer; }
        [StructLayout(LayoutKind.Sequential)] private struct Value { public uint type; public ValueUnion value; }
        [StructLayout(LayoutKind.Sequential)] private struct FilterAction { public uint type; public Guid calloutKey; }
        [StructLayout(LayoutKind.Sequential)] private struct Filter { public Guid filterKey; public DisplayData displayData; public uint flags; public IntPtr providerKey; public ByteBlob providerData; public Guid layerKey; public Guid subLayerKey; public Value weight; public uint numFilterConditions; public IntPtr filterCondition; public FilterAction action; public ulong rawContext; public IntPtr reserved; public ulong filterId; public Value effectiveWeight; }

        [DllImport("fwpuclnt.dll", CharSet = CharSet.Unicode, ExactSpelling = true)] private static extern int FwpmEngineOpen0(string serverName, uint authnService, IntPtr authIdentity, ref Session session, out IntPtr engineHandle);
        [DllImport("fwpuclnt.dll", ExactSpelling = true)] private static extern int FwpmEngineClose0(IntPtr engineHandle);
        [DllImport("fwpuclnt.dll", ExactSpelling = true)] private static extern int FwpmTransactionBegin0(IntPtr engineHandle, uint flags);
        [DllImport("fwpuclnt.dll", ExactSpelling = true)] private static extern int FwpmTransactionCommit0(IntPtr engineHandle);
        [DllImport("fwpuclnt.dll", ExactSpelling = true)] private static extern int FwpmTransactionAbort0(IntPtr engineHandle);
        [DllImport("fwpuclnt.dll", ExactSpelling = true)] private static extern int FwpmSubLayerAdd0(IntPtr engineHandle, ref Sublayer sublayer, IntPtr securityDescriptor);
        [DllImport("fwpuclnt.dll", ExactSpelling = true)] private static extern int FwpmSubLayerDeleteByKey0(IntPtr engineHandle, ref Guid sublayerKey);
        [DllImport("fwpuclnt.dll", ExactSpelling = true)] private static extern int FwpmFilterDeleteById0(IntPtr engineHandle, ulong id);
        [DllImport("fwpuclnt.dll", ExactSpelling = true)] private static extern int FwpmFilterAdd0(IntPtr engineHandle, ref Filter filter, IntPtr securityDescriptor, out ulong id);
        [DllImport("fwpuclnt.dll", CharSet = CharSet.Unicode, ExactSpelling = true)] private static extern int FwpmGetAppIdFromFileName0(string fileName, out IntPtr appId);
        [DllImport("fwpuclnt.dll", ExactSpelling = true)] private static extern void FwpmFreeMemory0(ref IntPtr pointer);
    }
}
