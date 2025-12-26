using _3DObjectToGCode.Application.Features.ObjToMeshConverter;
using GeometRi;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using ObjParser;
using ObjParser.Types;
using SvgLib;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Face = ObjParser.Types.Face;

namespace _3DObjectToGCode.Application.Features.CylinderObjectToSvgConverter;

public class CylinderObjectToSvgConverterSevice(ILogger<CylinderObjectToSvgConverterSevice> logger)
{
    public async Task<SvgDocument?> Convert(MeshObject meshObject)
    {

        var middleVert = meshObject.Verts.ElementAt(meshObject.Verts.Count() / 2);
        var targetFace = meshObject.Faces.First(f => f.VertexIndexList.Contains(middleVert.Index));

        var axis = GetCylinderAxis(meshObject, new CylinderNeighbourFaces(null, [targetFace.Id], targetFace));

        if (axis == null)
        {
            logger.LogInformation("Mesh object has not cylinder shape");
            return null;
        }
    }

    private Vector3d? GetCylinderAxis(MeshObject meshObject, CylinderNeighbourFaces cylinderNeighbourFaces)
    {
        var result = GetNextCylinderLoopFace(meshObject, cylinderNeighbourFaces);
        if (result?.FaceNormal != null && result.NextFace != null)
        {
            return GetCylinderAxis(meshObject, result);
        }

        return result?.FaceNormal?.Direction?.OrthogonalVector;
    }

    private CylinderNeighbourFaces? GetNextCylinderLoopFace(MeshObject meshObject, CylinderNeighbourFaces cylinderNeighbourFaces)
    {
        var (intersectionPoint, processedFaceIds, targetFace) = cylinderNeighbourFaces;
        var neighbourFaces = meshObject.Faces
            .Where(f => f.VertexIndexList.Length == 4 && f.VertexIndexList.Intersect(targetFace.VertexIndexList).Any())
            .ToArray();

        if (!neighbourFaces.Any() || neighbourFaces.Length < 3)
        {
            return null;
        }

        var neighbourFacesNormals = neighbourFaces.Select(f =>
        {
            var points = meshObject.Verts.Where(v => f.VertexIndexList.Contains(v.Index))
                .Select(v => new Point3d(v.X, v.Y, v.Z)).ToArray();

            var plane = new Plane3d(points[0], points[1], points[2]);
            return new { FaceId = f.Id, plane.Normal };
        })
        .ToArray();

        var firstNormal = neighbourFacesNormals.First(n => n.FaceId == targetFace.Id).Normal.ToLine;
        var intersections = neighbourFacesNormals
        .Where(n => n.FaceId != targetFace.Id)
            .Select(v => new { v.FaceId, Point = firstNormal.IntersectionWith(v.Normal.ToLine) as Point3d })
            .Where(p => p.Point != null)
            .ToArray();

        if (!intersections.Any())
        {
            return null;
        }

        var firstPoint = intersections[0].Point;
        var nextFaceId = intersections.First(p => processedFaceIds.Contains(p.FaceId)).FaceId;
        var nextFace = meshObject.Faces.First(f => f.Id == nextFaceId);

        processedFaceIds.AddRange(intersections.Select(i => i.FaceId).ToArray());

        return intersections.Skip(1).All(p => firstPoint.DistanceTo(p.Point) < 0.001) ?
            new CylinderNeighbourFaces(firstNormal, processedFaceIds, nextFace) : null;
    }

    private void a()
    {
        var watch = Stopwatch.StartNew();

        Console.WriteLine($"Start parsing {_fileName}!");
        Console.WriteLine();

        var content = await _file.ReadObjFile();
        var contentWitoutComments = content.Where(l => !l.StartsWith("#"));

        var meshesText = string.Join(Environment.NewLine, contentWitoutComments)
            .Split("o ")
            .Where(t => !(string.IsNullOrEmpty(t) || t == "\r\n")).ToList();

        var meshes = new List<Mesh>();
        meshesText.ForEach(t =>
        {
            var meshLines = t.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var obj = new Obj();
            obj.VertexListShift = meshes.Any() ? meshes.Last().Obj.VertexList.Last().Index : 0;
            obj.NormalListShift = meshes.Any() ? meshes.Last().Obj.NormalList.Last().Index : 0;
            obj.LoadObj(meshLines.Skip(1));
            meshes.Add(new Mesh
            {
                Name = meshLines[0],
                Obj = obj
            });
        });

        var meshesObjects = meshes.Select((mesh, i) =>
        {
            Console.WriteLine($"Starting process mesh - {mesh.Name}");

            var meshObjectsParser = new MeshObjectsParser();
            var meshObjects = meshObjectsParser.Parse(mesh);

            var edgeLoopParser = new EdgeLoopParser();
            var meshObjectsLoops = meshObjects.Select(mo => edgeLoopParser.GetMeshObjectsLoops(mo))
                .SelectMany(mol => mol.Objects).ToList();

            Console.WriteLine($"Converted to loops mesh - {mesh.Name}, loops - {meshObjectsLoops.Count()}");
            Console.WriteLine();
            Console.WriteLine($"Processed meshes {i + 1}/{meshes.Count}");
            Console.WriteLine();

            return new MeshObjects
            {
                MeshName = mesh.Name,
                Objects = meshObjectsLoops
            };
        }).ToList();

        watch.Stop();

        var objects = meshesObjects.SelectMany(mo => mo.Objects).ToArray();
        var resultCurvesCount = objects.Select(o => o.Loops.Count()).Sum();

        _statistics.ObjectsCount = objects.Length;

        Console.WriteLine($"Finished parsing, processed {meshesObjects.Count()} meshes, received {objects.Length} objects," +
            $" generated {resultCurvesCount} curves. Took - {watch.ElapsedMilliseconds / 1000.0} sec");
        Console.WriteLine();

        return meshesObjects;

    }

    private record CylinderNeighbourFaces(Line3d FaceNormal, List<int> ProcessedFaceIds, Face NextFace);
}


