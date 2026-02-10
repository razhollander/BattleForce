using Core.Scripts.Network;
using Core.Scripts.Utils;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.TeamFloorTracker
{
    public class PlayersOnTeamFloorTrackerService : IPlayersOnTeamFloorTrackerService
    {
        private readonly CapacityDict<ushort, FixedUnorderedList<ushort>> _playerTeamContacts;
        private readonly ConcurrentPool<FixedUnorderedList<ushort>> _contactsPool;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private const int MAX_OVERLAPPING_FLOORS = 8;

        public PlayersOnTeamFloorTrackerService(NetworkConfig networkConfig, SharedGamePlayConfig sharedGamePlayConfig)
        {
            var maxPlayers = networkConfig.MaxCap.ConcurrentPlayers;
            _playerTeamContacts = new CapacityDict<ushort, FixedUnorderedList<ushort>>(maxPlayers);
            _contactsPool = new ConcurrentPool<FixedUnorderedList<ushort>>(() => new FixedUnorderedList<ushort>(MAX_OVERLAPPING_FLOORS), maxPlayers);
            _sharedGamePlayConfig = sharedGamePlayConfig;
        }

        public void AddFloorContact(ushort playerId, ushort teamId)
        {
            if (!_playerTeamContacts.TryGetValue(playerId, out var contacts))
            {
                contacts = _contactsPool.Get();
                contacts.Clear();
                _playerTeamContacts.Add(playerId, contacts);
            }

            for (int i = 0; i < contacts.Count; i++)
            {
                if (contacts[i] == teamId)
                {
                    return;
                }
            }

            if (!contacts.IsFull)
            {
                ref var item = ref contacts.AddAndGet();
                item = teamId;
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
        }

        public ushort GetPlayerTeam(ushort playerId)
        {
            if (_playerTeamContacts.TryGetValue(playerId, out var contacts) && contacts.Count > 0)
            {
                ushort maxTeam = 0;
                for (int i = 0; i < contacts.Count; i++)
                {
                    if (contacts[i] > maxTeam)
                    {
                        maxTeam = contacts[i];
                    }
                }
                return maxTeam;
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
