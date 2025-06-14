using System.Text;
using System.Text.Json;
using Agenda.Application.Contacts.Commands;
using Agenda.Application.Contacts.Dtos;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Agenda.Infrastructure.Messaging.Consumers;

public class UpdateContactConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UpdateContactConsumer> _logger;
    private readonly IModel _channel;
    private readonly IConnection _connection;

    public UpdateContactConsumer(IServiceProvider serviceProvider, ILogger<UpdateContactConsumer> logger, RabbitMqOptions options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = options.Hostname,
            Port = options.Port,
            UserName = options.Username,
            Password = options.Password,
            VirtualHost = options.VirtualHost,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: "contacts.update", durable: true, exclusive: false, autoDelete: false);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var props = ea.BasicProperties;
            var replyProps = _channel.CreateBasicProperties();
            replyProps.CorrelationId = props.CorrelationId;

            try
            {
                var body = ea.Body.ToArray();
                var payload = JsonSerializer.Deserialize<UpdateContactPayload>(body);

                if (payload == null || payload.Id == Guid.Empty || payload.Contact == null)
                {
                    _logger.LogWarning("Payload inválido recebido em contacts.update");
                    _channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                using var scope = _serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var command = new UpdateContactCommand(payload.Id, payload.Contact);
                await mediator.Send(command, stoppingToken);

                var message = $"Contato {payload.Id} atualizado com sucesso";
                _logger.LogInformation("[✔] {Message}", message);

                var responseBytes = Encoding.UTF8.GetBytes(message);

                _channel.BasicPublish(
                    exchange: "",
                    routingKey: props.ReplyTo,
                    basicProperties: replyProps,
                    body: responseBytes
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem contacts.update");
                var error = Encoding.UTF8.GetBytes($"Erro: {ex.Message}");

                _channel.BasicPublish(
                    exchange: "",
                    routingKey: props.ReplyTo,
                    basicProperties: replyProps,
                    body: error
                );
            }
            finally
            {
                _channel.BasicAck(ea.DeliveryTag, false);
            }
        };

        _channel.BasicConsume(queue: "contacts.update", autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }

    private class UpdateContactPayload
    {
        public required Guid Id { get; set; }
        public required UpdateContactDto Contact { get; set; }
    }
}
