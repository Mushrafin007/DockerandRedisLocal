using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using Swashbuckle.AspNetCore;
using TestProject.Common;
using TestProject.MiddleWare;
using TestProject.Services.CrudApiService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddTransient<ICrudApiService, CrudApiService>();
builder.Services.AddTransient<CommonFunction>();
/*Rate limit config*/
//builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
//builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
//builder.Services.AddInMemoryRateLimiting();
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(
builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddInMemoryRateLimiting();
/*Rate limit config*/

/*Redis Config*/

var redisHost = builder.Configuration["ConnectionStringRedis"];
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect($"{redisHost},abortConnect=false"));
builder.Services.AddHostedService<RedisConsumerService>();
Console.WriteLine("Connected to Redis!");

/*Redis Config*/

builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo
//    {
//        Title = "TEST PROJECT DOCKER",
//        Version = "1.0"
//    });
//});
if (builder.Environment.IsDevelopment() || builder.Environment.EnvironmentName == "Docker")
{
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "TEST PROJECT DOCKER", Version = "v1" });
    });
}
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    //app.UseSwaggerUI(c =>
    //{
    //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TEST PROJECT DOCKER");
    //    c.RoutePrefix = ""; // Optional: loads Swagger at root (http://localhost:<port>/)
    //});
    if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "TEST PROJECT DOCKER");
            c.RoutePrefix = ""; // Optional: serves Swagger at the root (http://localhost:port/)
        });
    }
}

//app.UseHttpsRedirection();
app.UseIpRateLimiting();
app.UseAuthorization();
app.MapControllers();
app.Run();
