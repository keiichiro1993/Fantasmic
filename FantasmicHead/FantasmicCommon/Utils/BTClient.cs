using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;

namespace FantasmicCommon.Utils.BTClient
{
    public class BTInitEventArgs : EventArgs
    {
        public BTInitEventArgs(string hostName)
        {
            ConnectionHostName = hostName;
        }
        public string ConnectionHostName { get; set; }
    }

    public class BTSender
    {
        DeviceWatcher deviceWatcher;
        public ObservableCollection<DeviceInformation> DeviceInfoCollection { get; set; }

        public BTSender()
        {
            DeviceInfoCollection = new ObservableCollection<DeviceInformation>();
        }

        /*Events*/
        public event EventHandler InitializeCompleted;

        protected virtual void OnInitializeCompleted(BTInitEventArgs e)
        {
            InitializeCompleted?.Invoke(this, e);
        }

        public void Initialize()
        {
            StartDeviceWatcher();
        }


        private void StartDeviceWatcher()
        {
            // Request additional properties
            if (deviceWatcher == null)
            {
                deviceWatcher = DeviceInformation.CreateWatcher(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort));
                deviceWatcher.Added += deviceWatcher_Added;
                deviceWatcher.Removed += deviceWatcher_Removed;
                deviceWatcher.Updated += deviceWatcher_Updated;
                deviceWatcher.EnumerationCompleted += deviceWatcher_EnumerationCompleted;
            }
            deviceWatcher.Start();
        }

        private void deviceWatcher_Added(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            DeviceInfoCollection.Add(deviceInfo);
        }

        private void deviceWatcher_EnumerationCompleted(DeviceWatcher sender, object args)
        {
            OnInitializeCompleted(new BTInitEventArgs("found: " + DeviceInfoCollection.Count.ToString() + " devices."));
        }

        private void deviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            foreach (var deviceInfo in DeviceInfoCollection)
            {
                if (deviceInfo.Id == deviceInfoUpdate.Id)
                {
                    deviceInfo.Update(deviceInfoUpdate);
                    break;
                }
            }
        }

        private void deviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            foreach (var deviceInfo in DeviceInfoCollection)
            {
                if (deviceInfo.Id == deviceInfoUpdate.Id)
                {
                    DeviceInfoCollection.Remove(deviceInfo);
                    break;
                }
            }
        }
    }

    public class BTListner
    {
        RfcommServiceProvider _provider;
        StreamSocket _socket;

        async void Initialize()
        {
            // Initialize the provider for the hosted RFCOMM service
            _provider = await RfcommServiceProvider.CreateAsync(RfcommServiceId.ObexObjectPush);

            // Create a listener for this service and start listening
            StreamSocketListener listener = new StreamSocketListener();
            listener.ConnectionReceived += OnConnectionReceived;
            await listener.BindServiceNameAsync(
                _provider.ServiceId.AsString(),
                SocketProtectionLevel
                    .BluetoothEncryptionAllowNullAuthentication);

            // Set the SDP attributes and start advertising
            InitializeServiceSdpAttributes(_provider);
            _provider.StartAdvertising(listener);
        }

        const uint SERVICE_VERSION_ATTRIBUTE_ID = 0x0300;
        const byte SERVICE_VERSION_ATTRIBUTE_TYPE = 0x0A;   // UINT32
        const uint SERVICE_VERSION = 200;
        void InitializeServiceSdpAttributes(RfcommServiceProvider provider)
        {
            var writer = new Windows.Storage.Streams.DataWriter();

            // First write the attribute type
            writer.WriteByte(SERVICE_VERSION_ATTRIBUTE_TYPE);
            // Then write the data
            writer.WriteUInt32(SERVICE_VERSION);

            var data = writer.DetachBuffer();
            provider.SdpRawAttributes.Add(SERVICE_VERSION_ATTRIBUTE_ID, data);
        }

        async void OnConnectionReceived(
            StreamSocketListener listener,
            StreamSocketListenerConnectionReceivedEventArgs args)
        {
            // Stop advertising/listening so that we're only serving one client
            _provider.StopAdvertising();
            await listener.CancelIOAsync();
            _socket = args.Socket;

            // The client socket is connected. At this point the App can wait for
            // the user to take some action, e.g. click a button to receive a file
            // from the device, which could invoke the Picker and then save the
            // received file to the picked location. The transfer itself would use
            // the Sockets API and not the Rfcomm API, and so is omitted here for
            // brevity.
        }
    }
}
