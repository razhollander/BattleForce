using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models;
using Core.Game.Domains.GamePlay.Presentation.Scripts.LayerOrders;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Scripts.Extensions;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Services.Logger.Base;
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
            _view = Object.Instantiate(prefab, parent);
            _view.name = "EnvironmentFieldBarrier_" + _id;
            var fieldBarrierModel = _matchDataService.GetFieldBarrier(_id);
            _view.transform.position = fieldBarrierModel.Position.ToUnityVector2();
            var color = _gamePlayConfig.ColorPerTeamId[fieldBarrierModel.TeamId];
            _view.SetColor(color);
            var layerOrder = LayerOrder.EnvironmentFieldBarrier;
            var mesh = fieldBarrierModel.Shape switch
            {
                FieldBarrierShape.Rectangle => MeshUtils.CreateRectangleMesh(fieldBarrierModel.Size, layerOrder),
                FieldBarrierShape.Circle => MeshUtils.CreateCircleMesh(fieldBarrierModel.CircleRadius, _gamePlayConfig.FieldBarriers.Thickness,
                    _gamePlayConfig.FieldBarriers.CircleBarrierSegmentsAmount, layerOrder)
            };
            
            _view.SetMesh(mesh);
        }

        public void Destroy()
        {
            Object.Destroy(_view.gameObject);
            _view = null;
        }
    }
}
