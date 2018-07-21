﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace FantasmicCommon.Utils
{
    public class BTServer
    {

        private RfcommServiceProvider serviceProvider;
        private StreamSocketListener socketListener;

        private List<BTReaderWriter> btReaderWriters { get; set; }

        //TODO: writerをリストにして複数デバイス管理

        /// <summary>
        /// Initializes the server using RfcommServiceProvider to advertise the Chat Service UUID and start listening
        /// for incoming connections.
        /// </summary>
        public async Task InitializeRfcommServer()
        {
            btReaderWriters = new List<BTReaderWriter>();
            try
            {
                serviceProvider = await RfcommServiceProvider.CreateAsync(RfcommServiceId.FromUuid(Constants.RfcommChatServiceUuid));
            }
            // Catch exception HRESULT_FROM_WIN32(ERROR_DEVICE_NOT_AVAILABLE).
            catch (Exception ex) when ((uint)ex.HResult == 0x800710DF)
            {
                // The Bluetooth radio may be off.
                /*rootPage.NotifyUser("Make sure your Bluetooth Radio is on: " + ex.Message, NotifyType.ErrorMessage);
                ListenButton.IsEnabled = true;
                DisconnectButton.IsEnabled = false;
                */
                throw new InvalidOperationException("デバイスの設定で Bluetooth が有効になっていることをご確認ください。", ex);
            }


            // Create a listener for this service and start listening
            socketListener = new StreamSocketListener();
            socketListener.ConnectionReceived += OnConnectionReceived;
            var rfcomm = serviceProvider.ServiceId.AsString();

            await socketListener.BindServiceNameAsync(serviceProvider.ServiceId.AsString(),
                SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication);

            // Set the SDP attributes and start Bluetooth advertising
            InitializeServiceSdpAttributes(serviceProvider);

            try
            {
                serviceProvider.StartAdvertising(socketListener, true);
            }
            catch (Exception ex)
            {
                // If you aren't able to get a reference to an RfcommServiceProvider, tell the user why.  Usually throws an exception if user changed their privacy settings to prevent Sync w/ Devices.  
                /*rootPage.NotifyUser(e.Message, NotifyType.ErrorMessage);
                ListenButton.IsEnabled = true;
                DisconnectButton.IsEnabled = false;*/
                throw new Exception("サーバーを開始できませんでした。", ex);
            }
            //rootPage.NotifyUser("Listening for incoming connections", NotifyType.StatusMessage);
        }

        /// <summary>
        /// Creates the SDP record that will be revealed to the Client device when pairing occurs.  
        /// </summary>
        /// <param name="rfcommProvider">The RfcommServiceProvider that is being used to initialize the server</param>
        private void InitializeServiceSdpAttributes(RfcommServiceProvider rfcommProvider)
        {
            var sdpWriter = new DataWriter();

            // Write the Service Name Attribute.
            sdpWriter.WriteByte(Constants.SdpServiceNameAttributeType);

            // The length of the UTF-8 encoded Service Name SDP Attribute.
            sdpWriter.WriteByte((byte)Constants.SdpServiceName.Length);

            // The UTF-8 encoded Service Name value.
            sdpWriter.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            sdpWriter.WriteString(Constants.SdpServiceName);

            // Set the SDP Attribute on the RFCOMM Service Provider.
            rfcommProvider.SdpRawAttributes.Add(Constants.SdpServiceNameAttributeId, sdpWriter.DetachBuffer());
        }



        /// <summary>
        /// Invoked when the socket listener accepts an incoming Bluetooth connection.
        /// </summary>
        /// <param name="sender">The socket listener that accepted the connection.</param>
        /// <param name="args">The connection accept parameters, which contain the connected socket.</param>
        private async void OnConnectionReceived(
            StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            StreamSocket btStreamSocket;
            try
            {
                btStreamSocket = args.Socket;
            }
            catch (Exception ex)
            {
                /*await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    rootPage.NotifyUser(e.Message, NotifyType.ErrorMessage);
                });
                Disconnect();*/
                throw new Exception("新しいデバイスが見つかりましたが、Socketの取得に失敗しました。", ex);
            }

            // Note - this is the supported way to get a Bluetooth device from a given socket
            var remoteDevice = await BluetoothDevice.FromHostNameAsync(btStreamSocket.Information.RemoteHostName);

            var writer = new DataWriter(btStreamSocket.OutputStream);
            var reader = new DataReader(btStreamSocket.InputStream);

            var btReaderWriter = new BTReaderWriter(reader, writer);
            btReaderWriters.Add(btReaderWriter);
        }

        public void SendMessage(String message)
        {
            Parallel.ForEach(btReaderWriters, async btReaderWriter =>
            {
                btReaderWriter.btWriter.WriteUInt32((uint)message.Length);
                btReaderWriter.btWriter.WriteString(message);
                await btReaderWriter.btWriter.StoreAsync();
            });
        }
    }
}
