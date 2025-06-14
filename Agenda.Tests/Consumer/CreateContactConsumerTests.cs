using System.Text;
using System.Text.Json;
using Agenda.Application.Contacts.Commands;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Agenda.Infrastructure.Messaging.Consumers
{
    public class CreateContactConsumer
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CreateContactConsumer> _logger;
        protected readonly RabbitMqOptions _options;

        public CreateContactConsumer(IServiceProvider serviceProvider, ILogger<CreateContactConsumer> logger, RabbitMqOptions options)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _options = options;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var connection = CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "contacts.create",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var command = JsonSerializer.Deserialize<CreateContactCommand>(message);

                    if (command is null)
                    {
                        _logger.LogWarning("Mensagem inválida recebida para contacts.create");
                        return;
                    }

                    using var scope = _serviceProvider.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    await mediator.Send(command, cancellationToken);

                    _logger.LogInformation("Contato criado com sucesso via consumer.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagem contacts.create");
                }
            };

            channel.BasicConsume(queue: "contacts.create", autoAck: true, consumer: consumer);

            _logger.LogInformation("Consumer contacts.create iniciado.");
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken);
            }
        }

        protected virtual IConnection CreateConnection()
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.Hostname,
                Port = _options.Port,
                UserName = _options.Username,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost
            };

            return factory.CreateConnection();
        }
    }
}
