using _3DObjectToGCode.Application.Features.ObjToMeshConverter;

namespace _3DObjectToGCode.Application.Features._3DObjectToGCode;

public class _3DObjectToGCodeService(ObjToMeshConverterService objToMeshConverter)
{
    public async Task Convert()
    {

        var meshObject = await objToMeshConverter.Convert();
    }

}
