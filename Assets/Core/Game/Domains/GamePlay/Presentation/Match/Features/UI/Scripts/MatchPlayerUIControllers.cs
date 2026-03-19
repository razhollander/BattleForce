using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Utils.CustomCollections;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Timer;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts
{
    public class MatchPlayerUIControllers : IMatchPlayerUIControllers
    {
        private readonly MatchPlayerUIControllersView _view;
        private readonly IMatchDataService _matchDataService;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly IMatchPlayerTimersService _matchPlayerTimersService;
        private readonly Dictionary<ushort, MatchPlayerUIController> _playerControllers = new Dictionary<ushort, MatchPlayerUIController>();

        public MatchPlayerUIControllers(MatchPlayerUIControllersView view, IMatchDataService matchDataService, PresentationGamePlayConfig gamePlayConfig,
            SharedGamePlayConfig sharedGamePlayConfig, IMatchPlayerTimersService matchPlayerTimersService)
        {
            _view = view;
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _matchPlayerTimersService = matchPlayerTimersService;
        }

        public void AddPlayer(ushort playerId, int currentServerTick)
        {
            var newPlayerController = new MatchPlayerUIController(_matchDataService, playerId, _gamePlayConfig, _sharedGamePlayConfig, _matchPlayerTimersService);
            newPlayerController.CreateView(_view.PlayerUIView, _view.PlayersContainer);
            newPlayerController.UpdateTalents(_matchDataService.GetPlayer(playerId).Spaceship.TalentsState.Talents, currentServerTick);
            _playerControllers.Add(playerId, newPlayerController);
        }

        public void SetPlayerHealth(ushort playerId, ushort currentHealth, ushort maxHealth)
        {
            _playerControllers[playerId].SetHealth(currentHealth, maxHealth);
        }

        public void HidePlayerHealthBar(ushort playerId)
        {
            _playerControllers[playerId].HideHealthBar();
        }

        public void SwitchToPlayerDeadState(ushort playerId)
        {
            _playerControllers[playerId].SwitchToPlayerDeadState();
        }

        public void DestroyAll()
        {
            foreach (var controller in _playerControllers.Values)
            {
                controller.Destroy();
            }
            _playerControllers.Clear();
        }

        public void UpdatePlayerTalents(ushort playerId, FixedOrderedList<TalentStateS2C> talents, int currentServerTick)
        {
            _playerControllers[playerId].UpdateTalents(talents, currentServerTick);
            var selectedTalentIndex = _matchDataService.GetPlayer(playerId).Spaceship.TalentsState.SelectedTalentIndex;
            SetPlayerSelectedTalent(playerId, selectedTalentIndex);
        }

        public void UpdatePlayersTalentCooldowns(int currentServerTick)
        {
            foreach (var playerController in _playerControllers)
            {
                playerController.Value.UpdateTalentsCooldown(_matchDataService.GetPlayer(playerController.Key).Spaceship.TalentsState.Talents, currentServerTick);
            }
        }

        public void SetPlayerSelectedTalent(ushort playerId, int index)
        {
            _playerControllers[playerId].SetSelectedTalent(index);
        }
    }
}