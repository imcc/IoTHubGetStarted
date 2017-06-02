using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;

namespace SendCloudToDevice
{
    //发送云到设备的消息
    //https://docs.microsoft.com/zh-cn/azure/iot-hub/iot-hub-csharp-csharp-c2d
    class Program
    {
        private static ServiceClient _serviceClient;
        private static string _connectionString = "{service connection string}";

        static void Main(string[] args)
        {
            Console.WriteLine("Send CLoud-to-Device message\n");
            _serviceClient = ServiceClient.CreateFromConnectionString(_connectionString);

            ReceiveFeedbackAsync();

            Console.WriteLine("Press any key to send a C2D message.");
            Console.ReadLine();

            SendCloudToDeviceMessageAsync().Wait();
            Console.ReadLine();
        }

        private static async Task SendCloudToDeviceMessageAsync()
        {
            var i = 0;
            while (i < 5)
            {
                var commandMessage = new Message(Encoding.ASCII.GetBytes($"Cloud to device message {i++}."));
                commandMessage.Ack = DeliveryAcknowledgement.Full;
                await _serviceClient.SendAsync("Device01", commandMessage);
            }
        }

        //此接收模式与用于从设备应用接收云到设备消息的模式相同
        private static async void ReceiveFeedbackAsync()
        {
            var feedbackReceiver = _serviceClient.GetFeedbackReceiver();

            Console.WriteLine("\nReceiving c2d feedback from service");
            while (true)
            {
                var feedbackBatch = await feedbackReceiver.ReceiveAsync();
                if (feedbackBatch == null) continue;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Received feedback:{string.Join(",", feedbackBatch.Records.Select(x => x.StatusCode))}");
                Console.ResetColor();

                await feedbackReceiver.CompleteAsync(feedbackBatch);
            }
        }
    }
}
