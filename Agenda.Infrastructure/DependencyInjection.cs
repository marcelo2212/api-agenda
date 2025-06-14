using Microsoft.Extensions.DependencyInjection;

namespace Agenda.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Aqui você poderá registrar repositórios, serviços, publishers etc.
        return services;
    }
}
