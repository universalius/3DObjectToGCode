using _3DObjectToGCode.Application.Features.IOFile;
using _3DObjectToGCode.Application.Features.ObjToMeshConverter;
using _3DObjectToGCode.Application.Helpers;
using GeometRi;
using Microsoft.Extensions.Logging;
using SvgLib;
using Face = ObjParser.Types.Face;

namespace _3DObjectToGCode.Application.Features.CylinderObjectToSvgConverter;

public class CylinderObjectToSvgConverterSeviceFailed(IOFileService fileService, ILogger<CylinderObjectToSvgConverterSeviceFailed> logger)
{
    public IEnumerable<Point3d> Convert(MeshObject meshObject)
    {
        var points = meshObject.Verts.Select(v => new { Id = v.Index, Point = new Point3d(v.X, v.Y, v.Z) }).ToArray();

        var facesWith4Verts = meshObject.Faces.Where(f => f.VertexIndexList.Length == 4).ToArray();
        var targetFace = facesWith4Verts.ElementAt(facesWith4Verts.Length / 2);

        var cutPlane = GetCylinderAxis(meshObject, new CylinderNeighbourFaces(null, null, [targetFace.Id], targetFace));

        if (cutPlane == null)
        {
            logger.LogInformation("Mesh object has not cylinder shape");
            return null;
        }

        var facePlanes = meshObject.Faces
        .Select(f =>
        {
            var facePoints = points.Where(p => f.VertexIndexList.Contains(p.Id)).Select(p => p.Point).ToArray();
            var plane = new Plane3d(facePoints[0], facePoints[1], facePoints[2]);
            return new { Face = f, Plane = plane, Edges = facePoints.ToSegments() };
        })
        .ToList();

        var crossCurvePoints = new List<Point3d>();
        facePlanes.ForEach(fp =>
        {
            var intersection = cutPlane.IntersectionWith(fp.Plane) as Line3d;
            if (intersection != null)
            {
                var crossPoints = fp.Edges
                    .Select(e => intersection.IntersectionWith(e) as Point3d)
                    .Where(p => p != null)
                    .ToArray();

                crossCurvePoints.AddRange(crossPoints);
            }
        });

        var svgDocument = SvgDocument.Create();
        var path = svgDocument.AddPath();
        //path.D = crossCurvePoints.ToArray().ToPathString();
        fileService.SaveSvg("cylinder_cross_slice", svgDocument.Element.OuterXml);

        return crossCurvePoints;
    }

    private Plane3d? GetCylinderAxis(MeshObject meshObject, CylinderNeighbourFaces cylinderNeighbourFaces)
    {
        var result = GetNextCylinderLoopFace(meshObject, cylinderNeighbourFaces);
        if (result?.FaceNormal != null && result.NextFace != null)
        {
            return GetCylinderAxis(meshObject, result);
        }

        return result?.FaceNormal != null && result?.Center != null ?
            new Plane3d(result.Center, result.FaceNormal.Direction) : null;
    }

    private CylinderNeighbourFaces? GetNextCylinderLoopFace(MeshObject meshObject, CylinderNeighbourFaces cylinderNeighbourFaces)
    {
        var (faceNormal, center, processedFaceIds, targetFace) = cylinderNeighbourFaces;
        var neighbourFaces = meshObject.Faces
            .Where(f => f.VertexIndexList.Length == 4 && f.VertexIndexList.Intersect(targetFace.VertexIndexList).Count() >= 2)
            .ToArray();

        //var a = meshObject.Faces.First(f => f.Id == 1057);

        if (!neighbourFaces.Any() || neighbourFaces.Length < 3)
        {
            return null;
        }

        var neighbourFacesNormals = neighbourFaces
            .Where(f => processedFaceIds.Count > 1 ? !processedFaceIds.Contains(f.Id) : true)
            .Select(f =>
            {
                var points = meshObject.Verts.Where(v => f.VertexIndexList.Contains(v.Index))
                    .Select(v => new Point3d(v.X, v.Y, v.Z)).ToArray();

                var plane = new Plane3d(points[0], points[1], points[2]);
                return new { FaceId = f.Id, Normal = new Line3d(points[0], plane.Normal) };
            })
            .ToArray();

        var normalLine = faceNormal ?? neighbourFacesNormals.First(n => n.FaceId == targetFace.Id).Normal;
        var intersections = neighbourFacesNormals
            .Select(v => new { v.FaceId, Point = normalLine.IntersectionWith(v.Normal) as Point3d })
            .Where(p => p.Point != null)
            .ToArray();

        if (!intersections.Any())
        {
            return null;
        }

        var centerPoint = center ?? intersections[0].Point;
        if (!intersections.All(p => centerPoint.DistanceTo(p.Point) < 0.001))
        {
            return null;
        }

        var nextFaceId = intersections.FirstOrDefault(p => !processedFaceIds.Contains(p.FaceId))?.FaceId;
        var nextFace = nextFaceId != null ? meshObject.Faces.First(f => f.Id == nextFaceId) : null;

        processedFaceIds.AddRange(intersections.Select(i => i.FaceId).ToArray());

        return new CylinderNeighbourFaces(normalLine, centerPoint, processedFaceIds.Distinct().ToList(), nextFace);
    }

    private record CylinderNeighbourFaces(Line3d FaceNormal, Point3d Center, List<int> ProcessedFaceIds, Face NextFace);
}


