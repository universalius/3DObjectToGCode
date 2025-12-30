using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace _3DObjectToGCode.Application.Features.CyinderObjectPartsClassificator;

public static class ServiceRegistration
{
    public static IServiceCollection AddCyinderObjectPartsClassificatorFeature(this IServiceCollection services,
       IConfiguration configuration)
    {
        services
            .AddSingleton<CyinderObjectPartsClassificatorService>();

        //services
        //    .AddIOFileFeature(configuration);

        return services;
    }
}