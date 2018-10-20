using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        public BTInitEventArgs(string hostName, bool isHeadDetected)
        {
            ConnectionHostName = hostName;
            IsHeadDetected = isHeadDetected;
        }
        public string ConnectionHostName { get; set; }
        public bool IsHeadDetected { get; set; }
    }

    public class BTMessageRecievedEventArgs : EventArgs
    {
        public BTMessageRecievedEventArgs(string message)
        {
            RecievedMessage = message;
        }
        public string RecievedMessage { get; set; }
    }

    public class BTClient
    {
        DeviceWatcher deviceWatcher;
        Page mainPage;

        DeviceInformation FantasmicHead;

        public ObservableCollection<DeviceInformation> DeviceInfoCollection { get; set; }

        public BTClient(Page mainPage)
        {
            this.mainPage = mainPage;
            DeviceInfoCollection = new ObservableCollection<DeviceInformation>();
            FantasmicHead = null;
        }

        /*Events*/
        public event EventHandler InitializeCompleted;
        public event EventHandler MessageRecieved;
        public event EventHandler DeviceAdded;

        protected async virtual void OnInitializeCompleted(BTInitEventArgs e)
        {
            await mainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate
            {
                InitializeCompleted?.Invoke(this, e);
            });
        }

        protected async virtual void OnDeviceAdded(BTInitEventArgs e)
        {
            await mainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate
            {
                DeviceAdded?.Invoke(this, e);
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
                OnDeviceAdded(new BTInitEventArgs("found: " + deviceInfo.Name, false));
            });
        }

        private void deviceWatcher_EnumerationCompleted(DeviceWatcher sender, object args)
        {
            bool isDetected = false;
            var deviceInfo = DeviceInfoCollection.Where((x) => x.Name == "FANTASMICHEAD").FirstOrDefault();
            if (deviceInfo != null)
            {
                isDetected = true;
                FantasmicHead = deviceInfo;
                var btRW = new BTReaderWriter(FantasmicHead);
                ConnectReciever(btRW);
            }
            OnInitializeCompleted(new BTInitEventArgs("found: " + DeviceInfoCollection.Count.ToString() + " devices.", isDetected));
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

        public async void ConnectReciever(BTReaderWriter btRW)
        {
            try
            {
                await btRW.ConnectBTService();
                ReceiveStringLoop(btRW);
            }
            catch (NullReferenceException ex)
            {
                Debug.WriteLine(ex.Message + " : BTサービス探索をリトライします。");
                ConnectReciever(btRW);
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine(ex.Message + ":" + ex.InnerException.Message);
                ConnectReciever(btRW);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("不明なエラー: " + ex.Message + ":" + ex.InnerException?.Message);
                ConnectReciever(btRW);
            }
        }

        private async void ReceiveStringLoop(BTReaderWriter btRW)
        {
            var chatReader = btRW.BTReader;
            Debug.WriteLine("/////entering recieve string roop/////");
            try
            {
                uint size = await chatReader.LoadAsync(sizeof(uint));
                Debug.WriteLine("size check in ReceiveStringLoop");
                if (size < sizeof(uint))
                {
                    //Disconnect();
                    //return;
                    ReceiveStringLoop(btRW);
                }

                Debug.WriteLine("ReadUInt32 in ReceiveStringLoop");
                uint stringLength = chatReader.ReadUInt32();

                Debug.WriteLine("LoadAsync in ReceiveStringLoop");
                uint actualStringLength = await chatReader.LoadAsync(stringLength);
                if (actualStringLength != stringLength)
                {
                    // The underlying socket was closed before we were able to read the whole data
                    throw new InvalidOperationException("The underlying socket was closed before we were able to read the whole data");
                }

                //ConversationList.Items.Add("Received: " + chatReader.ReadString(stringLength));
                //TODO: 受信した文字列のハンドル
                String resultString = chatReader.ReadString(stringLength);
                OnMessageRecieved(new BTMessageRecievedEventArgs(resultString));
                Debug.WriteLine(resultString);

                ReceiveStringLoop(btRW);
            }
            catch (ObjectDisposedException ex)
            {
                Debug.WriteLine("BT接続がDisposeされています。再接続を試みます。: " + ex.Message);
                //disposedReaderWriters.Add(btReaderWriter);
                await btRW.ConnectBTService();
                ReceiveStringLoop(btRW);
            }
            catch (Exception ex)
            {
                lock (this)
                {
                    if (btRW.BTStreamSocket == null)
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
                        btRW.Disconnect();
                        Debug.WriteLine("サーバーとの接続が切断されました。: " + ex.Message + ":::" + ex.HResult);
                        //throw new Exception("サーバーとの接続が解除されました。", ex);
                    }
                }
            }
        }
    }
}
