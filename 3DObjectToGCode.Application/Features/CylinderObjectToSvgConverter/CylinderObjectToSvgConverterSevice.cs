using _3DObjectToGCode.Application.Features.IOFile;
using _3DObjectToGCode.Application.Features.ObjToMeshConverter;
using _3DObjectToGCode.Application.Helpers;
using GeometRi;
using Microsoft.Extensions.Logging;
using ObjParser;
using SvgLib;
using System.Linq;
using Face = ObjParser.Types.Face;

namespace _3DObjectToGCode.Application.Features.CylinderObjectToSvgConverter;

public class CylinderObjectToSvgConverterSevice(IOFileService fileService, ILogger<CylinderObjectToSvgConverterSeviceFailed> logger)
{
    public IEnumerable<Point3d> Convert(MeshObject meshObject)
    {
        //var rect = new[] { new Point3d(1.0, 1.0, 1.0), new Point3d(1.0, 5.0, 1.0), new Point3d(4.0, 5.0, 1.0), new Point3d(4.0, 1.0, 1.0), new Point3d(1.0, 1.0, 1.0) };
        //var a = new Plane3d(rect[0], rect[1], rect[2]);

        //var b = new Plane3d(new Point3d(2.0, 0.0, 0.0), new Vector3d(1.0, 0.0, 0.0));

        //var intersection = b.IntersectionWith(a) as Line3d;

        //var crossPoints = rect.ToSegments()
        //                .Select(e => intersection.IntersectionWith(e) as Point3d)
        //                //.Where(p => p != null)
        //                .ToArray();


        //var triangle = new Triangle(rect[0], rect[1], rect[2]);
        //b.IntersectionWith(triangle);

        //return null;
        var size = meshObject.Size;
        var points = meshObject.Verts.Select(v => new { Id = v.Index, Point = new Point3d(v.X, v.Y, v.Z) }).ToArray();
        var boxCenter = new Point3d(
            meshObject.Size.XMin + (meshObject.Size.XMax - meshObject.Size.XMin) / 2,
            meshObject.Size.YMin + (meshObject.Size.YMax - meshObject.Size.YMin) / 2,
            meshObject.Size.ZMin + (meshObject.Size.ZMax - meshObject.Size.ZMin) / 2);

        var axisCutPlanes = new[]
        {
            new { Axis = "yz", Plane = new Plane3d(boxCenter, new Vector3d(1.0, 0.0, 0.0))},
            new { Axis = "xz", Plane = new Plane3d(boxCenter, new Vector3d(0.0, 1.0, 0.0))},
            new { Axis = "xy", Plane = new Plane3d(boxCenter, new Vector3d(0.0, 0.0, 1.0))},
        };

        var facePlanes = meshObject.Faces
        .Select(f =>
        {
            var facePoints = points.Where(p => f.VertexIndexList.Contains(p.Id))
            .ToArray();
            var faceEdgePoints = f.VertexIndexList.Select(id => facePoints.First(p => p.Id == id).Point).ToArray();

            var plane = new Triangle(faceEdgePoints[0], faceEdgePoints[1], faceEdgePoints[2]);

            //if (f.Id == 379)
            //{
            //    var a = 1;
            //}

            return new { Face = f, Plane = plane, Edges = faceEdgePoints.ToSegments() };
        })
        .ToList();

        var count = 0;
        var d = new List<object>();
        var axisCrossCurvePoints = axisCutPlanes.Select(acp =>
        {
            var crossCurveEdges = facePlanes.Select(fp =>
            {
                var intersection = acp.Plane.IntersectionWith(fp.Plane);
                if (intersection != null)
                {
                    var targetEdges = fp.Edges.Where(e => e.IsAxisPlaneCuted(acp.Plane, acp.Axis)).ToArray();

                    var crossPoints = targetEdges
                        .Select(e => acp.Plane.IntersectionWith(e.ToLine) as Point3d)
                        .Where(p => p != null && IsInsideBox(p, size))
                        .ToArray();

                    var crossPoints1 = fp.Edges
                       .Select(e => acp.Plane.IntersectionWith(e.ToLine))
                       .ToArray();

                    if (fp.Face.Id == 379)
                    {
                        var a = 1;
                    }


                    if (acp.Axis == "xy")
                    {
                        d.Add(intersection);
                        count++;
                    }

                    if (acp.Axis == "yz" && crossPoints.Length > 2)
                    {
                        var b = 0;
                    }

                    return new { FacePlane = fp, CrossPoints = crossPoints };
                }

                return null;
            })
            .Where(cp => cp != null)
            .ToArray();

            return new { Axis = acp.Axis, CrossCurveEdges = crossCurveEdges };
        })
        .ToArray();

        var circleCurves = axisCrossCurvePoints.Select(i =>
        {
            var isCircle = false;
            if (i.CrossCurveEdges.Count() < 3)
            {
                return new { i.Axis, i.CrossCurveEdges, IsCircle = isCircle };
            }

            Circle3d circle;
            try
            {
                var points = i.CrossCurveEdges.SelectMany(cce => cce.CrossPoints).Distinct().ToArray();
                circle = new Circle3d(points[0], points[1], points[2]);
                var distancesToCenter = points.Select(p => p.DistanceTo(circle.Center) - circle.R).ToArray();
                isCircle = distancesToCenter.All(d => d < 0.0001);
            }
            catch (Exception ex)
            {

            }

            return new { i.Axis, i.CrossCurveEdges, IsCircle = isCircle };
        })
        .ToArray();

        return null;

        //var profileCurvePoints = circleCurves.FirstOrDefault(cc => !cc.IsCircle).Points;

        //var facesWith4Verts = meshObject.Faces.Where(f => f.VertexIndexList.Length == 4).ToArray();
        //var targetFace = facesWith4Verts.ElementAt(facesWith4Verts.Length / 2);

        //var cutPlane = GetCylinderAxis(meshObject, new CylinderNeighbourFaces(null, null, [targetFace.Id], targetFace));

        //if (cutPlane == null)
        //{
        //    logger.LogInformation("Mesh object has not cylinder shape");
        //    return null;
        //}



        //var svgDocument = SvgDocument.Create();
        //var path = svgDocument.AddPath();
        //path.D = profileCurvePoints.ToPathString();
        //fileService.SaveSvg("cylinder_cross_slice", svgDocument.Element.OuterXml);

        //return profileCurvePoints;
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

    private bool IsInsideBox(Point3d point, Extent box)
    {
        return point.X >= box.XMin && point.X <= box.XMax &&
               point.Y >= box.YMin && point.Y <= box.YMax &&
               point.Z >= box.ZMin && point.Z <= box.ZMax;
    }

    private record CylinderNeighbourFaces(Line3d FaceNormal, Point3d Center, List<int> ProcessedFaceIds, Face NextFace);
}


