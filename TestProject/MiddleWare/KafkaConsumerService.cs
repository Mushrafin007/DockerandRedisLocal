using Confluent.Kafka;
using Newtonsoft.Json;
using static TestProject.Model.CrudApiModel;
using TestProject.Services.CrudApiService;

namespace TestProject.MiddleWare
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;

        public KafkaConsumerService(IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"],
                GroupId = "user-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe(_configuration["Kafka:CreateUserTopic"]);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(stoppingToken);

                    if (result != null && !string.IsNullOrWhiteSpace(result.Message.Value))
                    {
                        var user = JsonConvert.DeserializeObject<CreateMode>(result.Message.Value);
                        using var scope = _scopeFactory.CreateScope();
                        var crudService = scope.ServiceProvider.GetRequiredService<ICrudApiService>();

                        var parameters = new
                        {
                            Name = user.Name,
                            Phone = user.Phone,
                            Email = user.Email,
                            Gender = user.Gender
                        };

                        await crudService.USP_CREATE_INS(parameters);
                    }
                }
                catch (ConsumeException ex)
                {
                    Console.WriteLine($"Kafka Error: {ex.Error.Reason}");
                }

                await Task.Delay(100);
            }

            consumer.Close();
        }
    }
}
