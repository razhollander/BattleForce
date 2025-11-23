using Core.Game.Domains.GamePlay.Shared;
using Core.Game.Domains.GamePlay.Shared.NetworkManager;

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

        public MatchPlayerModel AddPlayer(string playerName)
        {
            var newPlayer = new MatchPlayerModel(PlayersCount, playerName);
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