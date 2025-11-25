using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared;
using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel
{
    public interface IMatchDataService 
    {
        MatchPlayerModel[] Players { get; }
        MatchPlayerModel LocalPlayer { get; }
        MatchPlayerModel AddPlayer(string playerName, PlayerTransformStateS2C playerTransformStateS2C);
        void SetLocalPlayer(MatchPlayerModel matchPlayerModel);
    }
}