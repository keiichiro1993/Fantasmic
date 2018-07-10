using System;
using System.Collections.Generic;
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
        private RfcommDeviceService BTDeviceService { get; set; }
        private StreamSocket BTStreamSocket { get; set; }
        public DataWriter BTWriter { get; set; }
        public DataReader BTReader { get; set; }
        public BTReaderWriter(DeviceInformation device)
        {
            BTDeviceInfo = device;
        }

        public async Task ConnectBTService()
        {
            BluetoothDevice btDevice;

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
            }
            catch (Exception ex)
            {
                //rootPage.NotifyUser(ex.Message, NotifyType.ErrorMessage);
                //ResetMainUI();
                throw new Exception("Bluetooth Device の取得に失敗しました。",ex);
            }
            // If we were unable to get a valid Bluetooth device object,
            // it's most likely because the user has specified that all unpaired devices
            // should not be interacted with.
            if (btDevice == null)
            {
                throw new NullReferenceException("Bluetooth Device が空です。");
            }

            // This should return a list of uncached Bluetooth services (so if the server was not active when paired, it will still be detected by this call
            var rfcommServices = await btDevice.GetRfcommServicesForIdAsync(
                RfcommServiceId.FromUuid(Constants.RfcommChatServiceUuid), BluetoothCacheMode.Uncached);

            if (rfcommServices.Services.Count > 0)
            {
                BTDeviceService = rfcommServices.Services[0];
            }
            else
            {
                throw new NullReferenceException("対象のデバイスにBluetoothサービスが一つも見つかりません。正しい機器に接続していない可能性があります。");
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

            //StopWatcher();

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
            }
            catch (Exception ex) when ((uint)ex.HResult == 0x80070490) // ERROR_ELEMENT_NOT_FOUND
            {
                throw new NullReferenceException("ソケットのオープンに失敗しました。対象のデバイスでアプリケーションが起動されていることをご確認ください。(0x80070490: ERROR_ELEMENT_NOT_FOUND)", ex);
            }
            catch (Exception ex) when ((uint)ex.HResult == 0x80072740) // WSAEADDRINUSE
            {
                throw new InvalidOperationException("ソケットのオープンに失敗しました。対象のデバイスがすでに他のサーバーに接続されている可能性があります。(0x80072740: WSAEADDRINUSE)", ex);
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
