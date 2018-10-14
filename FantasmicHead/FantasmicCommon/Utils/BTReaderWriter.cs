using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace FantasmicCommon.Utils
{
    public class BTReaderWriter
    {
        public DeviceInformation BTDeviceInfo { get; set; }
        public DataWriter BTWriter { get; set; }
        public DataReader BTReader { get; set; }
        private RfcommDeviceService BTDeviceService { get; set; }
        public StreamSocket BTStreamSocket { get; set; }

        private bool isSocketOpened = false;

        public BTReaderWriter(DeviceInformation device)
        {
            this.BTDeviceInfo = device;
        }

        public BTReaderWriter(DataReader reader, DataWriter writer)
        {
            this.BTWriter = writer;
            this.BTReader = reader;
        }

        public async Task ConnectBTService()
        {
            BluetoothDevice btDevice;

            if (isSocketOpened)
            {
                Debug.WriteLine("すでにソケットが開かれています。");
                return;
            }

            // Perform device access checks before trying to get the device.
            // First, we check if consent has been explicitly denied by the user.
            DeviceAccessStatus accessStatus = DeviceAccessInformation.CreateFromId(BTDeviceInfo.Id).CurrentStatus;
            if (accessStatus == DeviceAccessStatus.DeniedByUser)
            {
                //rootPage.NotifyUser("This app does not have access to connect to the remote device (please grant access in Settings > Privacy > Other Devices", NotifyType.ErrorMessage);
                throw new UnauthorizedAccessException("ユーザーによってデバイスへのアクセスが拒否されました。");
            }
            // If not, try to get the Bluetooth device
            try
            {
                btDevice = await BluetoothDevice.FromIdAsync(BTDeviceInfo.Id);
                if (btDevice.ConnectionStatus == BluetoothConnectionStatus.Disconnected)
                {
                    //btDevice.RequestAccessAsync
                }
            }
            catch (Exception ex)
            {
                //rootPage.NotifyUser(ex.Message, NotifyType.ErrorMessage);
                //ResetMainUI();
                throw new Exception("Bluetooth Device の取得に失敗しました。", ex);
            }
            // If we were unable to get a valid Bluetooth device object,
            // it's most likely because the user has specified that all unpaired devices
            // should not be interacted with.
            if (btDevice == null)
            {
                throw new NullReferenceException("Bluetooth Device が空です。");
            }


            //Pairされているか確認する
            if (btDevice.DeviceInformation.Pairing.IsPaired == false)
            {
                var status = await btDevice.RequestAccessAsync();
                if (status == DeviceAccessStatus.Allowed)
                {
                    Debug.WriteLine("access granted");
                }
            }


            // This should return a list of uncached Bluetooth services (so if the server was not active when paired, it will still be detected by this call
            var rfcommServices = await btDevice.GetRfcommServicesForIdAsync(RfcommServiceId.FromUuid(Constants.RfcommChatServiceUuid), BluetoothCacheMode.Uncached);

            if (rfcommServices.Services.Count > 0)
            {
                BTDeviceService = rfcommServices.Services[0];
            }
            else
            {
                rfcommServices = await btDevice.GetRfcommServicesAsync();
                if (rfcommServices.Services.Count == 0)
                {
                    throw new NullReferenceException("対象のデバイスにBluetoothサービスが一つも見つかりません。正しい機器に接続していない可能性があります。");
                }
                else
                {
                    foreach (var service in rfcommServices.Services)
                    {
                        Debug.WriteLine(service.ConnectionServiceName + ":::" + service.Device.DeviceInformation.Kind.ToString());
                        if (service.ConnectionServiceName.Contains(Constants.RfcommChatServiceUuid.ToString()))
                        {
                            BTDeviceService = service;
                            break;
                        }
                    }
                    if (BTDeviceService == null)
                    {
                        throw new NullReferenceException("対象のデバイスにBluetoothサービスが一つも見つかりません。正しい機器に接続していない可能性があります。");
                    }
                }
            }

            // Do various checks of the SDP record to make sure you are talking to a device that actually supports the Bluetooth Rfcomm Chat Service
            var attributes = await BTDeviceService.GetSdpRawAttributesAsync();
            if (!attributes.ContainsKey(Constants.SdpServiceNameAttributeId))
            {
                throw new NullReferenceException("対象のデバイスにFantasmicサービスが見つかりません。正しい機器に接続していない可能性があります。");
            }
            var attributeReader = DataReader.FromBuffer(attributes[Constants.SdpServiceNameAttributeId]);
            var attributeType = attributeReader.ReadByte();
            if (attributeType != Constants.SdpServiceNameAttributeType)
            {
                throw new NullReferenceException("対象のデバイスにFantasmicサービスが見つかりません。正しい機器に接続していない可能性があります。");
            }
            var serviceNameLength = attributeReader.ReadByte();

            // The Service Name attribute requires UTF-8 encoding.
            attributeReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;

            lock (this)
            {
                BTStreamSocket = new StreamSocket();
            }
            try
            {
                await BTStreamSocket.ConnectAsync(BTDeviceService.ConnectionHostName, BTDeviceService.ConnectionServiceName);

                //SetChatUI(attributeReader.ReadString(serviceNameLength), bluetoothDevice.Name);
                BTWriter = new DataWriter(BTStreamSocket.OutputStream);
                BTReader = new DataReader(BTStreamSocket.InputStream);
                isSocketOpened = true;
            }
            catch (Exception ex) when ((uint)ex.HResult == 0x80070490) // ERROR_ELEMENT_NOT_FOUND
            {
                throw new NullReferenceException("ソケットのオープンに失敗しました。対象のデバイスでアプリケーションが起動されていることをご確認ください。(0x80070490: ERROR_ELEMENT_NOT_FOUND)", ex);
            }
            catch (Exception ex) when ((uint)ex.HResult == 0x80072740) // WSAEADDRINUSE
            {
                throw new InvalidOperationException("ソケットのオープンに失敗しました。対象のデバイスがすでに他のサーバーに接続されている可能性があります。(0x80072740: WSAEADDRINUSE)", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("ソケットのオープンに失敗しました。", ex);
            }
        }

        public void Disconnect()
        {
            if (BTWriter != null)
            {
                BTWriter.DetachStream();
                BTWriter = null;
            }


            if (BTDeviceService != null)
            {
                BTDeviceService.Dispose();
                BTDeviceService = null;
            }
            lock (this)
            {
                if (BTStreamSocket != null)
                {
                    BTStreamSocket.Dispose();
                    BTStreamSocket = null;
                }
                isSocketOpened = false;
            }
        }
    }

    /// <summary>
    /// Class containing Attributes and UUIDs that will populate the SDP record.
    /// </summary>
    class Constants
    {
        // The Chat Server's custom service Uuid: 34B1CF4D-1069-4AD6-89B6-E161D79BE4D8
        public static readonly Guid RfcommChatServiceUuid = Guid.Parse("34B1CF4D-1069-4AD6-89B6-E161D79BE4D8");
        //public static readonly Guid RfcommChatServiceUuid = Guid.Parse("0000111f-0000-1000-8000-00805f9b34fb");
        // The Id of the Service Name SDP attribute
        public const UInt16 SdpServiceNameAttributeId = 0x100;

        // The SDP Type of the Service Name SDP attribute.
        // The first byte in the SDP Attribute encodes the SDP Attribute Type as follows :
        //    -  the Attribute Type size in the least significant 3 bits,
        //    -  the SDP Attribute Type value in the most significant 5 bits.
        public const byte SdpServiceNameAttributeType = (4 << 3) | 5;

        // The value of the Service Name SDP attribute
        public const string SdpServiceName = "Bluetooth Rfcomm Fantasmic Service";
    }
}
