using _3DObjectToGCode.Application.Features.IOFile;
using _3DObjectToGCode.Application.Features.ObjToMeshConverter;
using _3DObjectToGCode.Application.Helpers;
using GeometRi;
using Microsoft.Extensions.Logging;
using ObjParser;
using SvgLib;
using System.Linq;
using static GeometRi.ConvexPolyhedron;
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

                    if (fp.Face.Id == 0 && acp.Axis == "xz")
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
                isCircle = distancesToCenter.All(d => d < 0.1);
            }
            catch (Exception ex)
            {

            }

            return new { i.Axis, i.CrossCurveEdges, IsCircle = isCircle };
        })
        .ToArray();

        var targetProfileCurve = circleCurves.FirstOrDefault(cc => !cc.IsCircle);
        var profileCurveEdges = targetProfileCurve.CrossCurveEdges
            .Select(cce => new CrossEdge(cce.FacePlane.Face.Id, cce.CrossPoints.ToSegments(false)[0])).ToArray();

        var profileCurvePoints = GetCrossEdgePoints(profileCurveEdges, profileCurveEdges[0], null, new List<Point3d>()).ToArray();

        //var facesWith4Verts = meshObject.Faces.Where(f => f.VertexIndexList.Length == 4).ToArray();
        //var targetFace = facesWith4Verts.ElementAt(facesWith4Verts.Length / 2);

        //var cutPlane = GetCylinderAxis(meshObject, new CylinderNeighbourFaces(null, null, [targetFace.Id], targetFace));

        //if (cutPlane == null)
        //{
        //    logger.LogInformation("Mesh object has not cylinder shape");
        //    return null;
        //}



        var svgDocument = SvgDocument.Create();
        svgDocument.Units = "m";
        var path = svgDocument.AddPath();
        path.D = profileCurvePoints.ToPathString(targetProfileCurve.Axis);
        path.SetStyle("stroke-width", "0.05");
        path.SetStyle("stroke", "red");

        fileService.SaveSvg("cylinder_cross_slice", svgDocument.Element.OuterXml);

        return profileCurvePoints;
    }

    private IEnumerable<Point3d> GetCrossEdgePoints(IEnumerable<CrossEdge> crossEdges,
        CrossEdge targetCrossEdge,
        bool? p1Added,
        List<Point3d> profileCurvePoints)
    {
        var processedFaceIds = new List<int> { targetCrossEdge.FaceId };
        var targetEdge = targetCrossEdge.Edge;
        //processedFaceIds.Add(targetCrossEdge.FaceId);

        //var itemsWithSameEdge = crossEdges.Where(pce => !processedFaceIds.Contains(pce.FaceId) &&
        //  pce.Edge == targetCrossEdge.Edge).ToArray();

        if (p1Added == null)
        {
            profileCurvePoints.AddRange([targetEdge.P1, targetEdge.P2]);
        }
        else
        {
            profileCurvePoints.Add(p1Added.Value ? targetEdge.P2 : targetEdge.P1);
        }

        var itemsWithSameEdge = crossEdges.Where(pce => pce.FaceId != targetCrossEdge.FaceId &&
              pce.Edge == targetEdge).ToArray();

        processedFaceIds.AddRange(itemsWithSameEdge.Select(i => i.FaceId).ToArray());

        var notProcessedCrossEdges = crossEdges
            .Where(ce => !processedFaceIds.Contains(ce.FaceId))
            .ToArray();

        var nextEdge = notProcessedCrossEdges.FirstOrDefault(ce =>
            PointEquals(ce.Edge.P1, targetEdge.P2) || PointEquals(ce.Edge.P2, targetEdge.P1));

        if (targetCrossEdge.FaceId == 1583)
        {
            var d = notProcessedCrossEdges.FirstOrDefault(i => i.FaceId == 1582);
            var c = PointEquals(d.Edge.P1, targetEdge.P2);
            var b = PointEquals(d.Edge.P2, targetEdge.P2);
        }

        if (nextEdge != null)
        {
            GetCrossEdgePoints(notProcessedCrossEdges, nextEdge, PointEquals(nextEdge.Edge.P1, targetEdge.P2), profileCurvePoints);
        }

        return profileCurvePoints.Distinct();
    }

    private bool PointEquals(Point3d p1, Point3d p2)
    {
        return p1.DistanceTo(p2) <= 0.01;
    }

    //private Face[] GetOrderedFacesLoop(IEnumerable<Face> faces, CylinderNeighbourFaces cylinderNeighbourFaces)
    //{
    //    var result = GetNextCylinderLoopFace(meshObject, cylinderNeighbourFaces);
    //    if (result?.FaceNormal != null && result.NextFace != null)
    //    {
    //        return GetCylinderAxis(meshObject, result);
    //    }

    //    return result?.FaceNormal != null && result?.Center != null ?
    //        new Plane3d(result.Center, result.FaceNormal.Direction) : null;
    //}

    //private CylinderNeighbourFaces? GetNextLoopFace(IEnumerable<int> faceIds, IEnumerable<Face> faces, Face targetFace, List<int> processedFaceIds)
    //{
    //    var notProcessedFaces = faces
    //        .Where(f => processedFaceIds.Count > 1 ? !processedFaceIds.Contains(f.Id) : true);
    //    var notProcessedFaceIds = notProcessedFaces.Select(f => f.Id).ToArray();

    //    var neighbourFaces = notProcessedFaces
    //        .Where(f => f.VertexIndexList.Intersect(targetFace.VertexIndexList).Any());
    //    var neighbourFaceIds = neighbourFaces.Select(f => f.Id).ToArray();

    //    var nextNeighbourFaceIds = notProcessedFaceIds.Intersect(neighbourFaceIds);
    //    if (nextNeighbourFaceIds.Count() > 1)
    //    {
    //        throw new Exception("More then one neighbor, can not select");
    //    }

    //    var nextFaceId = nextNeighbourFaceIds.First();
    //    var nextFace = nextFaceId != null ? neighbourFaces.First(f => f.Id == nextFaceId) : null;

    //    processedFaceIds.AddRange();


    //}



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

    private record CrossEdge(int FaceId, Segment3d Edge);

    private record CylinderNeighbourFaces(Line3d FaceNormal, Point3d Center, List<int> ProcessedFaceIds, Face NextFace);
}


