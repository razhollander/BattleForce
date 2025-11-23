using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel
{
    public interface IMatchDataService 
    {
        MatchPlayerModel[] Players { get; }
        MatchPlayerModel LocalPlayer { get; }
        MatchPlayerModel AddPlayer(string playerName);
        void SetLocalPlayer(MatchPlayerModel matchPlayerModel);
    }
}