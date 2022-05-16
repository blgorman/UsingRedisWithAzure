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

                //states
                await PrimeAndUseStates(db);
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

        private static async Task PrimeAndUseStates(IDatabase db)
        {
            //determine if have cache:
            string curCacheStates = await db.StringGetAsync("States");
            Console.WriteLine($"GET: {curCacheStates}");

            if (string.IsNullOrWhiteSpace(curCacheStates))
            {
                var states = GetStates();
                string statesJSON = JsonConvert.SerializeObject(states);

                Console.WriteLine("Cache response from storing States : " +
                        await db.StringSetAsync("States", statesJSON));

                curCacheStates = statesJSON;
            }

            var statesDataFromCache = JsonConvert.DeserializeObject<List<State>>(curCacheStates);

            foreach (var s in statesDataFromCache)
            {
                Console.WriteLine($"New State {s.Id} | {s.Abbreviation} | {s.Name}");
            }

            Console.WriteLine("States data priming completed");
        }

        private static List<State> GetStates()
        {
            return new List<State>()
            {
                new State() { Id = 1, Name = "Alabama", Abbreviation = "AL" },
                    new State() { Id = 2, Name = "Alaska", Abbreviation = "AK" },
                    new State() { Id = 3, Name = "Arizona", Abbreviation = "AZ" },
                    new State() { Id = 4, Name = "Arkansas", Abbreviation = "AR" },
                    new State() { Id = 5, Name = "California", Abbreviation = "CA" },
                    new State() { Id = 6, Name = "Colorado", Abbreviation = "CO" },
                    new State() { Id = 7, Name = "Connecticut", Abbreviation = "CT" },
                    new State() { Id = 8, Name = "Delaware", Abbreviation = "DE" },
                    new State() { Id = 9, Name = "District of Columbia", Abbreviation = "DC" },
                    new State() { Id = 10, Name = "Florida", Abbreviation = "FL" },
                    new State() { Id = 11, Name = "Georgia", Abbreviation = "GA" },
                    new State() { Id = 12, Name = "Hawaii", Abbreviation = "HI" },
                    new State() { Id = 13, Name = "Idaho", Abbreviation = "ID" },
                    new State() { Id = 14, Name = "Illinois", Abbreviation = "IL" },
                    new State() { Id = 15, Name = "Indiana", Abbreviation = "IN" },
                    new State() { Id = 16, Name = "Iowa", Abbreviation = "IA" },
                    new State() { Id = 17, Name = "Kansas", Abbreviation = "KS" },
                    new State() { Id = 18, Name = "Kentucky", Abbreviation = "KY" },
                    new State() { Id = 19, Name = "Louisiana", Abbreviation = "LA" },
                    new State() { Id = 20, Name = "Maine", Abbreviation = "ME" },
                    new State() { Id = 21, Name = "Maryland", Abbreviation = "MD" },
                    new State() { Id = 22, Name = "Massachusetts", Abbreviation = "MS" },
                    new State() { Id = 23, Name = "Michigan", Abbreviation = "MI" },
                    new State() { Id = 24, Name = "Minnesota", Abbreviation = "MN" },
                    new State() { Id = 25, Name = "Mississippi", Abbreviation = "MS" },
                    new State() { Id = 26, Name = "Missouri", Abbreviation = "MO" },
                    new State() { Id = 27, Name = "Montana", Abbreviation = "MT" },
                    new State() { Id = 28, Name = "Nebraska", Abbreviation = "NE" },
                    new State() { Id = 29, Name = "Nevada", Abbreviation = "NV" },
                    new State() { Id = 30, Name = "New Hampshire", Abbreviation = "NH" },
                    new State() { Id = 31, Name = "New Jersey", Abbreviation = "NJ" },
                    new State() { Id = 32, Name = "New Mexico", Abbreviation = "NM" },
                    new State() { Id = 33, Name = "New York", Abbreviation = "NY" },
                    new State() { Id = 34, Name = "North Carolina", Abbreviation = "NC" },
                    new State() { Id = 35, Name = "North Dakota", Abbreviation = "ND" },
                    new State() { Id = 36, Name = "Ohio", Abbreviation = "OH" },
                    new State() { Id = 37, Name = "Oklahoma", Abbreviation = "OK" },
                    new State() { Id = 38, Name = "Oregon", Abbreviation = "OR" },
                    new State() { Id = 39, Name = "Pennsylvania", Abbreviation = "PA" },
                    new State() { Id = 40, Name = "Rhode Island", Abbreviation = "RI" },
                    new State() { Id = 41, Name = "South Carolina", Abbreviation = "SC" },
                    new State() { Id = 42, Name = "South Dakota", Abbreviation = "SD" },
                    new State() { Id = 43, Name = "Tennessee", Abbreviation = "TN" },
                    new State() { Id = 44, Name = "Texas", Abbreviation = "TX" },
                    new State() { Id = 45, Name = "Utah", Abbreviation = "UT" },
                    new State() { Id = 46, Name = "Vermont", Abbreviation = "VT" },
                    new State() { Id = 47, Name = "Virginia", Abbreviation = "VA" },
                    new State() { Id = 48, Name = "Washington", Abbreviation = "WA" },
                    new State() { Id = 49, Name = "West Virginia", Abbreviation = "WV" },
                    new State() { Id = 50, Name = "Wisconsin", Abbreviation = "WI" },
                    new State() { Id = 51, Name = "Wyoming", Abbreviation = "WY" }
            };
        }

        private static void BuildOptions()
        {
            _configuration = ConfigurationBuilderSingleton.ConfigurationRoot;
        }
    }
}
