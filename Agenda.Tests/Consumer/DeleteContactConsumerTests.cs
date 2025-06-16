using System.Text;
using System.Text.Json;
using Agenda.Application.Contacts.Commands;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Agenda.Infrastructure.Messaging.Consumers;

public class DeleteContactConsumer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DeleteContactConsumer> _logger;
    private readonly RabbitMqOptions _options;
    private readonly IModel _channel;

    public DeleteContactConsumer(
        IServiceProvider serviceProvider,
        ILogger<DeleteContactConsumer> logger,
        RabbitMqOptions options,
        IModel? injectedChannel = null
    )
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options;

        if (injectedChannel != null)
        {
            _channel = injectedChannel;
            return;
        }

        var factory = new ConnectionFactory()
        {
            HostName = _options.Hostname,
            Port = _options.Port,
            UserName = _options.Username,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost,
        };

        var connection = factory.CreateConnection();
        _channel = connection.CreateModel();

        _channel.QueueDeclare(
            queue: "contacts.delete",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            await ExecuteAsync(ea.Body.ToArray(), ea.BasicProperties, CancellationToken.None);
        };

        _channel.BasicConsume(queue: "contacts.delete", autoAck: true, consumer: consumer);
    }

    private async Task ExecuteAsync(
        byte[] body,
        IBasicProperties props,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var id = JsonSerializer.Deserialize<Guid>(Encoding.UTF8.GetString(body));

            using var scope = _serviceProvider
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            await mediator.Send(new DeleteContactCommand(id), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mensagem de exclusão.");
        }
    }
}
