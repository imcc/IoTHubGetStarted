using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace SimulatedDevice
{
    //创建模拟设备应用程序
    //https://docs.microsoft.com/zh-cn/azure/iot-hub/iot-hub-csharp-csharp-getstarted
    class Program
    {
        private const string _deviceConnectionString = "HostName=IoT-Free-Demo.azure-devices.net;DeviceId=Device01;SharedAccessKey=GpYaoEW9QtfPDobUwucJH1K7L13LDfrt8Xhke0qWt1A=";
        private static DeviceClient _deviceClient;

        static void Main(string[] args)
        {
            Console.WriteLine("Simulated device\n");
            _deviceClient = DeviceClient.CreateFromConnectionString(_deviceConnectionString, TransportType.Mqtt);

            SendDeviceToCloudMessageAsync();
            Console.ReadKey(false);
        }

        private static async void SendDeviceToCloudMessageAsync()
        {
            double minTemperature = 20, minHumidity = 60;
            var messageId = 1;
            var rand = new Random();

            while (true)
            {
                double currentTemperature = minTemperature + rand.NextDouble() * 15;
                double currentHumidity = minHumidity + rand.NextDouble() * 20;

                var telemetryDataPoint = new
                {
                    messageId = messageId++,
                    deviceId = "Device01",
                    temperature = currentTemperature,
                    humidity = currentHumidity
                };

                var messageJson = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageJson));
                message.Properties.Add("temperatureAlert", currentTemperature > 30 ? "true" : "false");

                await _deviceClient.SendEventAsync(message);
                Console.WriteLine($"{DateTime.Now} > Sending message:{messageJson}");

                await Task.Delay(5000);
            }
        }
    }
}
