using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.UI.Match.Scripts
{
    public class MatchPlayerUIController
    {
        private readonly IMatchDataService _matchDataService;
        private readonly ushort _playerId;
        private MatchPlayerUIView _view;

        public MatchPlayerUIController(IMatchDataService matchDataService, ushort playerId)
        {
            _matchDataService = matchDataService;
            _playerId = playerId;
        }

        public void CreateView(MatchPlayerUIView viewPrefab, Transform parent)
        {
            _view = Object.Instantiate(viewPrefab, parent);
            var playerModel = _matchDataService.GetPlayer(_playerId);
            _view.Setup(playerModel.PlayerName, playerModel.Spaceship.Color);
        }

        public void SetHealth(ushort currentHealth, ushort maxHealth)
        {
            _view.SetHealth(currentHealth, maxHealth);
        }
    }
}