using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

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
        RfcommDeviceService _service;
        StreamSocket _socket;

        /*Events*/
        public event EventHandler InitializeCompleted;

        protected virtual void OnInitializeCompleted(BTInitEventArgs e)
        {
            InitializeCompleted?.Invoke(this, e);
        }

        public async void Initialize()
        {
            // Enumerate devices with the object push service
            var services =
                await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(
                    RfcommDeviceService.GetDeviceSelector(RfcommServiceId.GenericFileTransfer));

            if (services != null && services.Count > 0)
            {
                // Initialize the target Bluetooth BR device
                var service = await RfcommDeviceService.FromIdAsync(services[0].Id);

                // Check that the service meets this App's minimum requirement
                if (SupportsProtection(service) && await IsCompatibleVersion(service))
                {
                    _service = service;

                    // Create a socket and connect to the target
                    _socket = new StreamSocket();
                    await _socket.ConnectAsync(
                        _service.ConnectionHostName,
                        _service.ConnectionServiceName,
                        SocketProtectionLevel
                            .BluetoothEncryptionAllowNullAuthentication);

                    OnInitializeCompleted(new BTInitEventArgs(_service.ConnectionHostName.ToString()));
                    // The socket is connected. At this point the App can wait for
                    // the user to take some action, e.g. click a button to send a
                    // file to the device, which could invoke the Picker and then
                    // send the picked file. The transfer itself would use the
                    // Sockets API and not the Rfcomm API, and so is omitted here for
                    // brevity.
                }
            }
            OnInitializeCompleted(new BTInitEventArgs("not found"));

        }

        // This App requires a connection that is encrypted but does not care about
        // whether its authenticated.
        bool SupportsProtection(RfcommDeviceService service)
        {
            switch (service.ProtectionLevel)
            {
                case SocketProtectionLevel.PlainSocket:
                    if ((service.MaxProtectionLevel == SocketProtectionLevel
                            .BluetoothEncryptionWithAuthentication)
                        || (service.MaxProtectionLevel == SocketProtectionLevel
                            .BluetoothEncryptionAllowNullAuthentication))
                    {
                        // The connection can be upgraded when opening the socket so the
                        // App may offer UI here to notify the user that Windows may
                        // prompt for a PIN exchange.
                        return true;
                    }
                    else
                    {
                        // The connection cannot be upgraded so an App may offer UI here
                        // to explain why a connection won't be made.
                        return false;
                    }
                case SocketProtectionLevel.BluetoothEncryptionWithAuthentication:
                    return true;
                case SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication:
                    return true;
            }
            return false;
        }

        // This App relies on CRC32 checking available in version 2.0 of the service.
        const uint SERVICE_VERSION_ATTRIBUTE_ID = 0x0300;
        const byte SERVICE_VERSION_ATTRIBUTE_TYPE = 0x0A;   // UINT32
        const uint MINIMUM_SERVICE_VERSION = 200;
        async Task<bool> IsCompatibleVersion(RfcommDeviceService service)
        {
            var attributes = await service.GetSdpRawAttributesAsync(
                BluetoothCacheMode.Uncached);
            var attribute = attributes[SERVICE_VERSION_ATTRIBUTE_ID];
            var reader = DataReader.FromBuffer(attribute);

            // The first byte contains the attribute' s type
            byte attributeType = reader.ReadByte();
            if (attributeType == SERVICE_VERSION_ATTRIBUTE_TYPE)
            {
                // The remainder is the data
                uint version = reader.ReadUInt32();
                return version >= MINIMUM_SERVICE_VERSION;
            }
            else
            {
                //TODO: なんか適切なエラーに置き換え
                throw new Exception();
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
