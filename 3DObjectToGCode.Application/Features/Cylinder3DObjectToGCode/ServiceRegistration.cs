using _3DObjectToGCode.Application.Features.CyinderObjectPartsClassificator;
using _3DObjectToGCode.Application.Features.CylinderObjectToSvgConverter;
using _3DObjectToGCode.Application.Features.ObjToMeshConverter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace _3DObjectToGCode.Application.Features.Cylinder3DObjectToGCode;

public static class ServiceRegistration
{
    public static IServiceCollection AddCylinder3DObjectToGCodeFeature(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddSingleton<Cylinder3DObjectToGCodeService>();

        services
            .AddObjToMeshConverterFeature(configuration)
            .AddCylinderObjectToSvgConverterFeature(configuration)
            .AddCyinderObjectPartsClassificatorFeature(configuration);

        return services;
    }
}