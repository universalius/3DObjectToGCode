using _3DObjectToGCode.Application.Features.CyinderObjectPartsClassificator;
using _3DObjectToGCode.Application.Features.CylinderObjectToSvgConverter;
using _3DObjectToGCode.Application.Features.ObjToMeshConverter;
using GCodes;
using GCodes.Extensions;
using GCodes.G;
using GCodes.Interfaces;
using GCodes.M;
using GCodes.N;
using GCodes.T;
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
        var prepareGCodes = new List<IGCode>
        {
            new NLine(1),
            new G54OriginPoint(),
            new G21MetricUnits(),
            new G80CancelCycles(),
            new G40CancelCutterCompensation(),
            new G50MaxRpm(2500),
        };

        var rightParts = cylinder.RightParts.ToArray();
        var rightPartsRoughPassGCodes = Array.Empty<IGCode>();
        if (rightParts.Length > 0)
        {
            var rightPartsFinalPathGCodes = GetGCodesProfileWithoutGroves(rightParts);
            rightPartsRoughPassGCodes = GetRoughPassGCodes(new Point3d(size.XMax + 5, size.YMax + 5, 0),
                100, SpindleRotationDirection.Clockwise, rightPartsFinalPathGCodes);

        }

        var leftParts = cylinder.LeftParts.ToArray();
        var leftPartsRoughPassGCodes = Array.Empty<IGCode>();
        if (leftParts.Length > 1)
        {
            var leftPartsFinalPathGCodes = GetGCodesProfileWithoutGroves(leftParts.Skip(1).ToArray());
            leftPartsRoughPassGCodes = GetRoughPassGCodes(new Point3d(size.XMin, size.YMax + 5, 0),
               300, SpindleRotationDirection.CounterClockwise, leftPartsFinalPathGCodes);
        }

        var programm = new List<IGCode>();
        programm.AddRange(prepareGCodes);

        if (rightPartsRoughPassGCodes.Length > 0)
        {
            programm.Add(new T0_ToolChange(1, 1));
            programm.AddRange(rightPartsRoughPassGCodes);
            programm.Add(new M05SpindleStop());
        }

        if (leftPartsRoughPassGCodes.Length > 0)
        {
            programm.Add(new G40CancelCutterCompensation());
            programm.Add(new T0_ToolChange(2, 2));
            programm.AddRange(leftPartsRoughPassGCodes);
            programm.Add(new M05SpindleStop());
        }

        var text = programm.ToText();
    }

    private IGCode[] GetGCodesProfileWithoutGroves(CylinderPart[] parts)
    {
        var finalPassGCodes = new List<IGCode>();
        if (parts.Any())
        {
            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                G01LinearMove gCode = null;

                if (i == 0)
                {
                    if (part.Class == CylinderPartClass.Vertical)
                    {
                        finalPassGCodes.Add(new G01LinearMove(Transform(part.Segments[0].P2), 0.2));
                    }

                    if (part.Class == CylinderPartClass.Chamfer)
                    {
                        finalPassGCodes.Add(new G01LinearMove(Transform(part.Segments[0].P1), 0.2));
                        finalPassGCodes.Add(new G01LinearMove(Transform(part.Segments[0].P2)));
                    }

                    if (part.Class == CylinderPartClass.Horizontal)
                    {
                        finalPassGCodes.Add(new G01LinearMove(Transform(part.Segments[0].P1), 0.2));
                        finalPassGCodes.Add(new G01LinearMove(Transform(part.Segments[0].P2.X)));
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
                        finalPassGCodes.Add(new G01LinearMove(Transform(x: intersection.X)));
                        finalPassGCodes.Add(new G01LinearMove(Transform(lastSegment.P2)));
                        continue;
                    }
                }

                finalPassGCodes.Add(gCode);
            }

        }

        return finalPassGCodes.ToArray();
    }

    private IGCode[] GetRoughPassGCodes(Point3d startPoint,
        int startLine,
        SpindleRotationDirection rotationDirection,
        IGCode[] finalPathGCodes)
    {
        var endLine = startLine + 100;

        var roughPassGCodes = new List<IGCode>();
        var startPointGcode = new G00RapidTravel(Transform(startPoint));
        roughPassGCodes.Add(new G96ConstantSurfaceSpeed(280, rotationDirection));
        roughPassGCodes.Add(new G71RoughingCycle(1, 1, startLine, endLine, 0.5, 0.5, 0.2));
        roughPassGCodes.Add(startPointGcode);
        roughPassGCodes.Add(rotationDirection == SpindleRotationDirection.Clockwise ? new G41CutterCompensationLeft() : new G42CutterCompensationRight());
        roughPassGCodes.Add(new NLine(startLine));
        roughPassGCodes.AddRange(finalPathGCodes);
        roughPassGCodes.Add(new NLine(endLine));
        roughPassGCodes.Add(startPointGcode);

        return roughPassGCodes.ToArray();
    }


    private GCoordinate Transform(Point3d point)
    {
        var newPoint = _newOriginPoint - point;
        var allignedY = newPoint.Y != 0 ? -newPoint.Y : newPoint.Y;
        var allignedX = newPoint.X != 0 ? -newPoint.X : newPoint.X;

        return new GCoordinate(Z: allignedX, X: allignedY * 2);
    }

    private GCoordinate Transform(double? x = null, double? y = null)
    {
        var newX = x.HasValue ? _newOriginPoint.X - x.Value : (double?)null;
        var newY = y.HasValue ? _newOriginPoint.Y - y.Value : (double?)null;
        var allignedY = newY.HasValue && newY != 0 ? -newY.Value : newY;
        var allignedX = newX.HasValue && newX != 0 ? -newX.Value : newX;

        return new GCoordinate(Z: allignedX, X: allignedY * 2);
    }
}
