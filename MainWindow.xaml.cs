using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Inventory_Editor.Models;

namespace Inventory_Editor {
    public partial class MainWindow {
        private const string PROCESS_NAME = "ed8_4_PC";
        private const string MODULE_NAME  = "ed8_4_PC.exe";

        public readonly ObservableCollection<Item> items = new ObservableCollection<Item>();

        public MainWindow() {
            InitializeComponent();

            var processes = Process.GetProcessesByName(PROCESS_NAME);
            if (processes.Length == 0) {
                Application.Current.Shutdown();
                return;
            }

            var process = processes[0];
            Global.processHandle = Imports.OpenProcess(Imports.PROCESS_ALL_ACCESS, false, process.Id);

            var moduleInfo                = process.GetModule(MODULE_NAME);
            var getItemFunctionStart      = moduleInfo.FindPattern("4D ?? ?? ?? ?? ?? ?? 90 8B C8 48 C1 E1 05") - 24;
            var codeOffsetForBasePtr      = BitConverter.ToUInt32(Global.ReadMem(getItemFunctionStart + 3, 4)) + 7;
            var basePtr                   = getItemFunctionStart.AddAndReadAsPointer(codeOffsetForBasePtr);
            var codeOffsetForItemCountAdr = BitConverter.ToUInt32(Global.ReadMem(getItemFunctionStart + 12, 4));
            var itemCntAdr                = basePtr.Add(codeOffsetForItemCountAdr);
            var itemCnt                   = Global.ReadMem<int>(itemCntAdr);
            var baseInvPtr                = itemCntAdr.AddAndReadAsPointer(8);

            for (var i = 0; i < itemCnt; i++) {
                var itemAddress = baseInvPtr.Add(i * 0x20);
                var item        = new Item(i, itemAddress);
                items.Add(item);
            }

            dg_items.ItemsSource = items;

            SetupKeybind(new KeyGesture(Key.D, ModifierKeys.Control), (sender, args) => SetTo80());
        }

        private void SetupKeybind(InputGesture keyGesture, ExecutedRoutedEventHandler onPress) {
            var changeItemValues = new RoutedCommand();
            var ib               = new InputBinding(changeItemValues, keyGesture);
            InputBindings.Add(ib);
            // Bind handler.
            var cb = new CommandBinding(changeItemValues);
            cb.Executed += onPress;
            CommandBindings.Add(cb);
        }

        private void SetTo80() {
            foreach (var item in items) {
                if (item.Value > 50) {
                    item.Value = 80;
                }
            }
        }

        private void OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e) {
            Type sourceClassType = ((dynamic) e.PropertyDescriptor).ComponentType;
            var  propertyInfo    = sourceClassType.GetProperties().FirstOrDefault(info => info.Name == e.PropertyName);
            var  displayName     = ((DisplayNameAttribute) propertyInfo?.GetCustomAttribute(typeof(DisplayNameAttribute), true))?.DisplayName;

            if (displayName != null) e.Column.Header = displayName;
        }
    }
}