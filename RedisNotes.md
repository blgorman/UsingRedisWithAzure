# Important Things from Learn

The following are important things to know about Azure Cache for Redis as per Microsoft Learn.

## Introduction to Azure Cache for Redis

The first module is ["Introduction to Azure Cache for Redis"](https://docs.microsoft.com/en-us/learn/modules/intro-to-azure-cache-for-redis/).  

### What is it

Open-source caching solution. Key/Value datastore

Use to complement database apps.

### Four Pillars

Four pillars of a caching solution

- Performance
- Scalability
- Availability
- Support for Geographic distribution

### Offerings

Both open-source and enterprise levels are available.

### Patterns

- Data Cache [cache-aside].
- Content Cache [static content].
- Session Store [Instead of user cookies]
- Job and Message Queuing
- Distributed transactions

### Tiers

Basic: 1 VM, no distribution, no SLA
Standard: 2 VMs, same limits as basic, minimal SLA
Premium: 2 more powerful VMs
Enterprise: use Redis packages
Enterprise Flash: lower cost enterprise, uses non-volatile memory to save costs

All Tiers

- Data Encryption
- Network Isolation
- Scaling

Premium, Enterprise & Enterprise Flash:

- Clustering [HA and load distribution]
- Data persistence - snapshots and backups
- Zone Redundancy - Availability zones
- Geo replication - DR, regional failover
- Import/Export - integrate with storage

Enterprise only:

RediSearch
RedisBloom
RedisTimeSeries
Active Geo Replication

### How it works

Distributed Cache
- Manage spices
- Cache aside [data]
- Static Content
- Geo positioning [like a CDN]

Session Store
- 100000s of users
- reliability via data-replication
- shopping carts/cookies/login and session state/IoT Telemetry

Message Broker
- Pub/Sub
- Queue

Cloud Migration 
- Import and Export files
- Modernize your applications

## Develop for Azure Cache for Redis  

The second module is ["Develop for Azure Cache for Redis"](https://docs.microsoft.com/en-us/learn/modules/develop-for-azure-cache-for-redis/).  

This will be the first development exercise.

### Key Scenarios

As above

### Tiers

As above

### Configuring at Azure

How to create a named instance at azure

- Globally unique name
- Numbers, letters, and `-` only. No start/end or consecutive `-` characters.
- Region
- Tier
- Premium + ==> VNet support
- Premium + ==> Clustering support

### Commands

There are a number of basic commands.

Ping => Pong
SetString => GetString
exists[key]
incr[key]
flushdb - delete all K/V pairs in the database.

### Expiring values

Caching needs to have a Time To Live (TTL) to be valid.  You can set the TTL to expire after a number of seconds.

### Cache Instance

At azure, the instance will have a url string similar to:

```c#
[your-cache-instance-name].redis.cache.windows.net:6380
```  

A typical connection string with `StackExchance.Redis` will look similar to the following:  

```c#
[your-cache-instance-name].redis.cache.windows.net:6380,password=[your-password-here],ssl=True,abortConnect=False
```  

### Composing the object

The code to connect composes the object, starting with the database conneciotn, then modifying K/V pairs.

First, get the connection:

```c#
using StackExchange.Redis;
...
var connectionString = "your-cn-string-see-above";
var redisConnection = ConnectionMultiplexer.Connect(connectionString);
```  

With the connection, get the database:

```c#
IDatabase myRedisDatabase = redisConnection.GetDatabase();
```  

Then with the db, get and set values:

```c#
bool kvSet = myRedisDatabase.StringSet("product:name", "Axis and Allies 1942 board game");
string value = myRedisDatabase.StringGet("product:name");
Console.WriteLine(value); 
```

Data can also be stored as byte arrays [binary values]  

```c#
byte[] key = ...;
byte[] value = ...;

db.StringSet(key, value);
byte[] value = myRedisDatabase.StringGet(key);
```  

Execute commands:

```c#
Console.WriteLine(myRedisDatabase.Execute("ping"));
```  

Serialize to JSON before and after:

```c#
var stat = new GameStat("Soccer", new DateTime(2019, 7, 16), "Local Game", 
                new[] { "Team 1", "Team 2" },
                new[] { ("Team 1", 2), ("Team 2", 1) });

string serializedValue = JsonConvert.SerializeObject(stat);
bool added = myRedisDatabase.StringSet("event:1950-world-cup", serializedValue);

var result = myRedisDatabase.StringGet("event:2019-local-game");
var stat = Newtonsoft.Json.JsonConvert.DeserializeObject<GameStat>(result.ToString());
Console.WriteLine(stat.Sport);
```  

### QuickDemo

Use the RedisCacheDemo console app to demo basic C# functionality.

In addition to the app, do the following walkthrough:

[https://docs.microsoft.com/en-us/learn/modules/develop-for-azure-cache-for-redis/5-console-app-azure-cache-redis](Redis Cache)


## Optimize your web applications by caching read-only data with Redis

The third learn module is [Optimize your web applications by caching read-only data with Redis](https://docs.microsoft.com/en-us/learn/modules/optimize-your-web-apps-with-redis/)  

### Information about keys

Avoid Long keys.  Max size 512MB.

Make keys readable and descriptive, even if it means adding a few more characters.

For example:
movie:scifi:title:Inception

object:category:id:number

### How is data stored

Nodes and clusters

Node = space where data is stored
Cluster = 3+ Nodes, provides redundancy

### Architectures

- Single Node  [Basic Tier]
- Multiple Node [Standard Tier]
- Clustered [Premium Tier +]
  
### Configure and deploy Cache

Go through deployment in Azure.

Persistence:

- None
- RDB
- AOF


### Live Exercise

https://docs.microsoft.com/en-us/learn/modules/optimize-your-web-apps-with-redis/4-exercise-create-redis-cache?pivots=csharp

- Create the cache
- Write .Net code to execute against it [same as the first exercise really, just has an Azure Sandbox so you can play without a subscription.


## Work with mutable and partial data in Azure Cache for Redis

The fourth learn module is [Work with mutable and partial data in Azure Cache for Redis](https://docs.microsoft.com/en-us/learn/modules/work-with-mutable-and-partial-data-in-a-redis-cache/)  

### Transactions

Utilize ServiceStack.Redis in .Net

- CreateTransaction()
- QueueCommand()
- Commit()

Get the connection string:

```C#
rg=rg-name-here
redis_name=redis-cache-name-here

redis_key=$(az redis list-keys \
    --name "$redis_name" \
    --resource-group $rg \
    --query primaryKey \
    --output tsv)

echo $redis_key
echo "$redis_key"@"$redis_name".redis.cache.windows.net:6380?ssl=true
```  

Use the connection string:

```c#
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
``` 

>**Note:** Code for the expiration is included above to avoid repetitive posting.

#### Expirations

If you want, you can set keys to expire after a certain number of seconds.  See the code sample ablve.

#### Eviction policies and Memory Management

Azure provides a number of eviction policies to handle working with Redis when you are out of memory.  

| Title | Purpose |   
|--|--|
| **noeviction** | No eviction - errors are thrown when you are out of memory |
| **allkeys-lru** | Looks at all keys and removes the least-recently-used key |  
| **allkeys-random** | Looks at all keys and removes one at random |
| **allkeys-lfu** | Looks at all keys and removes the least-frequently-used key |  
| **volatile-lru** | Looks at all keys with expiration set and removes the least-recently-used of these keys |  
| **volatile-ttl** | Looks at all keys with expiration set and removes the key from these with the least remaining time to live |  
| **volatile-random** | Looks at all keys with expiration set and removes a key from these at random |
| **volatile-lfu** | Looks at all keys with expiration set and removes the least-frequently used key from these |   

#### Cache-Aside

Put data from your database into REDIS to avoid having to make a trip back to the database for it.

For example, the states of the United States.  You don't need to modify this data much, if ever.  It just needs to be available.  You can use in-memory cache, but you can also just as easily use Redis.

When doing cache-aside, ensure that you evaluate and plan for the following:

- Lifetime: Make sure that the cache lifetime makes sense.  Use shorter lifetimes for volatile data, longer lifetimes for data that changes infrequently.  

- Evicting: Ensure the correct data will be the first to be evicted (see above)  

- Priming: It is often useful to pre-load some of the cache.  You will want to consider what to prime and how to prime it.

- Consistency: Consider the cost of dirty reads and how you might work to prevent stale data and ensure conflicts are handled appropriately.  

## Improve session scalability in a .NET Framework ASP.NET web application by using Azure Cache for Redis

The fifth learn module is [Improve session scalability in a .NET Framework ASP.NET web application by using Azure Cache for Redis](https://docs.microsoft.com/en-us/learn/modules/aspnet-session/)  

### Practical uses

Storing session state in an app service using Redis can make your application much more scalable.  Additionally, you can leverage this pattern for migration of legacy applications that are storing much larger state values (Viewstate anyone?).  

By using Redis, you therefore solve two problems:

- Large sessions are easily managed without memory issues that might be typically encountered
- Session state just works across a web farm (instances).

### Set up application

In order to leverage Azure Redis for session state, you need to set up the application.  This repo has an accompanying web application that leverages  Redis to utilize session state and cache-aside for a dn6 web application.  

Although this is covered for legacy .Net apps in Learn, the latest follows this pattern:

https://docs.microsoft.com/en-us/aspnet/core/performance/caching/distributed?view=aspnetcore-6.0#distributed-redis-cache

An additional NuGet package is needed:

```powershell
Install-Package Microsoft.Extensions.Caching.StackExchangeRedis
```

Then add the following code to the Program.cs file (in the minimal API .Net 6, there is no Startup.cs file)

```C#
/* Add Redis */
//assumes you added something to local config (secrets!)/app configuration at Azure
var redisSection = builder.Configuration.GetSection("Redis");
var redisCNSTR = redisSection.GetValue<string>("ConnectionString").ToString();
var instanceName = redisSection.GetValue<string>("InstanceName");

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString(redisCNSTR);
    options.InstanceName = "SampleInstance";
});
 ```  

Additionally, code from this sample is modified:

[Azure Samples -- Redis Cache](https://github.com/Azure-Samples/azure-cache-redis-samples/blob/main/quickstart/aspnet-core/ContosoTeamStats/RedisConnection.cs)


## Implement Pub/Sub and Streams in Azure Cache for Redis

The final learn module is [Implement Pub/Sub and Streams in Azure Cache for Redis](https://docs.microsoft.com/en-us/learn/modules/azure-redis-publish-subscribe-streams/)  

Additional resources

- [StackExchange.Redis on GitHub](https://stackexchange.github.io/StackExchange.Redis/PubSubOrder.html)  

### Purpose

Use Pub/Sub to break up workloads into distributed microservices, or just offset and separate the performance of one app from another (i.e. allow many streams of data to be input from the front-end and then throttle the back-end processing without having to pay any penalty on the front-end).

### Channels

For Pub/Sub, you create a channel and then your clients can subscribe to it.

In addition to directly subscribing to a channel, you can subscribe to channels that match a pattern.

- `?` : Single Character
- `*` : Any Content
- `[]` : Chars in the list

### Publishing

Push messages to the channel

```C#

```  
### Message Order

Queue functionality guarantees order

```c#
var channel = multiplexer.GetSubscriber().Subscribe("messages");
channel.OnMessage(message =>
{
    Console.WriteLine((string)message.Message);
});
```  

Whereas concurrent processing has no order guarantee

```c#
var channel = multiplexer.GetSubscriber().Subscribe("messages", (channel, message) => {
    Console.WriteLine((string)message);
});
```  

Using this, you can create a pub/sub architecture.


