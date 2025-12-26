using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace _3DObjectToGCode;

public static class ServiceRegistration
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHostedService<Flat3DObjectsToSvgHostedService>();
        //    .AddSingleton<SvgCompactingService>()
        //    .AddSingleton<IOFileService>()
        //    .AddSingleton<Statistics>()
        //    .AddParse3dObjectsServices()
        //    .AddPostProcessorsServices();

        //services.AddOptions()
        //    .Configure<SvgNestConfig>(configuration.GetSection("SvgNest"))
        //    .Configure<IOSettings>(configuration.GetSection("IO"))
        //    .Configure<FeaturesSettings>(configuration.GetSection("Features"))
        //    .Configure<KerfSettings>(configuration.GetSection("Kerf"));

        return services;
    }
}