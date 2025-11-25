using Core.Game.Domains.GamePlay.Shared;
using Core.Game.Domains.GamePlay.Shared.NetworkManager;
using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;
using Core.Scripts.Network;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel
{
    public class MatchDataService : IMatchDataService
    {
        public MatchPlayerModel[] Players { get; private set; }
        public int PlayersCount { get; private set; }
        public MatchPlayerModel LocalPlayer { get; private set; }

        public MatchDataService(NetworkConfig networkConfig)
        {
            Players = new MatchPlayerModel[networkConfig.MaxConnectedPlayers];
        }

        public MatchPlayerModel AddPlayer(string playerName, PlayerTransformStateS2C playerTransformStateS2C)
        {
            var newPlayer = new MatchPlayerModel(PlayersCount, playerName, playerTransformStateS2C);
            Players[PlayersCount] = newPlayer;
            PlayersCount++;
            return newPlayer;
        }

        public void SetLocalPlayer(MatchPlayerModel matchPlayerModel)
        {
            LocalPlayer = matchPlayerModel;
        }
    }
}