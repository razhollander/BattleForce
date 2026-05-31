using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Scripts.Extensions;
using Core.Scripts.Extensions.Linq;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.TeamFloorTracker
{
    public class PlayersOnTeamFloorTrackerService : IPlayersOnTeamFloorTrackerService
    {
        private readonly CapacityDict<ushort, FixedUnorderedList<ushort>> _playerTeamContacts;
        private readonly ConcurrentPool<FixedUnorderedList<ushort>> _contactsPool;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;

        public PlayersOnTeamFloorTrackerService(NetworkConfig networkConfig, SharedGamePlayConfig sharedGamePlayConfig, ISimulationGamePlayConfigService gamePlayConfigService)
        {
            var maxPlayers = networkConfig.MaxCap.ConcurrentPlayers;
            _playerTeamContacts = new CapacityDict<ushort, FixedUnorderedList<ushort>>(maxPlayers);
            _gamePlayConfigService = gamePlayConfigService;
            _contactsPool = new ConcurrentPool<FixedUnorderedList<ushort>>(() => new FixedUnorderedList<ushort>(gamePlayConfigService.GamePlayConfig.MaxOverllapingFloors), maxPlayers);
            _sharedGamePlayConfig = sharedGamePlayConfig;
        }

        public void AddTeamFloorContact(ushort playerId, ushort teamId)
        {
            if (!_playerTeamContacts.TryGetValue(playerId, out var contacts))
            {
                contacts = _contactsPool.Get();
                contacts.Clear();
                _playerTeamContacts.Add(playerId, contacts);
            }

            if (!contacts.IsFull)
            {
                ref var item = ref contacts.AddAndGet();
                item = teamId;
            }
            else
            {
                LogService.LogError($"Contact is full! Player is touching above: {_gamePlayConfigService.GamePlayConfig.MaxOverllapingFloors} floor");
            }
        }

        public void RemoveFloorContact(ushort playerId, ushort teamId)
        {
            if (_playerTeamContacts.TryGetValue(playerId, out var contacts))
            {
                for (int i = 0; i < contacts.Count; i++)
                {
                    if (contacts[i] == teamId)
                    {
                        contacts.RemoveAt(i);
                        return;
                    }
                }
            }
            
            LogService.LogError("Player "+playerId+" tried to remove floor contact for team "+teamId+" but it was not found");
        }

        public ushort GetPlayerTeam(ushort playerId)
        {
            if (_playerTeamContacts.TryGetValue(playerId, out var contacts) && contacts.Count > 0)
            {
                return contacts.GetMostFrequent();
            }
            return _sharedGamePlayConfig.NoTeamId;
        }

        public void RemovePlayer(ushort playerId)
        {
            if (_playerTeamContacts.TryGetValue(playerId, out var contacts))
            {
                contacts.Clear();
                _contactsPool.Return(contacts);
                _playerTeamContacts.Remove(playerId);
            }
        }
    }
}
