using System.Collections.Generic;
using System.Numerics;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using Core.Scripts.Utils;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.FrigidBlock
{
    public class FrigidBlocksController : IFrigidBlocksController
    {
        private readonly IMatchDataService _matchDataService;
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly INetEventsDataService _netEventsDataService;
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly ConcurrentPool<FrigidBlockController> _controllersPool;
        private readonly List<FrigidBlockController> _activeControllers;

        public FrigidBlocksController(IMatchDataService matchDataService, IPhysicsSimulator physicsSimulator, INetEventsDataService netEventsDataService,
            ISimulationGamePlayConfigService gamePlayConfigService, NetworkConfig networkConfig, SharedGamePlayConfig sharedGamePlayConfig)
        {
            _matchDataService = matchDataService;
            _physicsSimulator = physicsSimulator;
            _netEventsDataService = netEventsDataService;
            _gamePlayConfigService = gamePlayConfigService;
            _sharedGamePlayConfig = sharedGamePlayConfig;

            var maxFrigidBlocks = networkConfig.MaxCap.ConcurrentFrigidBlocks;
            _controllersPool = new ConcurrentPool<FrigidBlockController>(
                () => new FrigidBlockController(matchDataService, gamePlayConfigService), maxFrigidBlocks);
            _activeControllers = new List<FrigidBlockController>(maxFrigidBlocks);
        }

        public void ShootFrigidBlock(ushort casterPlayerId, Vector2 position, Vector2 direction, int tick, int cooldownEndTick)
        {
            var config = _gamePlayConfigService.GamePlayConfig.Talents.FrigidBlockTalentConfig;
            var size = _sharedGamePlayConfig.FrigidBlockSize.ToNumericsVector2();
            var velocity = direction * config.ProjectileSpeed;
            var rotation = new Vector2(-direction.Y, direction.X);

            var block = _matchDataService.AddFrigidBlock(casterPlayerId, position, rotation, velocity);
            _physicsSimulator.AddFrigidBlock(block.Id, position, rotation, size, velocity, config.Density, config.Restitution, config.LinearDamping, config.AngularDamping);

            var controller = _controllersPool.Get();
            controller.Init(block.Id);
            _activeControllers.Add(controller);

            _netEventsDataService.AddShootFrigidBlockNetEvent(tick, block, cooldownEndTick);
        }

        public void OnTick(int tick, float deltaTime)
        {
            for (int i = _activeControllers.Count - 1; i >= 0; i--)
            {
                var controller = _activeControllers[i];
                if (controller.IsIdleLongEnoughToBeDestroyed(tick, deltaTime))
                {
                    DestroyBlock(controller.BlockId, tick);
                    _activeControllers.RemoveAt(i);
                    _controllersPool.Return(controller);
                }
            }
        }

        public void ResetData()
        {
            for (int i = 0; i < _activeControllers.Count; i++)
            {
                _controllersPool.Return(_activeControllers[i]);
            }

            _activeControllers.Clear();
        }

        private void DestroyBlock(ushort blockId, int tick)
        {
            if (_matchDataService.SimulationState.TryGetFrigidBlockById(blockId, out _))
            {
                _physicsSimulator.RemoveFrigidBlock(blockId);
                _matchDataService.SimulationState.RemoveFrigidBlockById(blockId);
            }

            _netEventsDataService.AddDestroyFrigidBlockNetEvent(tick, blockId);
        }
    }
}
