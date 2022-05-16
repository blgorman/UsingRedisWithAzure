using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Text.Json;
using rpsm = RedisPubSubModels;

namespace RedisPubSubConsumer
{
    public class Program
    {
        private static IConfigurationRoot _configuration;
        private const string MessageChannel = "sampleorders";

        public static async Task Main(string[] args)
        {
            BuildOptions();

            var connectionString = _configuration["Redis:ConnectionString"];
            Console.WriteLine("Reading orders from channel:");

            using (var cache = ConnectionMultiplexer.Connect(connectionString))
            {
                var sub = cache.GetSubscriber().Subscribe(MessageChannel);
                sub.OnMessage(message => MessageAction((string)message.Message));
                Console.ReadLine();
            }
        }

        private static void MessageAction(string message)
        {

            Console.WriteLine($"Received: {message}");

            if (!string.IsNullOrWhiteSpace(message))
            {
                var data = message.ToString();
                if (data.Contains("OrderTotal"))
                {
                    var order = JsonSerializer.Deserialize<rpsm.Order>(data);
                    if (order != null)
                    {
                        Console.WriteLine($"Order Total: {order.OrderTotal}");
                    }
                }
            }

        }
    

        private static void BuildOptions()
        {
            _configuration = ConfigurationBuilderSingleton.ConfigurationRoot;
        }
    }
}
