using Microsoft.Extensions.Configuration;
using rpsm = RedisPubSubModels;
using StackExchange.Redis;
using System.Text.Json;

namespace RedisPubSubAndStreaming
{
    public class Program
    {
        private static IConfigurationRoot _configuration;
        private const string MessageChannel = "sampleorders";

        public static async Task Main(string[] args)
        {
            BuildOptions();
            
            var order = await BuildSampleOrder();
            var orderJSON = JsonSerializer.Serialize(order);

            var connectionString = _configuration["Redis:ConnectionString"];
            Console.WriteLine($"Sending Sample Order to Redis pub/sub: {orderJSON}");

            using (var cache = ConnectionMultiplexer.Connect(connectionString))
            {
                var pub = cache.GetSubscriber();

                pub.Publish(MessageChannel, orderJSON);

                for (int i = 0; i < 10; i++)
                {
                    pub.Publish(MessageChannel, $"Message {i}");
                    Console.WriteLine($"Published Message {i}");
                    Thread.Sleep(2000);
                }
                Thread.Sleep(10000);
                Console.WriteLine("Order data published");
                Console.ReadLine();
            }
        }

        private static async Task<rpsm.Order> BuildSampleOrder()
        {
            var p1 = new rpsm.Product() { Id = 1, Name = "Bike" };
            var p2 = new rpsm.Product() { Id = 2, Name = "Car" };
            var p3 = new rpsm.Product() { Id = 3, Name = "Toy" };

            var li1 = new rpsm.LineItem() { Id = 27, PricePerItem = 104.99, ProductId = 1, Quantity = 2 };
            var li2 = new rpsm.LineItem() { Id = 28, PricePerItem = 25317.86, ProductId = 2, Quantity = 1 };
            var li3 = new rpsm.LineItem() { Id = 29, PricePerItem = 15.99, ProductId = 3, Quantity = 7 };

            var o = new rpsm.Order() { Id = 253, LineItems = new List<rpsm.LineItem> { li1, li2, li3 }, OrderDate = DateTime.Now };
            return o;
        }

        private static void BuildOptions()
        {
            _configuration = ConfigurationBuilderSingleton.ConfigurationRoot;
        }
    }
}
