using System.Text;
using System.Text.Json;
using Agenda.Application.Contacts.Queries;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Agenda.Infrastructure.Messaging.Consumers;

public class GetContactByIdConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GetContactByIdConsumer> _logger;
    private readonly IModel _channel;
    private readonly IConnection? _connection;

    public GetContactByIdConsumer(
        IServiceProvider serviceProvider,
        ILogger<GetContactByIdConsumer> logger,
        RabbitMqOptions options
    )
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
            DispatchConsumersAsync = true,
        };

        _connection = null;
        int retries = 10;
        while (retries-- > 0)
        {
            try
            {
                _connection = factory.CreateConnection();
                _logger.LogInformation("Conectado ao RabbitMQ para contacts.getbyid");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "RabbitMQ não disponível (contacts.getbyid). Tentativas restantes: {Retries}",
                    retries
                );
                Thread.Sleep(5000);
            }
        }

        if (_connection == null)
        {
            throw new Exception("Falha ao conectar ao RabbitMQ para contacts.getbyid");
        }

        _channel = _connection.CreateModel();
        _channel.QueueDeclare(
            queue: "contacts.getbyid",
            durable: true,
            exclusive: false,
            autoDelete: false
        );
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var props = ea.BasicProperties;
            var replyProps = _channel.CreateBasicProperties();
            replyProps.CorrelationId = props?.CorrelationId;

            try
            {
                var body = ea.Body.ToArray();
                var payload = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
                var id = Guid.Parse(payload["id"]);

                using var scope = _serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var result = await mediator.Send(new GetContactByIdQuery(id), stoppingToken);

                var responseJson = JsonSerializer.Serialize(result);

                var responseBytes = Encoding.UTF8.GetBytes(responseJson);

                _channel.BasicPublish(
                    exchange: "",
                    routingKey: props?.ReplyTo,
                    basicProperties: replyProps,
                    body: responseBytes
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem contacts.getbyid");

                var errorJson = JsonSerializer.Serialize(new { error = ex.Message });
                var error = Encoding.UTF8.GetBytes(errorJson);

                _channel.BasicPublish(
                    exchange: "",
                    routingKey: props?.ReplyTo,
                    basicProperties: replyProps,
                    body: error
                );
            }
            finally
            {
                _channel.BasicAck(ea.DeliveryTag, false);
            }
        };

        _channel.BasicConsume(queue: "contacts.getbyid", autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
