using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using CoreDomain.Scripts.Services.Logger.Base;
using Zenject;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent.TalentController
{
    public class SwapTalentController : ITalentController
    {
        private ushort _casterPlayerId;

        private readonly INetEventsDataService _netEventsDataService;
        private readonly IMatchDataService _matchDataService;
        private readonly SimulationGamePlayConfig _gamePlayConfig;
        private readonly IPhysicsSimulator _physicsSimulator;

        public TalentType TalentType => TalentType.Swap;
        public bool IsCurrentlyActive { get; private set; }

        private float _currentRadius;
        private int _startTick;

        public SwapTalentController(INetEventsDataService iNetEventsDataService, IMatchDataService matchDataService, SimulationGamePlayConfig gamePlayConfig, IPhysicsSimulator physicsSimulator)
        {
            _netEventsDataService = iNetEventsDataService;
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
            _physicsSimulator = physicsSimulator;
        }

        public void SetCasterId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
        }

        public void ProcessTalentInput(bool isTalentInputPressed, int tick, float deltaTime)
        {
            if (IsCurrentlyActive || !isTalentInputPressed)
            {
                return;
            }

            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);

            // Check cooldown
            if (casterPlayerState.Spaceship.TalentsState.GetCurrentSelectedTalent().IsOnCooldown())
            {
                return;
            }

            IsCurrentlyActive = true;
            _startTick = tick;
            _currentRadius = 0f;

           // _matchDataService.AddSwapField(_casterPlayerId, casterPlayerState.Spaceship.Transform.Position, 0);
            _physicsSimulator.AddSwapField(_casterPlayerId, casterPlayerState.Spaceship.Transform.Position);
            _netEventsDataService.AddCreateSwapFieldNetEvent(tick, _casterPlayerId);
        }

        public void Stop()
        {
            if (IsCurrentlyActive)
            {
                IsCurrentlyActive = false;
                _physicsSimulator?.RemoveSwapField(_casterPlayerId);
            }
        }

        public void OnTick(int tick)
        {
            if (!IsCurrentlyActive)
                return;

            float deltaTime = 1f / 60f; // Assuming standard tick rate
            var config = _gamePlayConfig.Talents.SwapTalentConfig;

            _currentRadius += (config.MaxRadius / config.GrowDurationSeconds) * deltaTime;

            if (_currentRadius >= config.MaxRadius)
            {
                _currentRadius = config.MaxRadius;
                CompleteTalent(tick, deltaTime);
            }
            else
            {
                // Follow the caster
                var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
                _physicsSimulator.UpdateSwapField(_casterPlayerId, casterPlayerState.Spaceship.Transform.Position, _currentRadius);
            }
        }

        public void CompleteTalentWithEnemy(PlayerStateS2C enemyPlayer, int tick)
        {
            if (!IsCurrentlyActive) return;

            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);

            SwapPlayersTransform(casterPlayerState, enemyPlayer);

            float deltaTime = 1f / 60f;
            var cooldownEndTick = TickUtils.GetTickInTime(tick, casterPlayerState.Spaceship.TalentsState.GetCurrentSelectedTalent().MaxCooldown, deltaTime);
            ref var currentSelectedTalent = ref casterPlayerState.Spaceship.TalentsState.GetCurrentSelectedTalent();
            currentSelectedTalent.CooldownEndTick = cooldownEndTick;

            _netEventsDataService.AddPlayersSwapEvent(tick, _casterPlayerId, enemyPlayer.Id,
                casterPlayerState.Spaceship.Transform.Position, enemyPlayer.Spaceship.Transform.Position,
                casterPlayerState.Spaceship.Transform.Direction, enemyPlayer.Spaceship.Transform.Direction,
                cooldownEndTick);

            _netEventsDataService.AddDestroySwapFieldNetEvent(tick, _casterPlayerId);

            Stop();
        }

        private void CompleteTalent(int tick, float deltaTime)
        {
            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
            var cooldownEndTick = TickUtils.GetTickInTime(tick, casterPlayerState.Spaceship.TalentsState.GetCurrentSelectedTalent().MaxCooldown, deltaTime);
            ref var currentSelectedTalent = ref casterPlayerState.Spaceship.TalentsState.GetCurrentSelectedTalent();
            currentSelectedTalent.CooldownEndTick = cooldownEndTick;

            _netEventsDataService.AddDestroySwapFieldNetEvent(tick, _casterPlayerId);
            Stop();
        }


        private void SwapPlayersTransform(PlayerStateS2C casterPlayerState, PlayerStateS2C closetPlayerToCaster)
        {
            (casterPlayerState.Spaceship.Transform.Position, closetPlayerToCaster.Spaceship.Transform.Position) =
                (closetPlayerToCaster.Spaceship.Transform.Position, casterPlayerState.Spaceship.Transform.Position);

            (casterPlayerState.Spaceship.Transform.Direction, closetPlayerToCaster.Spaceship.Transform.Direction) =
                (closetPlayerToCaster.Spaceship.Transform.Direction, casterPlayerState.Spaceship.Transform.Direction);
            
            (casterPlayerState.Spaceship.Transform.Velocity, closetPlayerToCaster.Spaceship.Transform.Velocity) =
                (closetPlayerToCaster.Spaceship.Transform.Velocity, casterPlayerState.Spaceship.Transform.Velocity);
            
            (casterPlayerState.Spaceship.Transform.AngularVelocity, closetPlayerToCaster.Spaceship.Transform.AngularVelocity) =
                (closetPlayerToCaster.Spaceship.Transform.AngularVelocity, casterPlayerState.Spaceship.Transform.AngularVelocity);
        }

        private PlayerStateS2C FindClosestPlayerToCaster(PlayerStateS2C casterPlayerState, MatchSimulationStateS2C simulationStateS2C)
        {
            var players = simulationStateS2C.Players;
            var span = players.AsSpan();

            var casterPos = casterPlayerState.Spaceship.Transform.Position;

            float smallestDistanceSqrd = float.MaxValue;
            int closePlayerIndex = -1;

            for (int i = 0; i < span.Length; i++)
            {
                var playerState = span[i];
                bool isCaster = playerState.Id == _casterPlayerId;

                if (isCaster)
                    continue;

                var otherPlayerPos = playerState.Spaceship.Transform.Position;
                var distSq = Vector2.DistanceSquared(otherPlayerPos, casterPos);

                if (distSq < smallestDistanceSqrd)
                {
                    smallestDistanceSqrd = distSq;
                    closePlayerIndex = i;
                }
            }

            if (closePlayerIndex == -1)
                throw new InvalidOperationException("No other players found (only caster exists).");

            return players.GetByIndex(closePlayerIndex);
        }
    }
}