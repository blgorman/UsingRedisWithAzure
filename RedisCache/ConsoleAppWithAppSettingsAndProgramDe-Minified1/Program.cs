using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RedisCacheDemo;
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

            }
        }

        private static void BuildOptions()
        {
            _configuration = ConfigurationBuilderSingleton.ConfigurationRoot;
        }
    }
}
