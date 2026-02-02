using System.Collections.Generic;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts
{
    public interface IStageEndedUiController
    {
        void Show(int winningTeamId, Dictionary<ushort, int> jemsWonPerTeam);
    }
}