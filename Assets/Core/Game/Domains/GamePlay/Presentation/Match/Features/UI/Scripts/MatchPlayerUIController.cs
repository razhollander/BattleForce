using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts
{
    public class MatchPlayerUIController
    {
        private readonly IMatchDataService _matchDataService;
        private readonly ushort _playerId;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private MatchPlayerUIView _view;

        public MatchPlayerUIController(IMatchDataService matchDataService, ushort playerId, SharedGamePlayConfig sharedGamePlayConfig)
        {
            _matchDataService = matchDataService;
            _playerId = playerId;
            _sharedGamePlayConfig = sharedGamePlayConfig;
        }

        public void CreateView(MatchPlayerUIView viewPrefab, Transform parent)
        {
            _view = Object.Instantiate(viewPrefab, parent);
            var playerModel = _matchDataService.GetPlayer(_playerId);
            _view.Setup(playerModel.PlayerName, _sharedGamePlayConfig.ColorPerTeamId[playerModel.TeamId]);
        }

        public void SetHealth(ushort currentHealth, ushort maxHealth)
        {
            _view.SetHealth(currentHealth, maxHealth);
        }
    }
}