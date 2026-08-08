using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Scripts.Utils.CustomCollections;
using Core.Scripts.Network;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts
{
    public class MatchPlayerUIControllers : IMatchPlayerUIControllers
    {
        private readonly MatchPlayerUIControllersView _view;
        private readonly IMatchDataService _matchDataService;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly NetworkConfig _networkConfig;
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;
        private readonly Dictionary<ushort, MatchPlayerUIController> _playerControllers = new Dictionary<ushort, MatchPlayerUIController>();

        public MatchPlayerUIControllers(MatchPlayerUIControllersView view, IMatchDataService matchDataService, PresentationGamePlayConfig gamePlayConfig,
            SharedGamePlayConfig sharedGamePlayConfig, NetworkConfig networkConfig, IStageCancellationTokenProvider stageCancellationTokenProvider)
        {
            _view = view;
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _networkConfig = networkConfig;
            _stageCancellationTokenProvider = stageCancellationTokenProvider;
        }

        public void AddPlayer(ushort playerId, int currentServerTick)
        {
            var newPlayerController = new MatchPlayerUIController(_matchDataService, playerId, _gamePlayConfig, _sharedGamePlayConfig, _networkConfig, _stageCancellationTokenProvider);
            newPlayerController.CreateView(_view.PlayerUIView, _view.PlayersContainer);

            switch (_matchDataService.StageType)
            {
                case StageType.DeathMatch: // the health bar is shown by default
                    break;
                case StageType.WhacAMole: // the health bar slot shows the player's moles-hit score contribution instead
                    newPlayerController.ShowMolesHitScore(_matchDataService.GetPlayer(playerId).MolesHitScore);
                    break;
                default:
                    newPlayerController.HideHealthBar();
                    break;
            }

            var playerTalentsState =_matchDataService.GetPlayer(playerId).Spaceship.TalentsState;
            newPlayerController.UpdateTalents(playerTalentsState.Talents, playerTalentsState.SelectedTalentIndex, currentServerTick);
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

        public void UpdatePlayerMolesHitScore(ushort playerId, int molesHitScore)
        {
            _playerControllers[playerId].UpdateMolesHitScore(molesHitScore);
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
            var selectedTalentIndex = _matchDataService.GetPlayer(playerId).Spaceship.TalentsState.SelectedTalentIndex;
            _playerControllers[playerId].UpdateTalents(talents, selectedTalentIndex, currentServerTick);
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