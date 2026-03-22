using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.KOProjectiles.Scripts;
using CoreDomain.Scripts.Services.UpdateService;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.KOProjectiles.Scripts.Mvc
{
    public class KOProjectilesControllers : IKOProjectilesControllers, IUpdatable
    {
        private readonly IMatchDataService _matchDataService;
        private readonly KOProjectilePool _koProjectilePool;
        private readonly ICachedPresentationEventsService _cachedPresentationEventsService;
        private readonly IUpdateSubscriptionService _updateSubscriptionService;
        private readonly Dictionary<ushort, KOProjectileController> _controllers;

        public KOProjectilesControllers(IMatchDataService matchDataService, KOProjectilePool koProjectilePool,
            ICachedPresentationEventsService cachedPresentationEventsService,
            IUpdateSubscriptionService updateSubscriptionService)
        {
            _matchDataService = matchDataService;
            _koProjectilePool = koProjectilePool;
            _cachedPresentationEventsService = cachedPresentationEventsService;
            _updateSubscriptionService = updateSubscriptionService;
            _controllers = new Dictionary<ushort, KOProjectileController>();
        }

        public void InitEntryPoint()
        {
            _updateSubscriptionService.RegisterUpdatable(this);
        }

        public void InitExitPoint()
        {
            _updateSubscriptionService.UnregisterUpdatable(this);

            foreach (var kvp in _controllers)
            {
                _koProjectilePool.Return(kvp.Value.View);
            }
            _controllers.Clear();
        }

        public void ManagedUpdate()
        {
        }

        public void HandleCreateEvents(List<CreateKOProjectileNetEventS2C> events)
        {
            for (int i = 0; i < events.Count; i++)
            {
                var netEvent = events[i];
                var koProjectileId = netEvent.KoProjectileId;
                if (!_controllers.ContainsKey(koProjectileId))
                {
                    var view = _koProjectilePool.Get();
                    var model = _matchDataService.GetKOProjectile(koProjectileId);
                    var casterModel = _matchDataService.GetPlayer(model.CasterPlayerId);
                    var controller = new KOProjectileController(view, model, casterModel);
                    _controllers.Add(koProjectileId, controller);
                }
            }
        }

        public void HandleDeactivateEvents(List<DeactivateKOTalentNetEventS2C> events)
        {
            for (int i = 0; i < events.Count; i++)
            {
                var netEvent = events[i];
                var koProjectileId = netEvent.KoProjectileId;
                if (_controllers.TryGetValue(koProjectileId, out var controller))
                {
                    _koProjectilePool.Return(controller.View);
                    _controllers.Remove(koProjectileId);
                }
            }
        }

        public void UpdateKOProjectilesTransform()
        {
            foreach (var kvp in _controllers)
            {
                kvp.Value.UpdateTransform();
            }
        }
    }
}
