using System;
using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Features.PowerUps.Scripts.Views;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using CoreDomain.Scripts.Services.UpdateService;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Features.PowerUps.Scripts
{
    public class PowerUpBallsController : IInitializable, IDisposable, IGuiTickable
    {
        private readonly SimulationStateS2C _simulationState;
        private readonly SimulationNetEventsHandler _simulationNetEventsHandler;
        private readonly IUpdateSubscriptionService _updateSubscriptionService;
        private readonly PowerUpBallView _prefab; // Injected
        private readonly Transform _root; // Where to spawn

        private readonly Dictionary<ushort, PowerUpBallView> _powerUps = new Dictionary<ushort, PowerUpBallView>();

        public PowerUpBallsController(
            SimulationStateS2C simulationState,
            SimulationNetEventsHandler simulationNetEventsHandler,
            IUpdateSubscriptionService updateSubscriptionService,
            [Inject(Id = "PowerUpBallPrefab")] PowerUpBallView prefab,
            [Inject(Id = "GamePlayRoot")] Transform root)
        {
            _simulationState = simulationState;
            _simulationNetEventsHandler = simulationNetEventsHandler;
            _updateSubscriptionService = updateSubscriptionService;
            _prefab = prefab;
            _root = root;
        }

        public void Initialize()
        {
            _simulationNetEventsHandler.PowerUpSpawned += OnPowerUpSpawned;
            _simulationNetEventsHandler.PowerUpObtained += OnPowerUpObtained;
            _updateSubscriptionService.RegisterGuiTickable(this);
        }

        public void Dispose()
        {
            _simulationNetEventsHandler.PowerUpSpawned -= OnPowerUpSpawned;
            _simulationNetEventsHandler.PowerUpObtained -= OnPowerUpObtained;
            _updateSubscriptionService.UnregisterGuiTickable(this);
        }

        private void OnPowerUpSpawned(PowerUpSpawnedNetEventsS2C evt)
        {
            if (_powerUps.ContainsKey(evt.Id)) return;

            var instance = UnityEngine.Object.Instantiate(_prefab, _root);
            instance.Init(evt.Id, evt.Type);
            instance.UpdatePosition(evt.Position);
            _powerUps.Add(evt.Id, instance);
        }

        private void OnPowerUpObtained(PowerUpObtainedNetEventS2C evt)
        {
            if (_powerUps.TryGetValue(evt.PowerUpId, out var view))
            {
                UnityEngine.Object.Destroy(view.gameObject);
                _powerUps.Remove(evt.PowerUpId);
            }
        }

        public void Tick(float deltaTime)
        {
            // Sync positions from SimulationStateS2C
            var powerUpsState = _simulationState.PowerUps;

            // Update existing and spawn new
            for (int i = 0; i < powerUpsState.Count; i++)
            {
                var state = powerUpsState[i];
                if (_powerUps.TryGetValue(state.Id, out var view))
                {
                    view.UpdatePosition(state.Position);
                }
                else
                {
                     SpawnViewIfNotExists(state);
                }
            }

            // Remove ghosts (views that are no longer in state)
            // Need to collect keys first to avoid modification during iteration
            List<ushort> toRemove = null;
            foreach (var kvp in _powerUps)
            {
                bool found = false;
                for (int i = 0; i < powerUpsState.Count; i++)
                {
                    if (powerUpsState[i].Id == kvp.Key)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    if (toRemove == null) toRemove = new List<ushort>();
                    toRemove.Add(kvp.Key);
                }
            }

            if (toRemove != null)
            {
                foreach (var id in toRemove)
                {
                    if (_powerUps.TryGetValue(id, out var view))
                    {
                        UnityEngine.Object.Destroy(view.gameObject);
                        _powerUps.Remove(id);
                    }
                }
            }
        }

        private void SpawnViewIfNotExists(PowerUpS2C state)
        {
             if (!_powerUps.ContainsKey(state.Id))
            {
                var instance = UnityEngine.Object.Instantiate(_prefab, _root);
                instance.Init(state.Id, state.Type);
                instance.UpdatePosition(state.Position);
                _powerUps.Add(state.Id, instance);
            }
        }
    }
}
