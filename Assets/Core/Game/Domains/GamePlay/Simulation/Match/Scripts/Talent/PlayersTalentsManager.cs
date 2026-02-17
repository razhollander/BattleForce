using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Scripts.Network;
using Core.Scripts.Utils;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent
{
    public class PlayersTalentsManager : IPlayersTalentsManager
    {
        private readonly IMatchDataService _matchDataService;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly SimulationGamePlayConfig _gamePlayConfig;
        private readonly Dictionary<int, PlayerTalentControllers> _talentControllersPerPlayer;
        private readonly ConcurrentPool<PlayerTalentControllers> _talentControllersPool;

        public PlayersTalentsManager(NetworkConfig networkConfig, IMatchDataService matchDataService, SharedGamePlayConfig sharedGamePlayConfig, INetEventsDataService iNetEventsDataService, SimulationGamePlayConfig gamePlayConfig)
        {
            _matchDataService = matchDataService;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _gamePlayConfig = gamePlayConfig;
            _talentControllersPerPlayer = new Dictionary<int, PlayerTalentControllers>(networkConfig.MaxCap.ConcurrentPlayers);
            _talentControllersPool = new ConcurrentPool<PlayerTalentControllers>(()=> new PlayerTalentControllers(iNetEventsDataService, matchDataService),networkConfig.MaxCap.ConcurrentPlayers);
        }

        public void AddPlayer(ushort playerId)
        {
            _talentControllersPerPlayer.Add(playerId, _talentControllersPool.Get());
        }

        public void RemovePlayer(ushort playerId)
        {
            _talentControllersPool.Return(_talentControllersPerPlayer[playerId]);
            _talentControllersPerPlayer.Remove(playerId);
        }

        public bool TryAddTalentToPlayer(TalentType talentType, ushort playerId, out TalentStateS2C newTalent)
        {
            var playerState = _matchDataService.SimulationState.GetPlayerById(playerId);

            if (DoesPlayerHaveTalent(playerState, talentType))
            {
                newTalent = default;
                return false;
            }
            
            var didPlayerReachMaxTalents = playerState.Spaceship.TalentsState.Talents.Count == _sharedGamePlayConfig.MaxConcurrentTalentsForPlayer;
            if (didPlayerReachMaxTalents)
            {
                newTalent = ReplaceTalentWithCurrentSelectedTalent(talentType, playerState);
            }
            else
            {
                newTalent = AddTalentToPlayer(talentType, playerState);
            }
            
            return true;
        }

        private bool DoesPlayerHaveTalent(PlayerStateS2C playerState, TalentType talentType)
        {
            foreach (var talentState in playerState.Spaceship.TalentsState.Talents.AsSpan())
            {
                if (talentType == talentState.TalentType)
                {
                    return true;
                }
            }

            return false;
        }

        public bool TrySwitchToNextTalent(ushort playerId)
        {
            var playerState = _matchDataService.SimulationState.GetPlayerById(playerId);
            var talents = playerState.Spaceship.TalentsState;
            if (talents.Talents.Count <=1)
            {
                return false;
            }

            talents.SelectedTalentIndex++;
            if (talents.SelectedTalentIndex >= talents.Talents.Count)
            {
                talents.SelectedTalentIndex = 0;
            }

            return true;
        }

        private TalentStateS2C AddTalentToPlayer(TalentType talentType, PlayerStateS2C playerState)
        {
            ref var newTalent = ref playerState.Spaceship.TalentsState.Talents.AddAndGet();
            var maxCooldown = _gamePlayConfig.Talents.CooldownPerTalentType[talentType];
            newTalent.Setup(talentType, maxCooldown);
            return newTalent;
        }

        private TalentStateS2C ReplaceTalentWithCurrentSelectedTalent(TalentType talentType, PlayerStateS2C playerState)
        {
            ref var currentSelectedTalent = ref playerState.Spaceship.TalentsState.Talents.Get(playerState.Spaceship.TalentsState.SelectedTalentIndex);
            var talentController = _talentControllersPerPlayer[playerState.Id].GetTalentByType(currentSelectedTalent.TalentType);

            if (talentController != null)
            {
                talentController.Stop();
            }
            
            var maxCooldown = _gamePlayConfig.Talents.CooldownPerTalentType[talentType];
            currentSelectedTalent.Setup(talentType, maxCooldown);
            return currentSelectedTalent;
        }
    }
}