using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.LayerOrders;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Scripts.Extensions;
using Core.Scripts.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.FieldBarriers.Scripts
{
    public class EnvironmentFieldBarrierController
    {
        private EnvironmentFieldBarrierView _view;
        private readonly IMatchDataService _matchDataService;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly ushort _id;

        public EnvironmentFieldBarrierController(ushort id, IMatchDataService matchDataService, PresentationGamePlayConfig gamePlayConfig)
        {
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
            _id = id;
        }

        public void CreateView(EnvironmentFieldBarrierView prefab, Transform parent)
        {
            var model = _matchDataService.GetFieldBarrier(_id);
            if (model == null) return;

            _view = Object.Instantiate(prefab, parent);
            _view.name = "EnvironmentFieldBarrier_" + _id;

            _view.transform.position = model.Position.ToUnityVector2();
            var circleRadius = model.Size.ToUnityVector2();
          //  _view.transform.localScale = circleRadius * 2;
            var color = _gamePlayConfig.ColorPerTeamId[model.TeamId];
            //color.a = 0.2f; // Make it semi-transparent
            _view.SetColor(color);
            _view.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
            Mesh mesh = null;
            if (model.Shape == FieldBarrierShape.Rectangle)
            {
                mesh = CreateRectangleMesh(model.Size);
            }
            else if (model.Shape == FieldBarrierShape.Circle)
            {
                mesh = CreateCircleMesh(model.Size.X, 0.2f);
            }
            
            if (mesh != null)
            {
                _view.SetMesh(mesh);
            }
        }

        private Mesh CreateRectangleMesh(System.Numerics.Vector2 size)
        {
            var halfSize = size * 0.5f;
            var points = new Vector2[]
            {
                new Vector2(-halfSize.X, -halfSize.Y),
                new Vector2(halfSize.X, -halfSize.Y),
                new Vector2(halfSize.X, halfSize.Y),
                new Vector2(-halfSize.X, halfSize.Y)
            };
            return MeshUtils.BuildMesh(points, LayerOrder.EnvironmentFieldBarrier);
        }


private Mesh CreateCircleMesh(float radius, float thickness)
{
    int totalSegments = 64;
    int halfSegments = totalSegments / 2;

    float innerRadius = radius;
    float outerRadius = radius + thickness;

    // 1. Generate and triangulate the Top Half (0 to 180 degrees)
    Vector2[] topHalfPoints = GenerateHalfRing(0f, 180f, halfSegments, innerRadius, outerRadius);
    Mesh topHalfMesh = MeshUtils.BuildMesh(topHalfPoints, LayerOrder.EnvironmentFieldBarrier);

    // 2. Generate and triangulate the Bottom Half (180 to 360 degrees)
    Vector2[] bottomHalfPoints = GenerateHalfRing(180f, 360f, halfSegments, innerRadius, outerRadius);
    Mesh bottomHalfMesh = MeshUtils.BuildMesh(bottomHalfPoints, LayerOrder.EnvironmentFieldBarrier);

    // 3. Combine the two halves into a single Mesh
    CombineInstance[] combine = new CombineInstance[2];
    
    combine[0].mesh = topHalfMesh;
    combine[0].transform = Matrix4x4.identity;
    
    combine[1].mesh = bottomHalfMesh;
    combine[1].transform = Matrix4x4.identity;

    Mesh finalMesh = new Mesh();
    finalMesh.CombineMeshes(combine, true, false);

    // // 4. Cleanup temporary meshes to prevent memory leaks in Unity
    // if (Application.isPlaying)
    // {
    //     Destroy(topHalfMesh);
    //     Destroy(bottomHalfMesh);
    // }
    // else
    // {
    //     DestroyImmediate(topHalfMesh);
    //     DestroyImmediate(bottomHalfMesh);
    // }

    return finalMesh;
}

// Helper method to create a perfect "C" shaped non-intersecting polygon
private Vector2[] GenerateHalfRing(float startAngle, float endAngle, int segments, float innerRadius, float outerRadius)
{
    // A half ring needs (segments + 1) for the outer edge, and (segments + 1) for the inner edge
    var points = new Vector2[(segments + 1) * 2];

    // Outer Arc (Counter-Clockwise)
    for (int i = 0; i <= segments; i++)
    {
        // Calculate interpolation factor (0.0 to 1.0)
        float t = (float)i / segments; 
        float angle = Mathf.Lerp(startAngle, endAngle, t) * Mathf.Deg2Rad;
        points[i] = new Vector2(Mathf.Cos(angle) * outerRadius, Mathf.Sin(angle) * outerRadius);
    }

    // Inner Arc (Clockwise)
    for (int i = 0; i <= segments; i++)
    {
        // Calculate interpolation factor backwards (1.0 to 0.0)
        float t = (float)(segments - i) / segments; 
        float angle = Mathf.Lerp(startAngle, endAngle, t) * Mathf.Deg2Rad;
        
        // Offset by (segments + 1) to place them in the second half of the array
        points[(segments + 1) + i] = new Vector2(Mathf.Cos(angle) * innerRadius, Mathf.Sin(angle) * innerRadius);
    }

    return points;
}

        public void Destroy()
        {
            if (_view != null)
            {
                Object.Destroy(_view.gameObject);
                _view = null;
            }
        }
    }
}
