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
        private const string _deviceConnectionString = "{device connection string}";
        private static DeviceClient _deviceClient;

        static void Main(string[] args)
        {
            Console.WriteLine("Simulated device\n");
            _deviceClient = DeviceClient.CreateFromConnectionString(_deviceConnectionString, TransportType.Mqtt);

            SendDeviceToCloudMessageAsync();
            ReceiveC2DAsync();
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


        //在设备收到消息时，ReceiveAsync 方法以异步方式返回收到的消息。 
        //它在可指定的超时期限过后返回 null（在本例中，使用的是默认值一分钟）。 
        //当应用收到 null 时，它应继续等待新消息。
        //https://docs.microsoft.com/zh-cn/azure/iot-hub/iot-hub-csharp-csharp-c2d
        private static async void ReceiveC2DAsync()
        {
            Console.WriteLine("\nReceiving cloud to device message from service");
            while (true)
            {
                var receiveMessage = await _deviceClient.ReceiveAsync();
                if (receiveMessage == null) continue;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Received message:{Encoding.ASCII.GetString(receiveMessage.GetBytes())}");
                Console.ResetColor();

                //将通知 IoT 中心，指出已成功处理消息。 可以安全地从设备队列中删除该消息。 
                //如果因故导致设备应用无法完成消息处理作业，IoT 中心将再传递一次。
                await _deviceClient.CompleteAsync(receiveMessage);
            }
        }
    }
}
