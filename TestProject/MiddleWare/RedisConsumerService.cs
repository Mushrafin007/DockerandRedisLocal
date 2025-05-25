using StackExchange.Redis;
using static TestProject.Model.CrudApiModel;
using TestProject.Services.CrudApiService;
using Newtonsoft.Json;

namespace TestProject.MiddleWare
{
    public class RedisConsumerService : BackgroundService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IServiceScopeFactory _scopeFactory;

        public RedisConsumerService(IConnectionMultiplexer redis, IServiceScopeFactory scopeFactory)
        {
            _redis = redis;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var db = _redis.GetDatabase();
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = await db.ListRightPopAsync("create-queue");
                if (!result.IsNullOrEmpty)
                {
                    var createModel = JsonConvert.DeserializeObject<CreateMode>(result.ToString());
                    using var scope = _scopeFactory.CreateScope();
                    var crudService = scope.ServiceProvider.GetRequiredService<ICrudApiService>();

                    var parameters = new
                    {
                        Name = createModel.Name,
                        Phone = createModel.Phone,
                        Email = createModel.Email,
                        Gender = createModel.Gender
                    };

                    await crudService.USP_CREATE_INS(parameters);
                }

                await Task.Delay(60); // slight delay to avoid tight loop
            }
        }
    }

}
