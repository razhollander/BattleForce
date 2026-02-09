using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts
{
    public class MatchPlayerUIController
    {
        private readonly IMatchDataService _matchDataService;
        private readonly ushort _playerId;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private MatchPlayerUIView _view;

        public MatchPlayerUIController(IMatchDataService matchDataService, ushort playerId, PresentationGamePlayConfig gamePlayConfig)
        {
            _matchDataService = matchDataService;
            _playerId = playerId;
            _gamePlayConfig = gamePlayConfig;
        }

        public void CreateView(MatchPlayerUIView viewPrefab, Transform parent)
        {
            _view = Object.Instantiate(viewPrefab, parent);
            var playerModel = _matchDataService.GetPlayer(_playerId);
            _view.Setup(playerModel.PlayerName, _gamePlayConfig.ColorPerTeamId[playerModel.TeamId]);
        }

        public void SetHealth(ushort currentHealth, ushort maxHealth)
        {
            _view.SetHealth(currentHealth, maxHealth);
        }

        public void HideHealthBar()
        {
            _view.HideHealthBar();
        }

        public void SwitchToPlayerDeadState()
        {
            _view.SetOpacity(0.5f);
        }

        public void Destroy()
        {
            Object.Destroy(_view.gameObject);
        }
    }
}