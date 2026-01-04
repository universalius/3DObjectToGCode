using _3DObjectToGCode.Application.Helpers;
using GeometRi;
using ObjParser;

namespace _3DObjectToGCode.Application.Features.CyinderObjectPartsClassificator;

public class CyinderObjectPartsClassificatorService()
{
    public Cylinder Classiffy(Point3d[] points)
    {
        var minX = points.Min(p => p.X);
        var maxX = points.Max(p => p.X);
        var minY = points.Min(p => p.Y);
        var maxY = points.Max(p => p.Y);

        var centerX = minX + (maxX - minX) / 2.0;
        var centerY = minY + (maxY - minY) / 2.0;

        var size = new Extent
        {
            XMax = maxX,
            XMin = minX,
            YMax = maxY,
            YMin = minY
        };

        var centerPoint = new Point3d(centerX, centerY, 0.0);
        var closed = points.First().DistanceTo(points.Last()) <= 0.001;
        var segments = points.Where(p => p.Y >= centerPoint.Y + 0.001 || p.Y >= centerPoint.Y - 0.001).ToArray().ToSegments(closed).ToList();

        if (segments.First().P1.X > segments.Last().P1.X)
        {
            segments.Reverse();
        }

        if (segments.First().P1.Y > segments.First().P2.Y)
        {
            segments = segments.Select(s => new Segment3d(s.P2.Copy(), s.P1.Copy())).ToList();
        }

        var maxHeightSegment = segments.First(s => s.P1.Y == size.YMax || s.P2.Y == size.YMax);
        var maxHeightSegmentIndex = segments.IndexOf(maxHeightSegment);
        var leftSegments = segments.Take(maxHeightSegmentIndex).ToList();
        var rightSegments = segments.Skip(maxHeightSegmentIndex).Reverse().Select(s => new Segment3d(s.P2.Copy(), s.P1.Copy())).ToList();


        var leftParts = leftSegments.Any() ? GetCylinderParts(leftSegments) : null;
        var rightParts = rightSegments.Any() ? GetCylinderParts(rightSegments) : null;

        return new Cylinder(centerPoint, size, leftParts.Where(p => p != null), rightParts.Where(p => p != null));
    }

    private CylinderPart?[] GetCylinderParts(List<Segment3d> segments)
    {
        var cylinderParts = new List<CylinderPart?>();
        for (int i = 0; i < segments.Count; i++)
        {
            var s = segments[i];
            var part = ClassiffySegment(s, segments);
            cylinderParts.Add(part);
            if (part != null && part.Segments.Length > 1)
            {
                i = segments.IndexOf(part.Segments.Last());
            }
        }

        return cylinderParts.ToArray();
    }

    // segments should be filtered by Y ascending
    private CylinderPart? ClassiffySegment(Segment3d segment, List<Segment3d> segments)
    {
        if (IsGoingUp(segment))
        {
            if (IsHorizontal(segment))
            {
                return new CylinderPart(CylinderPartClass.Horizontal, [segment]);
            }

            // vertical going up direction
            if (IsVertical(segment))
            {
                return new CylinderPart(CylinderPartClass.Vertical, [segment]);
            }

            if (IsChamfer(segment, segments))
            {
                return new CylinderPart(CylinderPartClass.Chamfer, [segment]);
            }
        }
        else
        {
            if (IsGroove(segment, segments, out var grooveSegments))
            {
                return new CylinderPart(CylinderPartClass.Groove, grooveSegments);
            }
        }

        return null;
    }

    private bool IsChamfer(Segment3d segment, List<Segment3d> segments)
    {
        if (!IsVertical(segment) && !IsHorizontal(segment))
        {
            var segmentIndex = segments.IndexOf(segment);
            if (segmentIndex == segments.Count - 1)
            {
                return true;
            }

            var nextSegment = segments[segmentIndex + 1];
            if (IsVertical(nextSegment) || IsHorizontal(nextSegment))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsGroove(Segment3d segment, List<Segment3d> segments, out Segment3d[] grooveSegments)
    {
        grooveSegments = Array.Empty<Segment3d>();
        if (IsGoingDown(segment))
        {
            var line = new Line3d(segment.P1, new Vector3d(1.0, 0.0, 0.0));
            var segmentIndex = segments.IndexOf(segment);
            var targetSegments = segments.Skip(segmentIndex + 1).ToList();
            var lastGrooveSegment = targetSegments.FirstOrDefault(s => line.IntersectionWith(s) != null);

            if (lastGrooveSegment != null)
            {
                var lastGrooveSegmentIndex = targetSegments.IndexOf(lastGrooveSegment);
                var result = new List<Segment3d> { segment };
                result.AddRange(targetSegments.Take(lastGrooveSegmentIndex + 1));
                grooveSegments = result.ToArray();

                return true;
            }
        }

        return false;
    }

    private bool IsGoingUp(Segment3d segment)
    {
        return segment.P1.Y <= segment.P2.Y;
    }

    private bool IsGoingDown(Segment3d segment)
    {
        return segment.P1.Y > segment.P2.Y;
    }

    private bool IsVerticalUp(Segment3d segment)
    {
        return IsVertical(segment) && IsGoingUp(segment);
    }

    private bool IsVertical(Segment3d segment)
    {
        return Math.Abs(segment.Direction.Y) == 1;
    }

    private bool IsHorizontal(Segment3d segment)
    {
        return Math.Abs(segment.Direction.X) == 1;
    }

    public record Cylinder(Point3d Center, Extent Size, IEnumerable<CylinderPart> LeftParts, IEnumerable<CylinderPart> RightParts);

    public record CylinderPart(CylinderPartClass Class, Segment3d[] Segments);

    public enum CylinderPartClass
    {
        Horizontal,
        Vertical,
        Chamfer,
        Groove,
        Fillet
    }
}
