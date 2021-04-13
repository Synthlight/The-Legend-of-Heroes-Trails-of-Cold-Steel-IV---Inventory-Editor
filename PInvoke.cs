using System;
using System.Runtime.InteropServices;

namespace Inventory_Editor {
    public static class PInvoke {
        private const long INVALID_HANDLE_VALUE = -1;

        [Flags]
        private enum SnapshotFlags : uint {
            HeapList = 0x00000001,
            Process  = 0x00000002,
            Thread   = 0x00000004,
            Module   = 0x00000008,
            Module32 = 0x00000010,
            Inherit  = 0x80000000,
            All      = 0x0000001F,
            NoHeaps  = 0x40000000
        }

        [StructLayout(LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Ansi)]
        public struct MODULEENTRY32 {
            internal uint   dwSize;
            internal uint   th32ModuleID;
            internal uint   th32ProcessID;
            internal uint   GlblcntUsage;
            internal uint   ProccntUsage;
            internal IntPtr modBaseAddr;
            internal uint   modBaseSize;
            internal IntPtr hModule;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            internal string szModule;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            internal string szExePath;
        }

        [DllImport("kernel32.dll")]
        private static extern bool Module32First(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

        [DllImport("kernel32.dll")]
        private static extern bool Module32Next(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

        [DllImport("kernel32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle([In] IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateToolhelp32Snapshot(SnapshotFlags dwFlags, IntPtr th32ProcessID);

        public static MODULEENTRY32 GetModuleInfo(IntPtr procId, string modName) {
            var moduleInfo = default(MODULEENTRY32);
            var hSnap      = CreateToolhelp32Snapshot(SnapshotFlags.Module | SnapshotFlags.Module32, procId);

            if (hSnap.ToInt64() != INVALID_HANDLE_VALUE) {
                var modEntry = new MODULEENTRY32 {dwSize = (uint) Marshal.SizeOf(typeof(MODULEENTRY32))};

                if (Module32First(hSnap, ref modEntry)) {
                    do {
                        if (modEntry.szModule.Equals(modName)) {
                            moduleInfo = modEntry;
                            break;
                        }
                    } while (Module32Next(hSnap, ref modEntry));
                }
            }
            CloseHandle(hSnap);

            return moduleInfo;
        }

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);
    }
}