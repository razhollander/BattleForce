using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Utils.CustomCollections;
using UnityEngine;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Timer;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Scripts.Network;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts
{
    public class MatchPlayerUIController
    {
        private readonly IMatchDataService _matchDataService;
        private readonly ushort _playerId;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly NetworkConfig _networkConfig;
        private MatchPlayerUIView _view;

        public MatchPlayerUIController(IMatchDataService matchDataService, ushort playerId, PresentationGamePlayConfig gamePlayConfig, SharedGamePlayConfig sharedGamePlayConfig, NetworkConfig networkConfig)
        {
            _matchDataService = matchDataService;
            _playerId = playerId;
            _gamePlayConfig = gamePlayConfig;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _networkConfig = networkConfig;
        }

        public void CreateView(MatchPlayerUIView viewPrefab, Transform parent)
        {
            _view = Object.Instantiate(viewPrefab, parent);
            var playerModel = _matchDataService.GetPlayer(_playerId);
            _view.Setup(playerModel.PlayerName, _gamePlayConfig.ColorPerTeamId[playerModel.TeamId], _sharedGamePlayConfig.MaxConcurrentTalentsForPlayer);
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

        public void UpdateTalents(FixedOrderedList<TalentStateS2C> talents, int currentServerTick)
        {
            _view.UpdateTalents(ConvertTalentsToVisualData(talents, currentServerTick));
        }
        
        public void UpdateTalentsCooldown(FixedOrderedList<TalentStateS2C> talents, int currentServerTick)
        {
            for (int i = 0; i < talents.Count; i++)
            {
                var maxCooldown = talents[i].MaxCooldown;
                var isOnCooldown = talents[i].IsOnCooldown();
                var cooldownLeft = isOnCooldown ? TickUtils.GetSecondsLeftUntilTick(currentServerTick, talents[i].CooldownEndTick, _networkConfig.DeltaTime) : maxCooldown;
                _view.UpdateTalentCooldown(i, maxCooldown, cooldownLeft, isOnCooldown);
            }
        }
        
        public void SetSelectedTalent(int talentIndex)
        {
            _view.SetSelectedTalent(talentIndex);
        }

        private TalentVisualData[] ConvertTalentsToVisualData(FixedOrderedList<TalentStateS2C> talents, int currentServerTick)
        {
            var talentsVisualData = new TalentVisualData[talents.Count];

            for (int i = 0; i < talentsVisualData.Length; i++)
            {
                var talentVisualData = new TalentVisualData();
                var talentState = talents[i];
                talentVisualData.Icon = _gamePlayConfig.TalentCards.TalentSprites[talentState.TalentType];
                 var isOnCooldown = talentState.IsOnCooldown();
                talentVisualData.IsOnCooldown = isOnCooldown;
                var maxCooldown = talentState.MaxCooldown;
                talentVisualData.CooldownLeft = isOnCooldown ? TickUtils.GetSecondsLeftUntilTick(currentServerTick, talentState.CooldownEndTick, _networkConfig.DeltaTime) : maxCooldown;
                talentVisualData.MaxCooldown = maxCooldown;
                talentsVisualData[i] = talentVisualData;
            }

            return talentsVisualData;
        }
    }

    public class TalentVisualData
    {
        public Sprite Icon;
        public float MaxCooldown;
        public float CooldownLeft;
        public bool IsOnCooldown;
    }
}