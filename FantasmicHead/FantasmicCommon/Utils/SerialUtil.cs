using FantasmicCommon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

namespace FantasmicCommon.Utils
{
    public class SerialUtil
    {
        private List<SerialDevice> serialDevices;

        public SerialUtil()
        {
            serialDevices = new List<SerialDevice>();
        }

        public async Task InitSerial()
        {
            var devices = await DeviceInformation.FindAllAsync(SerialDevice.GetDeviceSelector());
            foreach (var device in devices)
            {
                SerialDevice serialDevice;
                try
                {
                    serialDevice = await SerialDevice.FromIdAsync(device.Id);
                    serialDevice.BaudRate = 9600;
                    serialDevice.DataBits = 8;
                    serialDevice.StopBits = SerialStopBitCount.One;
                    serialDevice.Parity = SerialParity.None;
                    serialDevice.Handshake = SerialHandshake.None;
                    serialDevice.ReadTimeout = TimeSpan.FromMilliseconds(1000);
                    serialDevice.WriteTimeout = TimeSpan.FromMilliseconds(1000);
                    serialDevices.Add(serialDevice);
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
        }

        public async Task<bool> SendData(Scene scene)
        {
            string message = "Request Change:Scene" + (int)scene.CurrentScene + ":Mode" + scene.CurrentMode + "\n";

            foreach (var serialDevice in serialDevices)
            {
                DataWriter dataWriteeObject = new DataWriter(serialDevice.OutputStream);
                dataWriteeObject.WriteString(message);
                await dataWriteeObject.StoreAsync();
                /*
                DataReader dataReaderObject = new DataReader(serialDevice.InputStream);
                await dataReaderObject.LoadAsync(128);
                uint bytesToRead = dataReaderObject.UnconsumedBufferLength;
                string receivedStrings = dataReaderObject.ReadString(bytesToRead);
                */
            }
            return true;
        }

        public async Task<string> ReceiveData()
        {
            string reply = "";
            while (true)
            {
                foreach (var serialDevice in serialDevices)
                {
                    DataReader dataReaderObject = new DataReader(serialDevice.InputStream);
                    await dataReaderObject.LoadAsync(128);
                    uint bytesToRead = dataReaderObject.UnconsumedBufferLength;
                    reply = dataReaderObject.ReadString(bytesToRead);

                    if (!String.IsNullOrEmpty(reply))
                    {
                        break;
                    }
                }

                if (!String.IsNullOrEmpty(reply))
                {
                    break;
                }
            }

            return reply;
        }
    }
}
