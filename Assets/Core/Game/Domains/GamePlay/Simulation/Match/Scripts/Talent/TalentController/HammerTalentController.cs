/*using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent.TalentController
{
    public class HammerTalentController : ITalentController
    {
        private ushort _casterPlayerId;
        
        private readonly INetEventsDataService _netEventsDataService;
        private readonly IMatchDataService _matchDataService;

        public HammerTalentController(ushort casterPlayerId, INetEventsDataService netEventsDataService, IMatchDataService matchDataService)
        {
            _casterPlayerId = casterPlayerId;
            _netEventsDataService = netEventsDataService;
            _matchDataService = matchDataService;
        }

        public void OnTick(int tick, float deltaTime)
        {
            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
            var closetPlayerToCaster = FindClosestPlayerToCaster(casterPlayerState, _matchDataService.SimulationState);
            
            //SwapPlayersMatchData(casterPlayerState, ref closetPlayerToCaster);
           
            // _netEventsDataService.AddPlayersSwapEvent(tick, _casterPlayerId, closetPlayerToCaster.Id, casterPlayerState.Spaceship.Transform.Position,
            //     closetPlayerToCaster.Spaceship.Transform.Position, casterPlayerState.Spaceship.Transform.Direction, closetPlayerToCaster.Spaceship.Transform.Direction);
        }

        public void ResetData()
        {
            
        }

        private void SwapPlayersMatchData(PlayerStateS2C casterPlayerState, ref PlayerStateS2C closetPlayerToCaster)
        {
            (casterPlayerState.Spaceship.Transform.Position, closetPlayerToCaster.Spaceship.Transform.Position) = (closetPlayerToCaster.Spaceship.Transform.Position, casterPlayerState.Spaceship.Transform.Position);
            (casterPlayerState.Spaceship.Transform.Direction, closetPlayerToCaster.Spaceship.Transform.Direction) = (closetPlayerToCaster.Spaceship.Transform.Direction, casterPlayerState.Spaceship.Transform.Direction);
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

        public TalentType TalentType => TalentType.Hammer;
        public bool IsCurrentlyActive => true;//todo change this
        public void ProcessTalentInput(bool isTalentInputPressed, int tick, float deltaTime)
        {
            
        }

        public void StopIfActive(int tick)
        {

        }
    }
}*/