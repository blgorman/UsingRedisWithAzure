using DN6SimpleWebWithAuth;
using DN6SimpleWebWithAuth.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseSqlServer(connectionString)
    .Options;
using (var context = new ApplicationDbContext(contextOptions))
{
    context.Database.Migrate();
}

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["APPINSIGHTS_CONNECTIONSTRING"]);

/* Add Redis */

string redisCNSTR = string.Empty;
var env = builder.Configuration["Application:Environment"];

if (string.IsNullOrWhiteSpace(env) || !env.Equals("develop", StringComparison.OrdinalIgnoreCase))
{
    redisCNSTR = builder.Configuration["REDIS_CONNECTION_STRING"];
}
else
{
    var redisSection = builder.Configuration.GetSection("Redis");
    redisCNSTR = redisSection.GetValue<string>("ConnectionString").ToString();
    var redisInstanceName = redisSection.GetValue<string>("InstanceName");
}

//Turn this on to use session data in Redis and avoid using cookies for logins
////session
//builder.Services.AddSession(o =>
//{
//    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
//    o.Cookie.Name = "DemoRedis.Session";
//    o.Cookie.HttpOnly = true;
//});

//builder.Services.AddStackExchangeRedisCache(options =>
//{
//    options.Configuration = builder.Configuration.GetConnectionString(redisCNSTR);
//    options.InstanceName = redisInstanceName;
//});

//Direct access to the cache
builder.Services.AddSingleton(async x => await RedisConnection.InitializeAsync(connectionString: redisCNSTR));


/* NOTE: To enable the use of AppConfiguration and Keyvault integration, uncomment the following code,
 *          and ENSURE you have a system managed identity for both app services at Azure,
 *          then also add all of the app service and slots to the App Configuration with Configuration Data Reader Role
 * Additional NOTE: If using KeyVault, ensure additional identity on Azure App Config, and then ensure all of the identities can get keyvault secret
 *                      through access policies on your keyvault for Get Secret for App Config, App Service, and all App Service Slots.
 *                  ,Then uncomment Keyvault code
 *                  You will also need to remove the `); from the connect line to chain the command, and push.
 *                  also note that your devs will also need access to keyvault get secret in order to work with this locally through default credential
 * Final Note: For the code to work, ensure you add NuGet packages for Azure.Identity and Microsoft.Extensions.Configuration.AzureAppConfiguration

builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
{
    var settings = config.Build();
    var env = settings["Application:Environment"];
    if (env == null || !env.Trim().Equals("develop", StringComparison.OrdinalIgnoreCase))
    {
        var cred = new ManagedIdentityCredential();
        config.AddAzureAppConfiguration(options =>
                options.Connect(new Uri(settings["AzureAppConfigConnection"]), cred));
                            //.ConfigureKeyVault(kv => { kv.SetCredential(cred); }));
    }
    else
    {
        var cred = new DefaultAzureCredential();
        config.AddAzureAppConfiguration(options =>
            options.Connect(settings["AzureAppConfigConnection"]));
                   //.ConfigureKeyVault(kv => kv.SetCredential(cred)));
    }
});
*/

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

////turn this on to use session data in redis
//app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
