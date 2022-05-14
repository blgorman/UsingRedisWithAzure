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

### TODO

## Improve session scalability in a .NET Framework ASP.NET web application by using Azure Cache for Redis

The fifth learn module is [Improve session scalability in a .NET Framework ASP.NET web application by using Azure Cache for Redis](https://docs.microsoft.com/en-us/learn/modules/aspnet-session/)  

### TODO 

## Implement Pub/Sub and Streams in Azure Cache for Redis

The final learn module is [Implement Pub/Sub and Streams in Azure Cache for Redis](https://docs.microsoft.com/en-us/learn/modules/azure-redis-publish-subscribe-streams/)  

### TODO
