using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts
{
    public class MatchPlayerUIControllers : IMatchPlayerUIControllers
    {
        private readonly MatchPlayerUIControllersView _view;
        private readonly IMatchDataService _matchDataService;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly Dictionary<ushort, MatchPlayerUIController> _playerControllers = new Dictionary<ushort, MatchPlayerUIController>();

        public MatchPlayerUIControllers(MatchPlayerUIControllersView view, IMatchDataService matchDataService, SharedGamePlayConfig sharedGamePlayConfig)
        {
            _view = view;
            _matchDataService = matchDataService;
            _sharedGamePlayConfig = sharedGamePlayConfig;
        }

        public void AddPlayer(ushort playerId)
        {
            var newPlayerController = new MatchPlayerUIController(_matchDataService, playerId, _sharedGamePlayConfig);
            newPlayerController.CreateView(_view.PlayerUIView, _view.PlayersContainer);
            _playerControllers.Add(playerId, newPlayerController);
        }

        public void SetPlayerHealth(ushort playerId, ushort currentHealth, ushort maxHealth)
        {
            _playerControllers[playerId].SetHealth(currentHealth, maxHealth);
        }
    }
}