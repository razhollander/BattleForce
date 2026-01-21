using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.UI.Match.Scripts
{
    public class MatchPlayersUIController
    {
        private readonly MatchPlayersUIView _viewPrefab;
        private readonly IMatchDataService _matchDataService;
        private readonly Dictionary<ushort, MatchPlayersUIView> _playerViews = new Dictionary<ushort, MatchPlayersUIView>();
        private Transform _container;

        public MatchPlayersUIController(MatchPlayersUIView viewPrefab, IMatchDataService matchDataService)
        {
            _viewPrefab = viewPrefab;
            _matchDataService = matchDataService;
        }

        public void SetContainer(Transform container)
        {
            _container = container;
        }

        public void UpdateUI()
        {
            if (_container == null) return;

            // Check for new players and update existing ones
            foreach (var player in _matchDataService.Players)
            {
                if (!_playerViews.ContainsKey(player.PlayerId))
                {
                    var view = Object.Instantiate(_viewPrefab, _container);
                    view.Setup(player.PlayerName, player.Spaceship.Color);
                    _playerViews.Add(player.PlayerId, view);
                }

                var ui = _playerViews[player.PlayerId];

                // Update stats
                ui.UpdateHealth(player.Spaceship.Health.CurrentHealth, player.Spaceship.Health.MaxHealth);
                ui.UpdateTalents(player.Spaceship.Talents);

                // Update money (Placeholder as it's not in the model yet)
                ui.UpdateMoney(0);
            }
        }
    }
}