using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared;
using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel
{
    public interface IMatchDataService 
    {
        List<MatchPlayerModel> Players { get; }
        MatchPlayerModel GetPlayer(int playerId);
        MatchPlayerModel LocalPlayer { get; }
        MatchPlayerModel AddPlayer(int playerId, string playerName, PlayerTransformStateS2C transformState);
        void SetLocalPlayer(MatchPlayerModel matchPlayerModel);
    }
}