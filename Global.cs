using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Inventory_Editor {
    public static class Global {
        //public static readonly Dictionary<short, string> ID_DUMP;

        public static IntPtr processHandle;

        static Global() {
            //var json = Encoding.UTF8.GetString(Data.ItemData);
            //ID_DUMP = JsonConvert.DeserializeObject<Dictionary<short, string>>(json);
        }

        public static T GetDataAs<T>(this IEnumerable<byte> bytes) {
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

            try {
                return (T) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            } finally {
                if (handle.IsAllocated) {
                    handle.Free();
                }
            }
        }

        public static byte[] GetBytes<T>(this T @struct) {
            var size   = Marshal.SizeOf(@struct);
            var bytes  = new byte[size];
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

            try {
                Marshal.StructureToPtr(@struct, handle.AddrOfPinnedObject(), false);
                return bytes;
            } finally {
                if (handle.IsAllocated) {
                    handle.Free();
                }
            }
        }

        /**
         * Reads the address given as a pointer, returning the address it points to.
         */
        public static IntPtr ReadAsPointer(this IntPtr self) {
            return ReadMem<IntPtr>(self);
        }

        public static IntPtr ReadAsPointer32(this IntPtr self) {
            return (IntPtr) ReadMem<int>(self);
        }

        public static IntPtr Add(this IntPtr self, IntPtr other) {
            return self.Add(other.ToInt64());
        }

        public static IntPtr Add(this IntPtr self, long other) {
            return new IntPtr(self.ToInt64() + other);
        }

        public static IntPtr AddAndReadAsPointer(this IntPtr self, long other) {
            return new IntPtr(self.ToInt64() + other).ReadAsPointer();
        }

        public static IntPtr AddAndReadAsPointer32(this IntPtr self, long other) {
            return new IntPtr(self.ToInt64() + other).ReadAsPointer32();
        }

        public static T ReadMem<T>(IntPtr address) where T : struct {
            var length = Marshal.SizeOf(default(T));
            var buffer = new byte[length];
            Imports.ReadProcessMemory(processHandle, address, buffer, length, out var read);
            return buffer.GetDataAs<T>();
        }

        public static byte[] ReadMem(IntPtr address, int length) {
            var buffer = new byte[length];
            Imports.ReadProcessMemory(processHandle, address, buffer, length, out var read);
            return buffer;
        }

        public static void WriteMem<T>(IntPtr address, T value) {
            var bytes = value.GetBytes();
            Imports.WriteProcessMemory(processHandle, address, bytes, bytes.Length, out var read);
        }

        public static void WriteMem(IntPtr address, byte[] bytes) {
            Imports.WriteProcessMemory(processHandle, address, bytes, bytes.Length, out var read);
        }

        public static V TryGet<K, V>(this IDictionary<K, V> dict, K key, V defaultValue) {
            if (dict == null) return defaultValue;
            return dict.ContainsKey(key) ? dict[key] : defaultValue;
        }

        public static string TryGet<K>(this IDictionary<K, string> dict, K key, string defaultValue = "Unknown") {
            if (dict == null) return defaultValue;
            return dict.ContainsKey(key) ? dict[key] : defaultValue;
        }

        public static string ReadNullTermString(this IntPtr address, int bufferLength) {
            var bytes = ReadMem(address, bufferLength);
            return ReadNullTermString(bytes);
        }

        public static string ReadNullTermString(byte[] buffer) {
            var stringBytes = new List<byte>();
            var i           = 0;
            do {
                stringBytes.Add(buffer[i++]);
            } while (stringBytes[^1] != 0 && i < buffer.Length);

            return Encoding.ASCII.GetString(stringBytes.Subsequence(0, stringBytes.Count).ToArray());
        }

        public static char[] ToNullTermCharArray(this string str) {
            str ??= "\0";
            if (!str.EndsWith("\0")) str += "\0";
            return str.ToCharArray();
        }

        public static byte[] ToByteArray(this char[] str) {
            return Encoding.ASCII.GetBytes(str);
        }

        public static T[] Subsequence<T>(this IEnumerable<T> arr, int startIndex, int length) {
            return arr.Skip(startIndex).Take(length).ToArray();
        }
    }
}