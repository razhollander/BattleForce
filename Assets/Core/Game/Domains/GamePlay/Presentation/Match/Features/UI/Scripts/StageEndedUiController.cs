using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts
{
    public class StageEndedUiController
    {
        private readonly StageEndedUiView _viewPrefab;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private StageEndedUiView _view;

        public StageEndedUiController(StageEndedUiView viewPrefab, PresentationGamePlayConfig gamePlayConfig)
        {
            _viewPrefab = viewPrefab;
            _gamePlayConfig = gamePlayConfig;
        }

        public void Show(StageEndNetEventS2C evt)
        {
            if (_view == null)
            {
                _view = Object.Instantiate(_viewPrefab);
            }

            Color teamColor = Color.white;
            if (_gamePlayConfig.ColorPerTeamId.Count > evt.WinningTeamId)
            {
                teamColor = _gamePlayConfig.ColorPerTeamId[evt.WinningTeamId];
            }

            _view.Show(evt.WinningTeamId, teamColor, evt.TotalJemsPerTeam);
        }
    }
}
