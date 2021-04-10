using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Inventory_Editor.Models {
    public class Item : INotifyPropertyChanged {
        private readonly IntPtr idAddress;
        private readonly IntPtr valueAddress;

        /// <param name="index">Index.</param>
        /// <param name="address">The ItemStack address (not a pointer).</param>
        public Item(int index, IntPtr address) {
            Index = index;

            idAddress    = address;
            valueAddress = address.Add(0x2);
        }

        public int Index { [UsedImplicitly] get; }

        [UsedImplicitly]
        public short Id => Global.ReadMem<short>(idAddress);

        [UsedImplicitly]
        public string Name {
            get {
                if (ItemLookup.items.ContainsKey(Id)) return ItemLookup.items[Id];
                return Id switch {
                    151 => "Special Ops Notebook",
                    60 => "U Material",
                    0x2ca => "Champion Medallion",
                    _ => "Unknown"
                };
            }
        }

        [UsedImplicitly]
        public short Value {
            get => Global.ReadMem<short>(valueAddress);
            set {
                Global.WriteMem(valueAddress, value);
                OnPropertyChanged(nameof(Value));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        [UsedImplicitly]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}