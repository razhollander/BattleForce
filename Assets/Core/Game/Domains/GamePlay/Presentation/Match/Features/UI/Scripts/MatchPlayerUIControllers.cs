using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;

namespace Core.Game.Domains.GamePlay.Presentation.Features.UI.Match.Scripts
{
    public class MatchPlayerUIControllers : IMatchPlayerUIControllers
    {
        private readonly MatchPlayerUIControllersView _view;
        private readonly IMatchDataService _matchDataService;
        private readonly Dictionary<ushort, MatchPlayerUIController> _playerControllers = new Dictionary<ushort, MatchPlayerUIController>();

        public MatchPlayerUIControllers(MatchPlayerUIControllersView view, IMatchDataService matchDataService)
        {
            _view = view;
            _matchDataService = matchDataService;
        }

        public void AddPlayer(ushort playerId)
        {
            var newPlayerController = new MatchPlayerUIController(_matchDataService, playerId);
            newPlayerController.CreateView(_view.PlayerUIView, _view.PlayersContainer);
            _playerControllers.Add(playerId, newPlayerController);
        }

        public void SetPlayerHealth(ushort playerId, ushort currentHealth, ushort maxHealth)
        {
            _playerControllers[playerId].SetHealth(currentHealth, maxHealth);
        }
    }
}