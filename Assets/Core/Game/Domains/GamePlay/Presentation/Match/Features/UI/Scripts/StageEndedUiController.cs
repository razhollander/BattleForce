using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts
{
    public class StageEndedUiController : IStageEndedUiController
    {
        private readonly StageEndedUiView _view;
        private readonly PresentationGamePlayConfig _gamePlayConfig;

        public StageEndedUiController(StageEndedUiView view, PresentationGamePlayConfig gamePlayConfig)
        {
            _view = view;
            _gamePlayConfig = gamePlayConfig;
        }
        
        public void Show(int winningTeamId, Dictionary<ushort, int> jemsWonPerTeam)
        {
            var teamColor = _gamePlayConfig.ColorPerTeamId[winningTeamId];
            _view.Show(winningTeamId, teamColor, jemsWonPerTeam);
        }
        
        public void Hide()
        {
            _view.Hide();
        }
    }
}
