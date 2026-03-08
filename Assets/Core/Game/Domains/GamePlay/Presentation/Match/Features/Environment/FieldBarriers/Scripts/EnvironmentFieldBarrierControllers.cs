using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.FieldBarriers.Scripts
{
    public class EnvironmentFieldBarrierControllers : IEnvironmentFieldBarrierControllers
    {
        private readonly IMatchDataService _matchDataService;
        private readonly EnvironmentFieldBarrierView _fieldBarrierViewPrefab;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly List<EnvironmentFieldBarrierController> _controllers = new();
        private GameObject _parent;

        public EnvironmentFieldBarrierControllers(IMatchDataService matchDataService, EnvironmentFieldBarrierView fieldBarrierViewPrefab, PresentationGamePlayConfig gamePlayConfig)
        {
            _matchDataService = matchDataService;
            _fieldBarrierViewPrefab = fieldBarrierViewPrefab;
            _gamePlayConfig = gamePlayConfig;
        }
        
        public void InitEntryPoint()
        {
            _parent = new GameObject("EnvironmentFieldBarriersParent");
        }

        public void CreateFieldBarrier(ushort id)
        {
            var controller = new EnvironmentFieldBarrierController(id, _matchDataService, _gamePlayConfig);
            controller.CreateView(_fieldBarrierViewPrefab, _parent.transform);
            _controllers.Add(controller);
        }

        public void DestroyAll()
        {
            foreach (var controller in _controllers)
            {
                controller.Destroy();
            }
            _controllers.Clear();
        }
    }
}
