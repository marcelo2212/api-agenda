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

public class CreateContactConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CreateContactConsumer> _logger;
    private readonly IModel _channel;
    private readonly IConnection? _connection;

    public CreateContactConsumer(IServiceProvider serviceProvider, ILogger<CreateContactConsumer> logger, RabbitMqOptions options)
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
        _channel.QueueDeclare(queue: "contacts.create", durable: true, exclusive: false, autoDelete: false);
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
                var dto = JsonSerializer.Deserialize<CreateContactDto>(body);

                if (dto == null)
                {
                    _logger.LogWarning("Payload inválido recebido em contacts.create");
                    _channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                using var scope = _serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var command = new CreateContactCommand(dto);
                var id = await mediator.Send(command, stoppingToken);

                _logger.LogInformation("Contato criado com sucesso: {Id}", id);

                var responseBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(id));

                _channel.BasicPublish(
                    exchange: "",
                    routingKey: props.ReplyTo,
                    basicProperties: replyProps,
                    body: responseBytes
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem contacts.create");
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

        _channel.BasicConsume(queue: "contacts.create", autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }

    
}
