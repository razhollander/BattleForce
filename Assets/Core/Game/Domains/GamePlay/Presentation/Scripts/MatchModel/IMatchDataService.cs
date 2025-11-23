using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel
{
    public interface IMatchDataService 
    {
        List<MatchPlayerModel> Players { get; }
        MatchPlayerModel LocalPlayer { get; }
        MatchPlayerModel AddPlayer(int playerId, string playerName);
        void SetLocalPlayer(MatchPlayerModel matchPlayerModel);
    }
}