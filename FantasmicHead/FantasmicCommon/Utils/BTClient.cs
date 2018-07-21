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

    public class BTMessageRecievedEventArgs : EventArgs
    {
        public BTMessageRecievedEventArgs(string message)
        {
            RecievedMessage = message;
        }
        public string RecievedMessage { get; set; }
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
        public event EventHandler MessageRecieved;

        protected async virtual void OnInitializeCompleted(BTInitEventArgs e)
        {
            await mainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate
            {
                InitializeCompleted?.Invoke(this, e);
            });
        }

        protected async virtual void OnMessageRecieved(BTMessageRecievedEventArgs e)
        {
            await mainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate
            {
                MessageRecieved?.Invoke(this, e);
            });
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
                OnInitializeCompleted(new BTInitEventArgs("found: " + deviceInfo.Name));
            });
        }

        private void deviceWatcher_EnumerationCompleted(DeviceWatcher sender, object args)
        {
            OnInitializeCompleted(new BTInitEventArgs("found: " + DeviceInfoCollection.Count.ToString() + " devices."));
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

        public async void ConnectReciever(DeviceInformation device)
        {
            var btRW = new BTReaderWriter(device);
            await btRW.ConnectBTService();
            ReceiveStringLoop(btRW.btReader);
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
                OnMessageRecieved(new BTMessageRecievedEventArgs(resultString));

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
}
