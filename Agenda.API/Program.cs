using Agenda.Application;
using Agenda.Infrastructure;
using Agenda.Infrastructure.Contacts.Handlers;
using Agenda.Infrastructure.Data;
using Agenda.Infrastructure.Messaging.Consumers;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configuração fixa da connection string
var connectionString = "Host=postgres;Port=5432;Database=agenda;Username=postgres;Password=postgres";

builder.Services.AddDbContext<AgendaDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<InfrastructureAssemblyReference>());

builder.Services.AddAutoMapper(typeof(ApplicationAssemblyReference));

builder.Services.AddControllers();

builder.Services
    .AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters()
    .AddValidatorsFromAssemblyContaining<ApplicationAssemblyReference>();

// Configuração do RabbitMQ com valores fixos
builder.Services.AddSingleton(new RabbitMqOptions
{
    Hostname = "rabbitmq",
    Port = 5672,
    Username = "guest",
    Password = "guest",
    VirtualHost = "/"
});

// builder.Services.AddHostedService<GetAllContactsConsumer>();
// builder.Services.AddHostedService<CreateContactConsumer>();
// builder.Services.AddScoped<CreateContactHandler>();
// builder.Services.AddHostedService<GetContactByIdConsumer>();

builder.Services.AddHostedService<CreateContactConsumer>();
builder.Services.AddHostedService<GetAllContactsConsumer>();
builder.Services.AddHostedService<GetContactByIdConsumer>();
builder.Services.AddHostedService<UpdateContactConsumer>();
builder.Services.AddHostedService<DeleteContactConsumer>();

builder.Services.AddInfrastructure();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "Agenda API", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

// Executa migração com retry
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AgendaDbContext>();

    var retries = 10;
    var delay = TimeSpan.FromSeconds(5);

    for (int i = 0; i < retries; i++)
    {
        try
        {
            db.Database.Migrate();
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MIGRATION ERROR] Tentativa {i + 1} falhou: {ex.Message}");
            if (i == retries - 1) throw;
            Thread.Sleep(delay);
        }
    }
}

app.Run();
