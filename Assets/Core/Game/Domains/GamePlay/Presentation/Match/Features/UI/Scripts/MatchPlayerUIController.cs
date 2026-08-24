using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Utils.CustomCollections;
using UnityEngine;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts
{
    public class MatchPlayerUIController
    {
        private readonly IMatchDataService _matchDataService;
        private readonly ushort _playerId;
        private readonly PresentationGamePlayConfig _gamePlayConfig;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly NetworkConfig _networkConfig;
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;
        private MatchPlayerUIView _view;

        public MatchPlayerUIController(IMatchDataService matchDataService, ushort playerId, PresentationGamePlayConfig gamePlayConfig, SharedGamePlayConfig sharedGamePlayConfig,
            NetworkConfig networkConfig, IStageCancellationTokenProvider stageCancellationTokenProvider)
        {
            _matchDataService = matchDataService;
            _playerId = playerId;
            _gamePlayConfig = gamePlayConfig;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _networkConfig = networkConfig;
            _stageCancellationTokenProvider = stageCancellationTokenProvider;
        }

        public void CreateView(MatchPlayerUIView viewPrefab, Transform parent)
        {
            _view = Object.Instantiate(viewPrefab, parent);
            var playerModel = _matchDataService.GetPlayer(_playerId);
            _view.Setup(playerModel.PlayerName, _gamePlayConfig.ColorPerTeamId[playerModel.TeamId], _sharedGamePlayConfig.MaxConcurrentTalentsForPlayer);
        }

        public void SetHealth(ushort currentHealth, ushort maxHealth)
        {
            _view.SetHealth(currentHealth, maxHealth, _stageCancellationTokenProvider.CancellationTokenSource.Token);
        }

        public void HideHealthBar()
        {
            _view.HideHealthBar();
        }

        public void ShowMolesKilledScore(int molesKilledScore)
        {
            _view.ShowMolesKilledScore();
            UpdateMolesKilledScore(molesKilledScore);
        }

        public void UpdateMolesKilledScore(int molesKilledScore)
        {
            _view.UpdateMolesKilledScore(molesKilledScore);
        }

        public void ShowGatePassScore(int gatePassScore)
        {
            _view.ShowGatePassScore();
            UpdateGatePassScore(gatePassScore);
        }

        public void UpdateGatePassScore(int gatePassScore)
        {
            _view.UpdateGatePassScore(gatePassScore);
        }

        public void SwitchToPlayerDeadState()
        {
            _view.SetOpacity(0.5f);
        }

        public void Destroy()
        {
            Object.Destroy(_view.gameObject);
        }

        public void UpdateTalents(FixedOrderedList<TalentStateS2C> talents, int selectedTalentIndex, int currentServerTick)
        {
            _view.UpdateTalents(ConvertTalentsToVisualData(talents, currentServerTick));
            _view.SetSelectedTalent(selectedTalentIndex);
        }
        
        public void UpdateTalentsCooldown(FixedOrderedList<TalentStateS2C> talents, int currentServerTick)
        {
            for (int i = 0; i < talents.Count; i++)
            {
                var talentState = talents[i];

                switch (talentState.CooldownType)
                {
                    case TalentCooldownType.Normal: 
                        UpdateTalentViewNormalCooldown(talentState, i, currentServerTick);
                        break;
                    case TalentCooldownType.Stocks: 
                        UpdateTalentViewStocksCooldown(talentState, i, currentServerTick);
                        break;
                    case TalentCooldownType.AlwaysActive:
                        break;
                    default: LogService.LogError("Not implemented cooldown type: " + talentState.CooldownType);
                        break;
                }
            }
        }
        
        private void UpdateTalentViewStocksCooldown(TalentStateS2C talentState, int talentViewIndex, int currentServerTick)
        {
            var maxCooldown = talentState.StocksCooldown.MaxSingleStockCooldown;
            var isOnCooldown = talentState.StocksCooldown.IsOnCooldown();
            var cooldownLeft = talentState.StocksCooldown.IsAtMaxStocks() ? 0 : TickUtils.GetSecondsLeftUntilTick(currentServerTick, talentState.StocksCooldown.RecieveNextStockOnTick, _networkConfig.DeltaTime);
            _view.UpdateTalentCooldown(talentViewIndex, maxCooldown, cooldownLeft, isOnCooldown);
            _view.UpdateTalentStocks(talentViewIndex, talentState.StocksCooldown.CurrentStocksAmount);
        }

        private void UpdateTalentViewNormalCooldown(TalentStateS2C talentState, int talentViewIndex, int currentServerTick)
        {
            var maxCooldown = talentState.NormalCooldown.MaxCooldown;
            var isOnCooldown = talentState.NormalCooldown.IsOnCooldown();
            var cooldownLeft = isOnCooldown ? TickUtils.GetSecondsLeftUntilTick(currentServerTick, talentState.NormalCooldown.CooldownEndTick, _networkConfig.DeltaTime) : 0;
            _view.UpdateTalentCooldown(talentViewIndex, maxCooldown, cooldownLeft, isOnCooldown);
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

                switch (talentState.CooldownType)
                {
                    case TalentCooldownType.Normal:
                        var isOnCooldown = talentState.IsOnCooldown();
                        talentVisualData.IsOnCooldown = isOnCooldown;
                        talentVisualData.IsStockable = false;
                        talentVisualData.CooldownLeft = isOnCooldown ? TickUtils.GetSecondsLeftUntilTick(currentServerTick, talentState.NormalCooldown.CooldownEndTick, _networkConfig.DeltaTime) : 0;
                        break;
                    case TalentCooldownType.Stocks:
                        var maxCooldown2 = talentState.StocksCooldown.MaxSingleStockCooldown;
                        var isOnCooldown2 = talentState.StocksCooldown.IsOnCooldown();
                        var cooldownLeft2 = talentState.StocksCooldown.IsAtMaxStocks() ? 0 : TickUtils.GetSecondsLeftUntilTick(currentServerTick, talentState.StocksCooldown.RecieveNextStockOnTick, _networkConfig.DeltaTime);
                        talentVisualData.IsStockable = true;
                        _view.UpdateTalentCooldown(i, maxCooldown2, cooldownLeft2, isOnCooldown2);
                        break;
                    case TalentCooldownType.AlwaysActive:
                        _view.UpdateTalentCooldown(i, 1, 1, true);
                        break;
                    default: LogService.LogError("Not implemented cooldown type: " + talentState.CooldownType);
                        break;
                }
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
        
        public bool IsStockable;
        public int StocksAmount;
    }
}