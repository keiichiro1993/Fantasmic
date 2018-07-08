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
using Windows.UI.Xaml.Controls;

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
        Page mainPage;

        StreamSocket chatSocket;
        DataWriter chatWriter;
        RfcommDeviceService chatService;

        public ObservableCollection<DeviceInformation> DeviceInfoCollection { get; set; }

        public BTSender(Page mainPage)
        {
            this.mainPage = mainPage;
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

        public void StopDeviceWatcher()
        {
            deviceWatcher.Stop();
        }


        private void StartDeviceWatcher()
        {
            // Request additional properties
            if (deviceWatcher == null)
            {
                //deviceWatcher = DeviceInformation.CreateWatcher(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort));
                string[] requestedProperties = new string[] { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };

                deviceWatcher = DeviceInformation.CreateWatcher("(System.Devices.Aep.ProtocolId:=\"{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}\")",
                                                                requestedProperties,
                                                                DeviceInformationKind.AssociationEndpoint);

                deviceWatcher.Added += deviceWatcher_AddedAsync;
                deviceWatcher.Removed += deviceWatcher_Removed;
                deviceWatcher.Updated += deviceWatcher_UpdatedAsync;
                deviceWatcher.EnumerationCompleted += deviceWatcher_EnumerationCompleted;
            }
            deviceWatcher.Start();
        }

        private async void deviceWatcher_AddedAsync(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            await mainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate
            {
                DeviceInfoCollection.Add(deviceInfo);
            });
        }

        private async void deviceWatcher_EnumerationCompleted(DeviceWatcher sender, object args)
        {
            await mainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate
            {
                OnInitializeCompleted(new BTInitEventArgs("found: " + DeviceInfoCollection.Count.ToString() + " devices."));
            });
        }

        private async void deviceWatcher_UpdatedAsync(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            await mainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate
            {
                foreach (var deviceInfo in DeviceInfoCollection)
                {
                    if (deviceInfo.Id == deviceInfoUpdate.Id)
                    {
                        deviceInfo.Update(deviceInfoUpdate);
                        break;
                    }
                }
            });
        }

        private async void deviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            await mainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate
            {
                foreach (var deviceInfo in DeviceInfoCollection)
                {
                    if (deviceInfo.Id == deviceInfoUpdate.Id)
                    {
                        DeviceInfoCollection.Remove(deviceInfo);
                        break;
                    }
                }
            });
        }

        private async void ConnectReciever(DeviceInformation device)
        {
            BluetoothDevice btDevice;

            // Perform device access checks before trying to get the device.
            // First, we check if consent has been explicitly denied by the user.
            DeviceAccessStatus accessStatus = DeviceAccessInformation.CreateFromId(device.Id).CurrentStatus;
            if (accessStatus == DeviceAccessStatus.DeniedByUser)
            {
                //rootPage.NotifyUser("This app does not have access to connect to the remote device (please grant access in Settings > Privacy > Other Devices", NotifyType.ErrorMessage);
                return;
            }
            // If not, try to get the Bluetooth device
            try
            {
                btDevice = await BluetoothDevice.FromIdAsync(device.Id);
            }
            catch (Exception ex)
            {
                //rootPage.NotifyUser(ex.Message, NotifyType.ErrorMessage);
                //ResetMainUI();
                return;
            }
            // If we were unable to get a valid Bluetooth device object,
            // it's most likely because the user has specified that all unpaired devices
            // should not be interacted with.
            if (btDevice == null)
            {
                //rootPage.NotifyUser("Bluetooth Device returned null. Access Status = " + accessStatus.ToString(), NotifyType.ErrorMessage);
            }

            // This should return a list of uncached Bluetooth services (so if the server was not active when paired, it will still be detected by this call
            var rfcommServices = await btDevice.GetRfcommServicesForIdAsync(
                RfcommServiceId.FromUuid(Constants.RfcommChatServiceUuid), BluetoothCacheMode.Uncached);

            if (rfcommServices.Services.Count > 0)
            {
                chatService = rfcommServices.Services[0];
            }
            else
            {
                //rootPage.NotifyUser(
                //   "Could not discover the chat service on the remote device",
                //   NotifyType.StatusMessage);
                //ResetMainUI();
                return;
            }

            // Do various checks of the SDP record to make sure you are talking to a device that actually supports the Bluetooth Rfcomm Chat Service
            var attributes = await chatService.GetSdpRawAttributesAsync();
            if (!attributes.ContainsKey(Constants.SdpServiceNameAttributeId))
            {
                //rootPage.NotifyUser(
                //    "The Chat service is not advertising the Service Name attribute (attribute id=0x100). " +
                //    "Please verify that you are running the BluetoothRfcommChat server.",
                //    NotifyType.ErrorMessage);
                //ResetMainUI();
                return;
            }
            var attributeReader = DataReader.FromBuffer(attributes[Constants.SdpServiceNameAttributeId]);
            var attributeType = attributeReader.ReadByte();
            if (attributeType != Constants.SdpServiceNameAttributeType)
            {
                //rootPage.NotifyUser(
                //    "The Chat service is using an unexpected format for the Service Name attribute. " +
                //    "Please verify that you are running the BluetoothRfcommChat server.",
                //    NotifyType.ErrorMessage);
                //ResetMainUI();
                return;
            }
            var serviceNameLength = attributeReader.ReadByte();

            // The Service Name attribute requires UTF-8 encoding.
            attributeReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;

            //StopWatcher();

            lock (this)
            {
                chatSocket = new StreamSocket();
            }
            try
            {
                await chatSocket.ConnectAsync(chatService.ConnectionHostName, chatService.ConnectionServiceName);

                //SetChatUI(attributeReader.ReadString(serviceNameLength), bluetoothDevice.Name);
                chatWriter = new DataWriter(chatSocket.OutputStream);

                DataReader chatReader = new DataReader(chatSocket.InputStream);
                ReceiveStringLoop(chatReader);
            }
            catch (Exception ex) when ((uint)ex.HResult == 0x80070490) // ERROR_ELEMENT_NOT_FOUND
            {
                //rootPage.NotifyUser("Please verify that you are running the BluetoothRfcommChat server.", NotifyType.ErrorMessage);
                //ResetMainUI();
            }
            catch (Exception ex) when ((uint)ex.HResult == 0x80072740) // WSAEADDRINUSE
            {
                //rootPage.NotifyUser("Please verify that there is no other RFCOMM connection to the same device.", NotifyType.ErrorMessage);
                //ResetMainUI();
            }
        }

        //TODO: ConnectとRecieve分ける。
        private async void ConnectSender(DeviceInformation device)
        {
            try
            {
                if (MessageTextBox.Text.Length != 0)
                {
                    chatWriter.WriteUInt32((uint)MessageTextBox.Text.Length);
                    chatWriter.WriteString(MessageTextBox.Text);

                    ConversationList.Items.Add("Sent: " + MessageTextBox.Text);
                    MessageTextBox.Text = "";
                    await chatWriter.StoreAsync();

                }
            }
            catch (Exception ex) when ((uint)ex.HResult == 0x80072745)
            {
                // The remote device has disconnected the connection
                rootPage.NotifyUser("Remote side disconnect: " + ex.HResult.ToString() + " - " + ex.Message,
                    NotifyType.StatusMessage);
            }
        }

        private async void ReceiveStringLoop(DataReader chatReader)
        {
            try
            {
                uint size = await chatReader.LoadAsync(sizeof(uint));
                if (size < sizeof(uint))
                {
                    Disconnect("Remote device terminated connection - make sure only one instance of server is running on remote device");
                    return;
                }

                uint stringLength = chatReader.ReadUInt32();
                uint actualStringLength = await chatReader.LoadAsync(stringLength);
                if (actualStringLength != stringLength)
                {
                    // The underlying socket was closed before we were able to read the whole data
                    return;
                }

                //ConversationList.Items.Add("Received: " + chatReader.ReadString(stringLength));
                //TODO: 受信した文字列のハンドル
                String resultString = chatReader.ReadString(stringLength);

                ReceiveStringLoop(chatReader);
            }
            catch (Exception ex)
            {
                lock (this)
                {
                    if (chatSocket == null)
                    {
                        // Do not print anything here -  the user closed the socket.
                        if ((uint)ex.HResult == 0x80072745)
                        {
                            //rootPage.NotifyUser("Disconnect triggered by remote device", NotifyType.StatusMessage);
                        }
                        else if ((uint)ex.HResult == 0x800703E3)
                        {
                            //rootPage.NotifyUser("The I/O operation has been aborted because of either a thread exit or an application request.", NotifyType.StatusMessage);
                        }
                    }
                    else
                    {
                        Disconnect("Read stream failed with error: " + ex.Message);
                    }
                }
            }
        }

        private void Disconnect(string disconnectReason)
        {
            if (chatWriter != null)
            {
                chatWriter.DetachStream();
                chatWriter = null;
            }


            if (chatService != null)
            {
                chatService.Dispose();
                chatService = null;
            }
            lock (this)
            {
                if (chatSocket != null)
                {
                    chatSocket.Dispose();
                    chatSocket = null;
                }
            }

            //rootPage.NotifyUser(disconnectReason, NotifyType.StatusMessage);
            //ResetMainUI();
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

    /// <summary>
    /// Class containing Attributes and UUIDs that will populate the SDP record.
    /// </summary>
    class Constants
    {
        // The Chat Server's custom service Uuid: 34B1CF4D-1069-4AD6-89B6-E161D79BE4D8
        public static readonly Guid RfcommChatServiceUuid = Guid.Parse("34B1CF4D-1069-4AD6-89B6-E161D79BE4D8");

        // The Id of the Service Name SDP attribute
        public const UInt16 SdpServiceNameAttributeId = 0x100;

        // The SDP Type of the Service Name SDP attribute.
        // The first byte in the SDP Attribute encodes the SDP Attribute Type as follows :
        //    -  the Attribute Type size in the least significant 3 bits,
        //    -  the SDP Attribute Type value in the most significant 5 bits.
        public const byte SdpServiceNameAttributeType = (4 << 3) | 5;

        // The value of the Service Name SDP attribute
        public const string SdpServiceName = "Bluetooth Rfcomm Chat Service";
    }
}
