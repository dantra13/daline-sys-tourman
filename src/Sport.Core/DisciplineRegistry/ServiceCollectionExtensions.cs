using Microsoft.Extensions.DependencyInjection;

namespace Sport.Core.DisciplineRegistry;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSportCore(this IServiceCollection services)
    {
        services.AddSingleton<DisciplineRegistry>();
        services.AddSingleton<IDisciplineRegistry>(sp => sp.GetRequiredService<DisciplineRegistry>());
        return services;
    }

    public static IServiceCollection AddDisciplineModule<TModule>(this IServiceCollection services)
        where TModule : class, IDisciplineModule, new()
    {
        services.AddSingleton<IDisciplineModule, TModule>();
        return services;
    }

    public static IDisciplineRegistry BuildSportRegistry(this IServiceProvider provider)
    {
        var registry = provider.GetRequiredService<DisciplineRegistry>();
        foreach (var module in provider.GetServices<IDisciplineModule>())
            if (!registry.IsRegistered(module.Code))
                registry.Register(module);
        return registry;
    }
}
