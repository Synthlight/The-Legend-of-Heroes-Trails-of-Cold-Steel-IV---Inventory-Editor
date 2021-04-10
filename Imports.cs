using System;
using System.Runtime.InteropServices;

namespace Inventory_Editor {
    public static class Imports {
        public const int PROCESS_ALL_ACCESS   = PROCESS_VM_OPERATION | PROCESS_VM_READ | PROCESS_VM_WRITE;
        public const int PROCESS_VM_OPERATION = 0x0008;
        public const int PROCESS_VM_READ      = 0x0010;
        public const int PROCESS_VM_WRITE     = 0x0020;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();
    }
}