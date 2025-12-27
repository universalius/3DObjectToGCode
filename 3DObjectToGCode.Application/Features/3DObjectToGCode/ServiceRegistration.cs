using _3DObjectToGCode.Application.Features.CylinderObjectToSvgConverter;
using _3DObjectToGCode.Application.Features.ObjToMeshConverter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace _3DObjectToGCode.Application.Features._3DObjectToGCode;

public static class ServiceRegistration
{
    public static IServiceCollection Add3DObjectToGCodeFeature(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddSingleton<_3DObjectToGCodeService>();

        services
            .AddObjToMeshConverterFeature(configuration)
            .AddCylinderObjectToSvgConverterFeature(configuration);

        return services;
    }
}