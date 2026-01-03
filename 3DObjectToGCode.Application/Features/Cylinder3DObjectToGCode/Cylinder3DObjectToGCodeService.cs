using _3DObjectToGCode.Application.Features.CyinderObjectPartsClassificator;
using _3DObjectToGCode.Application.Features.CylinderObjectToSvgConverter;
using _3DObjectToGCode.Application.Features.ObjToMeshConverter;
using GCodes;
using GCodes.Extensions;
using GCodes.G;
using GCodes.Interfaces;
using GCodes.M;
using GCodes.N;
using GeometRi;
using static _3DObjectToGCode.Application.Features.CyinderObjectPartsClassificator.CyinderObjectPartsClassificatorService;

namespace _3DObjectToGCode.Application.Features.Cylinder3DObjectToGCode;

public class Cylinder3DObjectToGCodeService(
    CylinderObjectToSvgConverterSevice cylinderObjectToSvgConverter,
    CyinderObjectPartsClassificatorService cyinderObjectPartsClassificator)
{
    private Point3d _newOriginPoint;


    public async Task Convert(MeshObject meshObject)
    {
        var profilePoints = cylinderObjectToSvgConverter.Convert(meshObject);

        var cylinder = cyinderObjectPartsClassificator.Classiffy(profilePoints.ToArray());

        CreateGCodeProgramm(cylinder);
    }


    private void CreateGCodeProgramm(Cylinder cylinder)
    {
        var size = cylinder.Size;
        _newOriginPoint = new Point3d(size.XMax, cylinder.Center.Y, 0);
        //var parts = cylinder.Parts.ToList();
        var prepareGCodes = new List<IGCode>
        {
            new NLine(1),
            new G54OriginPoint(),
            new G21MetricUnits(),
            new G80CancelCycles(),
            new G40CancelCutterCompensation(),
            new G50MaxRpm(2500),
        };

        var rightParts = cylinder.RightParts.ToList();

        var rightPartsFinalPassGCodes = new List<IGCode>();
        if (rightParts.Any())
        {
            for (int i = 0; i < rightParts.Count; i++)
            {
                var part = rightParts[i];
                G01LinearMove gCode = null;

                if (i == 0)
                {
                    if (part.Class == CylinderPartClass.Vertical)
                    {
                        rightPartsFinalPassGCodes.Add(new G01LinearMove(Transform(part.Segments[0].P2), 0.2));
                    }

                    if (part.Class == CylinderPartClass.Chamfer)
                    {
                        rightPartsFinalPassGCodes.Add(new G01LinearMove(Transform(part.Segments[0].P1), 0.2));
                        rightPartsFinalPassGCodes.Add(new G01LinearMove(Transform(part.Segments[0].P2)));
                    }

                    continue;
                }

                if (part.Class == CylinderPartClass.Vertical)
                {
                    gCode = new G01LinearMove(Transform(y: part.Segments[0].P2.Y));
                }

                if (part.Class == CylinderPartClass.Horizontal)
                {
                    gCode = new G01LinearMove(Transform(x: part.Segments[0].P2.X));
                }

                if (part.Class == CylinderPartClass.Chamfer)
                {
                    gCode = new G01LinearMove(Transform(part.Segments[0].P2));
                }

                if (part.Class == CylinderPartClass.Groove)
                {
                    var segments = part.Segments.ToList();
                    var lastSegment = segments.Last();
                    var line = new Line3d(segments[0].P1, new Vector3d(1.0, 0.0, 0.0));
                    var intersection = line.IntersectionWith(lastSegment) as Point3d;

                    if (intersection != null)
                    {
                        rightPartsFinalPassGCodes.Add(new G01LinearMove(Transform(x: intersection.X)));
                        rightPartsFinalPassGCodes.Add(new G01LinearMove(Transform(lastSegment.P2)));
                        continue;
                    }
                }

                rightPartsFinalPassGCodes.Add(gCode);

                //if (part.Class == CylinderPartClass.Vertical)
                //{
                //    var prevPart = rightParts[i - 1];
                //    if (part.Segments[0].P1.Y < prevPart.Segments[0].P2.Y)
                //    {
                //        var nextVerticalPart = rightParts.Skip(i + 1).FirstOrDefault(p => p.Class == CylinderPartClass.Vertical);
                //        rightPartsRouphPassGCodes.Add(new G01LinearMove(Transform(y: part.Segments[0].P2.Y), 0.2));
                //        continue;
                //    }


                //    rightPartsRouphPassGCodes.Add(new G01LinearMove(Transform(y: part.Segments[0].P2.Y), 0.2));
                //    continue;
                //}
            }

        }

        var rightPartsRoughPassGCodes = new List<IGCode>();
        var startPointGcode = new G00RapidTravel(Transform(size.XMax + 5, size.YMax + 5));
        rightPartsRoughPassGCodes.Add(new G96ConstantSurfaceSpeed(280, SpindleRotationDirection.Clockwise));
        rightPartsRoughPassGCodes.Add(new G71RoughingCycle(1, 1, 100, 200, 0.5, 0.5, 0.2));
        rightPartsRoughPassGCodes.Add(startPointGcode);
        rightPartsRoughPassGCodes.Add(new G41CutterCompensationLeft());
        rightPartsRoughPassGCodes.Add(new NLine(100));
        rightPartsRoughPassGCodes.AddRange(rightPartsFinalPassGCodes);
        rightPartsRoughPassGCodes.Add(new NLine(200));
        rightPartsRoughPassGCodes.Add(startPointGcode);
        rightPartsRoughPassGCodes.Add(new G53MachineOriginPoint(0, 0, 0));

        var programm = new List<IGCode>();
        programm.AddRange(prepareGCodes);
        programm.AddRange(rightPartsRoughPassGCodes);

        var text = programm.ToText();
    }

    private GCoordinate Transform(Point3d point)
    {
        var newPoint = _newOriginPoint - point;
        var allignedY = newPoint.Y != 0 ? -newPoint.Y : newPoint.Y;
        var allignedX = newPoint.X != 0 ? -newPoint.X : newPoint.X;

        return new GCoordinate(Z: allignedX, X: allignedY*2);
    }

    private GCoordinate Transform(double? x = null, double? y = null)
    {
        var newX = x.HasValue ? _newOriginPoint.X - x.Value : (double?)null;
        var newY = y.HasValue ? _newOriginPoint.Y - y.Value : (double?)null;
        var allignedY = newY.HasValue && newY != 0 ? -newY.Value : newY;
        var allignedX = newX.HasValue && newX != 0 ? -newX.Value : newX;

        return new GCoordinate(Z: allignedX, X: allignedY*2);
    }
}
