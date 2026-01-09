using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Configurations;
using Core.Scripts.Network;
using Core.Scripts.Utils;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Talent
{
    public class PlayersTalentsManager : IPlayersTalentsManager
    {
        private readonly IMatchDataService _matchDataService;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly SimulationGamePlayConfig _gamePlayConfig;
        private readonly Dictionary<int, PlayerTalentControllers> _talentControllersPerPlayer;
        private readonly ConcurrentPool<PlayerTalentControllers> _talentControllersPool;

        public PlayersTalentsManager(NetworkConfig networkConfig, IMatchDataService matchDataService, SharedGamePlayConfig sharedGamePlayConfig, IMatchNetEventsDataService matchNetEventsDataService, SimulationGamePlayConfig gamePlayConfig)
        {
            _matchDataService = matchDataService;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _gamePlayConfig = gamePlayConfig;
            _talentControllersPerPlayer = new Dictionary<int, PlayerTalentControllers>(networkConfig.MaxCap.ConcurrentPlayers);
            _talentControllersPool = new ConcurrentPool<PlayerTalentControllers>(()=> new PlayerTalentControllers(matchNetEventsDataService, matchDataService),networkConfig.MaxCap.ConcurrentPlayers);
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

        public bool TryAddTalentToPlayer(TalentType talentType, ushort playerId)
        {
            var playerState = _matchDataService.SimulationState.GetPlayerById(playerId);
            var didPlayerReachMaxTalents = playerState.Spaceship.Talents.Talents.Count == _sharedGamePlayConfig.MaxConcurrentTalentsForPlayer;

            if (didPlayerReachMaxTalents)
            {
                return TryReplaceTalentWithCurrentSelectedTalent(talentType, playerState);
            }

            AddTalentToPlayer(talentType, playerState);
            return true;
        }

        private void AddTalentToPlayer(TalentType talentType, PlayerStateS2C playerState)
        {
            ref var newTalent = ref playerState.Spaceship.Talents.Talents.AddAndGet();
            var maxCooldown = _gamePlayConfig.Talents.CooldownPerTalentType[talentType];
            newTalent.Setup(talentType, maxCooldown);
        }

        private bool TryReplaceTalentWithCurrentSelectedTalent(TalentType talentType, PlayerStateS2C playerState)
        {
            ref var currentSelectedTalent = ref playerState.Spaceship.Talents.Talents.Get(playerState.Spaceship.Talents.SelectedTalentIndex);
            bool isCurrentSelectedTalentActive = _talentControllersPerPlayer[playerState.Id].IsTalentCurrentlyActive(currentSelectedTalent.TalentType);
            if (isCurrentSelectedTalentActive)
            {
                return false;
            }
            
            var maxCooldown = _gamePlayConfig.Talents.CooldownPerTalentType[talentType];
            currentSelectedTalent.Setup(talentType, maxCooldown);
            return true;
        }
    }
}