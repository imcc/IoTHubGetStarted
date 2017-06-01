using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;

namespace CreateDeviceIdentity
{
    //https://docs.microsoft.com/zh-cn/azure/iot-hub/iot-hub-csharp-csharp-getstarted
    //创建设备标识
    class Program
    {
        static RegistryManager _registryManager;
        static readonly string _connectionString = ConfigurationManager.ConnectionStrings["mqtt"].ConnectionString;

        static void Main(string[] args)
        {
            _registryManager = RegistryManager.CreateFromConnectionString(_connectionString);
            AddDeviceAsync().Wait();
            Console.ReadKey(false);
        }

        private static async Task AddDeviceAsync()
        {
            var deviceid = "Device01";
            Device device;
            try
            {
                device = await _registryManager.AddDeviceAsync(new Device(deviceid));
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await _registryManager.GetDeviceAsync(deviceid);
            }

            Console.WriteLine($"Device key:{device.Authentication.SymmetricKey.PrimaryKey}");
        }
    }
}
