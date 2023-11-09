using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using BaseProject.DAO.IService;
using BaseProject.Util;

namespace BaseProject.DAO.Service
{
    public class ServiceBusSender : IServiceBusSender
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ServiceBusSender> _logger;
        private readonly string _connection_string;
        private readonly ServiceBusClient _client;
        
        public ServiceBusSender(
            IConfiguration config,
            IWebHostEnvironment env,
            ILogger<ServiceBusSender> logger
        ) 
        {
            _config = config;
            _env = env;
            _logger = logger;
            _connection_string = _config.GetProperty<string>("AzureServiceBus", "ConnectionString");
            _client = new ServiceBusClient(_connection_string);
        }

        public async Task<bool> SendMessage<T>(byte queue, T message)
        {
            try
            {
                var sender = _client.CreateSender($"{((EnumAzureServiceBusQueue)queue).GetEnumDisplayName().ToLower()}_{_env.EnvironmentName.ToLower()}");

                await sender.SendMessageAsync(new ServiceBusMessage(JsonConvert.SerializeObject(message)));

                await sender.CloseAsync();

                return true;
            }
            catch
            {
                _logger.LogError($"Erro ao enviar mensagem ao Azure Service Bus! Queue Enum: {queue} - Queue Name: {((EnumAzureServiceBusQueue)queue).GetEnumDisplayName().ToLower()}_{_env.EnvironmentName.ToLower()}");
                return false;
            }
        }

    }
}