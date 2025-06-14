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

public class GetAllContactsConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GetAllContactsConsumer> _logger;
    private readonly IModel _channel;
    private readonly IConnection? _connection;
    internal AsyncEventingBasicConsumer? TestConsumer { get; private set; }

    public GetAllContactsConsumer(IServiceProvider serviceProvider, ILogger<GetAllContactsConsumer> logger, RabbitMqOptions options)
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

        _connection = null;
        int retries = 10;
        while (retries-- > 0)
        {
            try
            {
                _connection = factory.CreateConnection();
                _logger.LogInformation("Conectado ao RabbitMQ com sucesso.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "RabbitMQ não disponível. Tentativas restantes: {Retries}", retries);
                Thread.Sleep(5000);
            }
        }

        if (_connection == null)
        {
            throw new Exception("Não foi possível conectar ao RabbitMQ após múltiplas tentativas.");
        }

        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: "contacts.getall", durable: true, exclusive: false, autoDelete: false);
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
                using var scope = _serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var result = await mediator.Send(new GetAllContactsQuery(), stoppingToken);

                var responseJson = JsonSerializer.Serialize(result);
                _logger.LogInformation("[✔] Resposta para contacts.getall: {Response}", responseJson);

                var responseBytes = Encoding.UTF8.GetBytes(responseJson);

                _channel.BasicPublish(
                    exchange: "",
                    routingKey: props.ReplyTo,
                    basicProperties: replyProps,
                    body: responseBytes
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem contacts.getall");
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

        _channel.BasicConsume(queue: "contacts.getall", autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
