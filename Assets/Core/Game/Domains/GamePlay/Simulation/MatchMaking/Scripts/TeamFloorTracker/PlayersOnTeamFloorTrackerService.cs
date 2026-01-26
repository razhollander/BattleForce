using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.TeamFloorTracker
{
    public class PlayersOnTeamFloorTrackerService : IPlayersOnTeamFloorTrackerService
    {
        private readonly CapacityDict<ushort, ushort> _playerTeamMap;

        public PlayersOnTeamFloorTrackerService(NetworkConfig networkConfig)
        {
            _playerTeamMap = new CapacityDict<ushort, ushort>(networkConfig.MaxCap.ConcurrentPlayers);
        }

        public void SetPlayerTeam(ushort playerId, ushort teamId)
        {
            _playerTeamMap[playerId] = teamId;
        }

        public ushort GetPlayerTeam(ushort playerId)
        {
            if (_playerTeamMap.TryGetValue(playerId, out var teamId))
            {
                return teamId;
            }
            return 0;
        }

        public void RemovePlayer(ushort playerId)
        {
            if (_playerTeamMap.ContainsKey(playerId))
            {
                _playerTeamMap.Remove(playerId);
            }
        }
    }
}
