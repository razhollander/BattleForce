using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared;
using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;
using Core.Scripts.Network;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel
{
    public class MatchDataService : IMatchDataService
    {
        public List<MatchPlayerModel> Players { get; private set; }

        public MatchPlayerModel LocalPlayer { get; private set; }

        public MatchDataService(NetworkConfig networkConfig)
        {
            Players = new List<MatchPlayerModel>(networkConfig.MaxConnectedPlayers);
        }

        public MatchPlayerModel GetPlayer(int playerId)
        {
            return Players.Find(x => x.PlayerId == playerId);
        }

        public MatchPlayerModel AddPlayer(int playerId, string playerName, PlayerTransformStateS2C transformState)
        {
            var newPlayer = new MatchPlayerModel(playerId, playerName, transformState);
            Players.Add(newPlayer);
            return newPlayer;
        }

        public void SetLocalPlayer(MatchPlayerModel matchPlayerModel)
        {
            LocalPlayer = matchPlayerModel;
        }
    }
}