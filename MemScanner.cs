using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Inventory_Editor {
    public static class MemScanner {
        [SuppressMessage("ReSharper", "ParameterHidesMember")]
        private static byte[] ConvertPattern(string pattern) {
            var convertedArray = new List<byte>();
            foreach (var each in pattern.Split(' ')) {
                if (each == "??") {
                    convertedArray.Add(Convert.ToByte("0", 16));
                } else {
                    convertedArray.Add(Convert.ToByte(each, 16));
                }
            }
            return convertedArray.ToArray();
        }

        private static IntPtr ScanLogic(ProcessModule processModule, byte[] localModuleBytes, byte[] convertedByteArray) {
            for (var indexAfterBase = 0; indexAfterBase < localModuleBytes.Length; indexAfterBase++) {
                var noMatch = false;

                if (localModuleBytes[indexAfterBase] != convertedByteArray[0]) continue;

                for (var matchedIndex = 0; matchedIndex < convertedByteArray.Length && indexAfterBase + matchedIndex < localModuleBytes.Length; matchedIndex++) {
                    if (convertedByteArray[matchedIndex] == 0x0) continue;

                    if (convertedByteArray[matchedIndex] != localModuleBytes[indexAfterBase + matchedIndex]) {
                        noMatch = true;
                        break;
                    }
                }

                if (!noMatch) {
                    return processModule.BaseAddress + indexAfterBase;
                }
            }
            return default;
        }

        public static IntPtr FindPattern(this ProcessModule localModule, string pattern) {
            var localModuleBytes   = new byte[localModule.ModuleMemorySize];
            var convertedByteArray = ConvertPattern(pattern);

            PInvoke.ReadProcessMemory(Global.processHandle, localModule.BaseAddress, localModuleBytes, localModule.ModuleMemorySize, out _);

            return ScanLogic(localModule, localModuleBytes, convertedByteArray);
        }
    }
}