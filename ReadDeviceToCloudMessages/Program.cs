using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace ReadDeviceToCloudMessages
{
    //https://docs.microsoft.com/zh-cn/azure/iot-hub/iot-hub-csharp-csharp-getstarted
    //接收设备到云的消息
    class Program
    {
        //D2C:Device-to-Cloud

        private static readonly string _connectionString = ConfigurationManager.ConnectionStrings["mqtt"].ConnectionString;
        private static readonly string _iotHubD2cEndpoint = "messages/events";
        private static EventHubClient _eventHubClient;

        static void Main(string[] args)
        {
            Console.WriteLine("Receive message. Ctrl-C to exit.\n");

            _eventHubClient = EventHubClient.CreateFromConnectionString(_connectionString, _iotHubD2cEndpoint);
            var d2CPartitions = _eventHubClient.GetRuntimeInformation().PartitionIds;
            var cts = new CancellationTokenSource();

            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
                Console.WriteLine("Exiting...");
            };

            var tasks = new List<Task>();
            foreach (var partition in d2CPartitions)
            {
                tasks.Add(ReceiveMessageFromDeviceAsync(partition, cts.Token));
            }

            Task.WaitAll(tasks.ToArray());;
        }

        private static async Task ReceiveMessageFromDeviceAsync(string partition, CancellationToken ct)
        {
            //使用 EventHubReceiver 实例接收来自所有 IoT 中心设备到云接收分区的消息
            //创建 EventHubReceiver 对象时传递 DateTime.Now 参数的方式，使它仅接收启动后发送的消息。
            //此筛选器在测试环境中非常有用，因为这样可以看到当前的消息集。 在生产环境中，代码应确保处理所有消息。
            var eventHubReceiver = _eventHubClient.GetDefaultConsumerGroup().CreateReceiver(partition, DateTime.UtcNow);
            while (true)
            {
                if (ct.IsCancellationRequested) break;

                var eventData = await eventHubReceiver.ReceiveAsync();
                if (eventData == null) continue;

                var data = Encoding.UTF8.GetString(eventData.GetBytes());
                Console.WriteLine($"Message received. Partition:{partition} Data:'{data}'");
            }
        }
    }
}
