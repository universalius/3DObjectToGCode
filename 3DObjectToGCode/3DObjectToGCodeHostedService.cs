using _3DObjectToGCode.Application.Features._3DObjectToGCode;
using _3DObjectToGCode.Application.Features.Cylinder3DObjectToGCode;
using _3DObjectToGCode.Application.Features.ObjToMeshConverter;
using Microsoft.Extensions.Hosting;

namespace _3DObjectToGCode;

public class _3DObjectToGCodeHostedService(ObjToMeshConverterService objToMeshConverter,
    Cylinder3DObjectToGCodeService _3DObjectToGCode) : IHostedService
{
    public async Task StartAsync(CancellationToken stoppingToken)
    {
        var meshObject = await objToMeshConverter.Convert();

        await _3DObjectToGCode.Convert(meshObject);

        //var svg = await threeDObjectsParser.Transform3DObjectsTo2DSvgLoops();
        //var compactedSvg = await svgCompactingService.Compact(svg);

        ////SvgDocument svgDocument = SvgFileHelpers.ParseSvgFile(@"D:\Виталик\Hexapod\Modo\Svg\BodyAndLegs 26.04.2025 12-16-36\BodyAndLegs_compacted.svg");
        ////var compactedSvg = svgDocument.Element.OuterXml;

        //var kerfedSvg = kerfApplier.ApplyKerf(compactedSvg);

        ////SvgDocument svgDocument = SvgFileHelpers.ParseSvgFile(@"D:\Виталик\Hexapod\Modo\Svg\BodyAndLegs 03.05.2025 12-35-02\BodyAndLegs_compacted_kerfed_with_line_gaps.svg");
        ////var kerfedSvg = svgDocument.Element.OuterXml;

        //await postProccessors.Run(kerfedSvg);

        //if (statistics.ObjectsCount != statistics.CompactedLoopsCount)
        //{
        //    Console.WriteLine($"NOT COMPACTED ALL PARSED OBJECTS, total parsed objects - {statistics.ObjectsCount}, " +
        //        $"total compacted loops - {statistics.CompactedLoopsCount}. Pls descrease spacing or increase document size");
        //    Console.WriteLine();
        //}

        Console.ReadKey();
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

}

