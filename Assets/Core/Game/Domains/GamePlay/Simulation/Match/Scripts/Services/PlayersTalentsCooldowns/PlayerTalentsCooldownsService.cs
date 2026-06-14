using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.OverrideableNetEvents;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Services.PlayersTalentsCooldowns
{
    public class PlayerTalentsCooldownsService : IPlayerTalentsCooldownsService
    {
        private const float NATURIAL_COOLDOWN_MULTIPLIER = 1f;
        
        private readonly NetworkConfig _networkConfig;
        private readonly IMatchDataService _matchDataService;
        private readonly IOverrideableNetEventsService _overrideableNetEventsService;
        private readonly Dictionary<ushort, Dictionary<TalentCooldownMultiplierType, float>> _talentCooldownMultipliersPerPlayerId;

        public PlayerTalentsCooldownsService(NetworkConfig networkConfig, IMatchDataService matchDataService, IOverrideableNetEventsService overrideableNetEventsService)
        {
            _networkConfig = networkConfig;
            _matchDataService = matchDataService;
            _overrideableNetEventsService = overrideableNetEventsService;
            _talentCooldownMultipliersPerPlayerId = new Dictionary<ushort, Dictionary<TalentCooldownMultiplierType, float>>(networkConfig.MaxCap.ConcurrentPlayers);
        }
        
        public void AddPlayer(ushort playerId)
        {
            _talentCooldownMultipliersPerPlayerId.Add(playerId, new Dictionary<TalentCooldownMultiplierType, float>());
        }
        
        public void AddCooldownMultiplierForPlayer(int currentTick, ushort playerId, TalentCooldownMultiplierType talentCooldownMultiplierType, float multiplier, bool shouldSendNetEvent = true)
        {
            var playerTalentsState = _matchDataService.SimulationState.GetPlayerById(playerId).Spaceship.TalentsState;
            var previousPlayerCooldown = playerTalentsState.AllTalentsCooldownMultiplier;
            if (!_talentCooldownMultipliersPerPlayerId[playerId].TryAdd(talentCooldownMultiplierType, multiplier))
            {
                LogService.LogError($"Player already has a cooldown multiplier for this type {talentCooldownMultiplierType}");
                return;
            }
            
            var newCooldown = GetPlayerTotalTalentCooldownMultiplier(playerId);
            var didCooldownChange = previousPlayerCooldown.IsAlmostEqual(newCooldown);

            if (didCooldownChange || !shouldSendNetEvent)
            {
                return;
            }

            SendTalentCooldownChangedNetEvent(playerTalentsState, currentTick, playerId, newCooldown);
        }

        public void RemoveCooldownMultiplierForPlayer(int currentTick, ushort playerId, TalentCooldownMultiplierType talentCooldownMultiplierType, bool shouldSendNetEvent = true)
        {
            var playerTalentsState = _matchDataService.SimulationState.GetPlayerById(playerId).Spaceship.TalentsState;
            var previousPlayerCooldown = playerTalentsState.AllTalentsCooldownMultiplier;
            _talentCooldownMultipliersPerPlayerId[playerId].Remove(talentCooldownMultiplierType);
            var newCooldown = GetPlayerTotalTalentCooldownMultiplier(playerId);
            var didCooldownChange = previousPlayerCooldown.IsAlmostEqual(newCooldown);

            if (didCooldownChange || !shouldSendNetEvent)
            {
                return;
            }

            SendTalentCooldownChangedNetEvent(playerTalentsState, currentTick, playerId, newCooldown);
        }

        private void SendTalentCooldownChangedNetEvent(PlayerTalentsStateS2C playerTalentsState, int currentTick, ushort playerId, float newCooldown)
        {
            playerTalentsState.AllTalentsCooldownMultiplier = newCooldown;

            for (int i = 0; i < playerTalentsState.Talents.Count; i++)
            {
                var talentState = playerTalentsState.Talents[i];
                switch (talentState.CooldownType)
                {
                    case TalentCooldownType.Normal:
                        if (talentState.IsOnCooldown())
                        {
                            var secondsLeftForCooldown = TickUtils.GetSecondsLeftUntilTick(currentTick, talentState.NormalCooldown.CooldownEndTick, _networkConfig.DeltaTime);
                            var newMaxCooldown = talentState.NormalCooldown.MaxCooldown * newCooldown;
                            var shouldReduceCooldownToBeNewMaxCooldown = secondsLeftForCooldown > newMaxCooldown;
                            if (shouldReduceCooldownToBeNewMaxCooldown)
                            {
                                talentState.NormalCooldown.CooldownEndTick = TickUtils.GetTickPassedAfterDuration(currentTick, newMaxCooldown, _networkConfig.DeltaTime);
                            }
                        }
                        break;
                    case TalentCooldownType.Stocks:
                        var secondsLeftForCooldown2 = TickUtils.GetSecondsLeftUntilTick(currentTick, talentState.NormalCooldown.CooldownEndTick, _networkConfig.DeltaTime);
                        var newMaxCooldown2 = talentState.StocksCooldown.MaxSingleStockCooldown * newCooldown;
                        var shouldReduceCooldownToBeNewMaxCooldown2 = secondsLeftForCooldown2 > newMaxCooldown2;
                        if (shouldReduceCooldownToBeNewMaxCooldown2)
                        {
                            talentState.StocksCooldown.MaxSingleStockCooldown = TickUtils.GetTickPassedAfterDuration(currentTick, newMaxCooldown2, _networkConfig.DeltaTime);
                        }
                        break;
                }
                
                playerTalentsState.Talents[i] = talentState;
            }

            _overrideableNetEventsService.OverridePlayerTalentsMaxCooldownMultiplierChangedEvent(currentTick, playerId, newCooldown, playerTalentsState.Talents);

        }

        public float GetPlayerTotalTalentCooldownMultiplier(ushort playerId)
        {
            var sum = NATURIAL_COOLDOWN_MULTIPLIER;
            foreach (var kvp in _talentCooldownMultipliersPerPlayerId[playerId])
            {
                var cooldownMultiplier = kvp.Value;
                sum += cooldownMultiplier;
            }

            return sum;
        }
    }
}