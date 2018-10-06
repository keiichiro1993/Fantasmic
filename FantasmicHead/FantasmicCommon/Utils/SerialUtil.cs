using FantasmicCommon.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private List<DataWriter> dataWriters;

        public SerialUtil()
        {
            serialDevices = new List<SerialDevice>();
            dataWriters = new List<DataWriter>();
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
                    dataWriters.Add(new DataWriter(serialDevice.OutputStream));
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
        }

        public async Task SendData(Scene scene)
        {
            string message = "Request Change:Scene" + (int)scene.CurrentScene + ":Mode" + scene.CurrentSequence + "\n";
            Debug.WriteLine("Serial send: " + message);
            foreach (var dataWriter in dataWriters)
            {
                try
                {
                    dataWriter.WriteString(message);
                    await dataWriter.StoreAsync();
                }
                catch (Exception ex)
                {
                    throw new Exception("error: " + ex.Message);
                }
            }
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
