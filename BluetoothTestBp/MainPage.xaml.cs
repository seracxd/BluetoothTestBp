using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using Windows.Devices.Bluetooth;

// Dokumentaci k šabloně položky Prázdná stránka najdete na adrese https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x405

namespace BluetoothTestBp
{
    /// <summary>
    /// Prázdná stránka, která se dá použít samostatně nebo v rámci objektu Frame
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            InitializeGattServer();
        }
        public async void InitializeGattServer()
        {
            // UUID pro vaši GATT službu
            var serviceUuid = new Guid("0000180D-0000-1000-8000-00805F9B34FB");
            var serviceProviderResult = await GattServiceProvider.CreateAsync(serviceUuid);

            if (serviceProviderResult.Error != BluetoothError.Success)
            {
                Console.WriteLine("Failed to create GATT service");
                return;
            }

            var serviceProvider = serviceProviderResult.ServiceProvider;

            // Přidání charakteristiky
            var characteristicUuid = new Guid("00002A37-0000-1000-8000-00805F9B34FB"); // UUID pro vaši charakteristiku
            var characteristicParameters = new GattLocalCharacteristicParameters
            {
                CharacteristicProperties = GattCharacteristicProperties.Read | GattCharacteristicProperties.Notify,
                WriteProtectionLevel = GattProtectionLevel.Plain
            };

            var characteristicResult = await serviceProvider.Service.CreateCharacteristicAsync(characteristicUuid, characteristicParameters);
            if (characteristicResult.Error != BluetoothError.Success)
            {
                Console.WriteLine("Failed to create GATT characteristic");
                return;
            }

            var characteristic = characteristicResult.Characteristic;

            // Nastavení událostí pro čtení a zápis
            characteristic.ReadRequested += async (sender, args) =>
            {
                var deferral = args.GetDeferral();
                var readRequest = await args.GetRequestAsync();
                var writer = new DataWriter();
                writer.WriteString("Sample data for reading");
                readRequest.RespondWithValue(writer.DetachBuffer());
                deferral.Complete();
            };

            characteristic.WriteRequested += async (sender, args) =>
            {
                var request = await args.GetRequestAsync();
                var reader = DataReader.FromBuffer(request.Value);
                string receivedData = reader.ReadString(request.Value.Length);
                Console.WriteLine("Received data: " + receivedData);
                request.Respond();
            };

            // Spuštění služby, aby byla dostupná BLE klientům
            serviceProvider.StartAdvertising();
            Console.WriteLine("GATT server started and advertising!");
        }
    }

}
