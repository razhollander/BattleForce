using System.Numerics;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Inputs
{
    public class PlayersMouseDataService : IPlayersMouseDataService
    {
        private readonly CapacityDict<ushort, PlayerMouseData> _mouseDataPerPlayer;

        public PlayersMouseDataService(NetworkConfig networkConfig)
        {
            _mouseDataPerPlayer = new CapacityDict<ushort, PlayerMouseData>(networkConfig.MaxCap.ConcurrentPlayers);
        }

        public void SetPlayerMouseData(ushort playerId, bool isUsingMouseAim, Vector2 mouseWorldPosition)
        {
            _mouseDataPerPlayer[playerId] = new PlayerMouseData
            {
                IsUsingMouseAim = isUsingMouseAim,
                MouseWorldPosition = mouseWorldPosition,
            };
        }

        public PlayerMouseData GetPlayerMouseData(ushort playerId)
        {
            return _mouseDataPerPlayer.TryGetValue(playerId, out var data) ? data : default;
        }
    }
}
