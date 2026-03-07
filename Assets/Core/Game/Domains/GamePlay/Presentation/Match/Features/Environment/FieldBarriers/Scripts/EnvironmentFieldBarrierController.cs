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

            var color = _gamePlayConfig.ColorPerTeamId[model.TeamId];
            color.a = 0.2f; // Make it semi-transparent
            _view.SetColor(color);

            Mesh mesh = null;
            if (model.Shape == FieldBarrierShape.Rectangle)
            {
                mesh = CreateRectangleMesh(model.Size);
            }
            else if (model.Shape == FieldBarrierShape.Circle)
            {
                mesh = CreateCircleMesh(model.Size.X);
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

        private Mesh CreateCircleMesh(float radius)
        {
            int segments = 64;
            var points = new Vector2[segments];
            float angleStep = 360f / segments;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                points[i] = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
            }
            return MeshUtils.BuildMesh(points, LayerOrder.EnvironmentFieldBarrier);
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
