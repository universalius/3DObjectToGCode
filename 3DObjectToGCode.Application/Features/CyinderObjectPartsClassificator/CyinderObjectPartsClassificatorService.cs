using _3DObjectToGCode.Application.Helpers;
using GeometRi;

namespace _3DObjectToGCode.Application.Features.CyinderObjectPartsClassificator;

public class CyinderObjectPartsClassificatorService()
{
    public Cylinder Classiffy(Point3d[] points)
    {
        var centerMinX = points.Min(p => p.X);
        var centerMaxX = points.Max(p => p.X);
        var centerMinY = points.Min(p => p.Y);
        var centerMaxY = points.Max(p => p.Y);

        var centerX = centerMinX + (centerMaxX - centerMinX) / 2.0;
        var centerY = centerMinY + (centerMaxY - centerMinY) / 2.0;

        var centerPoint = new Point3d(centerX, centerY, 0.0);

        var segments = points.Where(p => p.Y >= centerPoint.Y).ToArray().ToSegments(false);
        var cylinderParts = segments.Select(s =>
        {
            return new CylinderPart(ClassiffySegment(s), [s]);
        }).ToArray();

        return new Cylinder(centerPoint, cylinderParts);
    }

    public CylinderPartClass ClassiffySegment(Segment3d segment)
    {
        if (Math.Abs(segment.Direction.Y) == 1)
        {
            return CylinderPartClass.Vertical;
        }

        if (Math.Abs(segment.Direction.X) == 1)
        {
            return CylinderPartClass.Horizontal;
        }

        return CylinderPartClass.Chamfer;
    }


    public record Cylinder(Point3d Center, IEnumerable<CylinderPart> Parts);

    public record CylinderPart(CylinderPartClass Class, Segment3d[] Segments);

    public enum CylinderPartClass
    {
        Horizontal,
        Vertical,
        Chamfer
    }
}
