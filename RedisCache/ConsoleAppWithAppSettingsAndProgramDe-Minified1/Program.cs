using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RedisCacheDemo;
using ServiceStack.Redis;
using StackExchange.Redis;

namespace ConsoleAppWithAppSettingsAndProgramDe_Minified1
{
    public class Program
    {
        private static IConfigurationRoot _configuration;
        private static readonly string connectionString;

        public static async Task Main(string[] args)
        {
            BuildOptions();
            Console.WriteLine("Hello World");

            var connectionString = _configuration["Redis:ConnectionString"];
            Console.WriteLine($"Configuration: {connectionString}");

            using (var cache = ConnectionMultiplexer.Connect(connectionString))
            {
                //db
                IDatabase db = cache.GetDatabase();

                //commands
                var result = await db.ExecuteAsync("ping");
                Console.WriteLine($"PING = {result.Type} : {result}");

                bool setValue = await db.StringSetAsync("test:key", "100");
                Console.WriteLine($"SET: {setValue}");

                // StringGetAsync takes the key to retrieve and return the value
                string getValue = await db.StringGetAsync("test:key");
                Console.WriteLine($"GET: {getValue}");

                Employee e007 = new Employee("007", "James Bond", 42);
                Console.WriteLine("Cache response from storing Employee .NET object : " +
                    await db.StringSetAsync("e007", JsonConvert.SerializeObject(e007)));

                // Retrieve .NET object from cache
                var employee = JsonConvert.DeserializeObject<Employee>(await db.StringGetAsync("e007"));
                Console.WriteLine("Deserialized Employee .NET object :\n");
                Console.WriteLine("\tEmployee.Name : " + employee.Name);
                Console.WriteLine("\tEmployee.Id   : " + employee.Id);
                Console.WriteLine("\tEmployee.Age  : " + employee.Age + "\n");


                //use transactions:
                var data = new Dictionary<string, string>();
                var guid1 = "26548860-6a55-4184-881b-1d84b61d253e";
                var guid2 = "7b5a99e0-6177-4358-8f88-d5a895c5bdeb";

                data.Add(guid1.ToString(), "The first entry");
                data.Add(guid2.ToString(), "The second entry");

                var success = UtilizeTransactions(data, 10);
                if (success)
                {
                    foreach (var item in data)
                    {
                        var value = await db.StringGetAsync(item.Key);
                        Console.WriteLine($"The next item is {item.Key} with value {value}");
                    }
                }
            }
        }
        private static bool UtilizeTransactions(Dictionary<string, string> data, int expirationSeconds)
        {
            bool transactionResult = false;

            var redisConnectionString = _configuration["Redis:RedisConnectionString"];
            using (RedisClient redisClient = new RedisClient(redisConnectionString))
            {
                using (var transaction = redisClient.CreateTransaction())
                {
                    //Add multiple operations to the transaction
                    foreach (var item in data)
                    {
                        transaction.QueueCommand(c => c.Set(item.Key, item.Value));
                        if (expirationSeconds > 0)
                        {
                            transaction.QueueCommand(c => ((RedisNativeClient)c).Expire(item.Key, expirationSeconds));
                        }
                    }

                    //Commit and get result of transaction
                    transactionResult = transaction.Commit();
                }
            }

            Console.WriteLine(transactionResult ? "Transaction committed" : "Transaction failed to commit");
            
            return transactionResult;
        }

        

        private static void BuildOptions()
        {
            _configuration = ConfigurationBuilderSingleton.ConfigurationRoot;
        }
    }
}
