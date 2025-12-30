using _3DObjectToGCode.Application.Features.CyinderObjectPartsClassificator;
using _3DObjectToGCode.Application.Features.CylinderObjectToSvgConverter;
using _3DObjectToGCode.Application.Features.ObjToMeshConverter;

namespace _3DObjectToGCode.Application.Features._3DObjectToGCode;

public class _3DObjectToGCodeService(ObjToMeshConverterService objToMeshConverter,
    CylinderObjectToSvgConverterSevice cylinderObjectToSvgConverter,
    CyinderObjectPartsClassificatorService cyinderObjectPartsClassificator)
{
    public async Task Convert()
    {
        var meshObject = await objToMeshConverter.Convert();

        var profilePoints = cylinderObjectToSvgConverter.Convert(meshObject);

        var cylinder = cyinderObjectPartsClassificator.Classiffy(profilePoints.ToArray());
    }

}
