using System;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Extensions;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Network.PacketsHandlers
{
    public class PresentationMatchNetEventsHandler
    {
        private readonly IMatchDataService _matchDataService;
        private readonly ICachedPresentationEventsService _cachedPresentationEventsService;
        private readonly ICommandFactory _commandFactory;
        private readonly AddMatchPlayerCommand _addMatchPlayerCommand;
        
        public PresentationMatchNetEventsHandler(IMatchDataService matchDataService,
            ICachedPresentationEventsService cachedPresentationEventsService, ICommandFactory commandFactory)
        {
            _matchDataService = matchDataService;
            _cachedPresentationEventsService = cachedPresentationEventsService;
            _commandFactory = commandFactory;
            _addMatchPlayerCommand = _commandFactory.CreateCommandVoid<AddMatchPlayerCommand>();
        }

        public void ProcessPlayerRejoinedEvents(CapacityList<PlayerRejoinAcceptPacketS2C> playerRejoinAcceptNetEvents, int currentServerTick)
        {
            foreach (var playerRejoinAcceptNetEvent in playerRejoinAcceptNetEvents)
            {
                var playerId = playerRejoinAcceptNetEvent.PlayerState.Id;
                var isLocalPlayer = playerRejoinAcceptNetEvent.IsLocal;
                LogService.LogTopic(
                    $"Join packet accepted processed,  isLocalPlayer:{isLocalPlayer}, player id: " + playerId,
                    LogTopicType.ClientNetwork);

                if (!isLocalPlayer)
                {
                    _addMatchPlayerCommand.SetPlayerState(playerRejoinAcceptNetEvent.PlayerState).SetCurrentServerTick(currentServerTick).Execute();
                }
            }
        }
        
        public void ProcessBulletSpawnEvents(CapacityList<BulletSpawnNetEventS2C> bulletSpawnNetEvents)
        {
            if (bulletSpawnNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var bulletSpawnNetEvent in bulletSpawnNetEvents)
            {
                _matchDataService.AddBullet(bulletSpawnNetEvent.BulletId, bulletSpawnNetEvent.BelongToPlayerId,
                    bulletSpawnNetEvent.Position, bulletSpawnNetEvent.BulletRadius);
                _cachedPresentationEventsService.BulletSpawnNetEvents.Add(bulletSpawnNetEvent);
            }
        }

        public void ProcessPlayerTakeDamageEvents(CapacityList<PlayerTakeDamageNetEventS2C> playerTakeDamageEvents)
        {
            if (playerTakeDamageEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var playerTakeDamageEvent in playerTakeDamageEvents)
            {
                var playerModel = _matchDataService.GetPlayer(playerTakeDamageEvent.PlayerId);

                if (playerModel == null)
                {
                    LogService.LogError($"Null for id {playerTakeDamageEvent.PlayerId}!");
                }
                playerModel.Spaceship.Health.CurrentHealth = Math.Min(playerModel.Spaceship.Health.CurrentHealth, playerTakeDamageEvent.PlayerHealth);// we do Min because the player may get hit multiple times the same frame
                LogService.LogTopic($"Player lose {playerTakeDamageEvent.HitDamage} health, and now has {playerModel.Spaceship.Health.CurrentHealth}");
                _cachedPresentationEventsService.PlayerTakeDamageNetEvents.Add(playerTakeDamageEvent);
            }
        }

        public void ProcessPlayerDiedEvents(CapacityList<PlayerDiedNetEventS2C> playerDiedEvents)
        {
            if (playerDiedEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var playerDiedEvent in playerDiedEvents)
            {
                _cachedPresentationEventsService.PlayerDiedNetEvents.Add(playerDiedEvent);
            }
        }

        public void ProcessBulletDestroyedEvents(CapacityList<BulletDestroyedNetEventS2C> bulletDestroyedEvents)
        {
            if (bulletDestroyedEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var bulletDestroyedEvent in bulletDestroyedEvents)
            {
                _matchDataService.RemoveBullet(bulletDestroyedEvent.BulletId);
                _cachedPresentationEventsService.BulletDestroyedNetEvents.Add(bulletDestroyedEvent);
            }
        }

        public void ProcessPlayerSwapEvents(CapacityList<PlayersSwapNetEventS2C> playerSwapEvents)
        {
            if (playerSwapEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var playerSwapEvent in playerSwapEvents)
            {
                var casterPlayer = _matchDataService.GetPlayer(playerSwapEvent.CasterPlayerId);
                casterPlayer.Spaceship.Transform.Position = playerSwapEvent.CasterPosition;
                casterPlayer.Spaceship.Transform.Direction = playerSwapEvent.CasterDirection;
                
                var otherPlayer = _matchDataService.GetPlayer(playerSwapEvent.OtherPlayerId);
                otherPlayer.Spaceship.Transform.Position = playerSwapEvent.OtherPosition;
                otherPlayer.Spaceship.Transform.Direction = playerSwapEvent.OtherDirection;

                _cachedPresentationEventsService.PlayerSwapNetEvents.Add(playerSwapEvent);
            }
        }

        public void ProcessTalentCardObtainedEvents(CapacityList<TalentCardObtainedNetEventS2C> talentCardObtainedNetEvents)
        {
            if (talentCardObtainedNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var talentCardObtainedNetEvent in talentCardObtainedNetEvents)
            {
                var cardId = talentCardObtainedNetEvent.TalentCardId;
                _matchDataService.RemoveTalentCard(cardId);
                UpdatePlayerTalentsFromCardObtainedEvent(talentCardObtainedNetEvent);
                _cachedPresentationEventsService.TalentCardObtainedNetEvents.Add(talentCardObtainedNetEvent);
            }
        }

        private void UpdatePlayerTalentsFromCardObtainedEvent(TalentCardObtainedNetEventS2C talentCardObtainedNetEvent)
        {
            var playerTalents = _matchDataService.GetPlayer(talentCardObtainedNetEvent.ObtainedByPlayerId).Spaceship.TalentsState.Talents;
            playerTalents.Clear();
            foreach (var newPlayerTalent in talentCardObtainedNetEvent.PlayerTalents.AsSpan())
            {
                ref var playerTalent = ref playerTalents.AddAndGet();
                playerTalent = newPlayerTalent;
            }
        }

        public void ProcessTalentCardHitEvents(CapacityList<TalentCardHitNetEventS2C> talentCardHitNetEvents)
        {
            if (talentCardHitNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var talentCardHitNetEvent in talentCardHitNetEvents)
            {
                _cachedPresentationEventsService.TalentCardHitNetEvents.Add(talentCardHitNetEvent);
                _matchDataService.GetTalentCard(talentCardHitNetEvent.TalentCardId).Health = talentCardHitNetEvent.TalentCardHealth;
            }
        }

        public void ProcessPowerUpSpawnedEvents(CapacityList<PowerUpBallSpawnedNetEventS2C> powerUpBallSpawnedNetEvents)
        {
            if (powerUpBallSpawnedNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var powerUpBallSpawnedNetEvent in powerUpBallSpawnedNetEvents)
            {
                _matchDataService.AddPowerUpBall(powerUpBallSpawnedNetEvent.PowerUpBallId, powerUpBallSpawnedNetEvent.Position.ToUnityVector2());
                _cachedPresentationEventsService.PowerUpBallSpawnedNetEvents.Add(powerUpBallSpawnedNetEvent);
            }
        }

        public void ProcessPowerUpObtainedEvents(CapacityList<PowerUpBallObtainedNetEventS2C> powerUpBallObtainedEvents)
        {
            if (powerUpBallObtainedEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var powerUpBallObtainedNetEvent in powerUpBallObtainedEvents)
            {
                var powerUpBallId = powerUpBallObtainedNetEvent.Id;
                _matchDataService.RemovePowerUpBall(powerUpBallId);
                _cachedPresentationEventsService.PowerUpBallObtainedNetEvents.Add(powerUpBallObtainedNetEvent);
            }
        }

        public void ProcessStageEndEvents(CapacityList<StageEndNetEventS2C> stageEndNetEvents)
        {
            if (stageEndNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var stageEndNetEvent in stageEndNetEvents)
            {
                _cachedPresentationEventsService.StageEndNetEvents.Add(stageEndNetEvent);
            }
        }

        public void ProcessTeamLostEvents(CapacityList<TeamLostNetEventS2C> teamLostNetEvents)
        {
            if (teamLostNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var teamLostNetEvent in teamLostNetEvents)
            {
                foreach (var gemsPerTeam in teamLostNetEvent.TotalGemsPerTeam)
                {
                    _matchDataService.SetTeamGems(gemsPerTeam.Key, gemsPerTeam.Value);
                }
                _cachedPresentationEventsService.TeamLostNetEvents.Add(teamLostNetEvent);
            }
        }

        public void ProcessTalentSwitchEvents(CapacityList<TalentSwitchNetEventS2C> talentSwitchNetEvents)
        {
            if (talentSwitchNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var talentSwitchNetEvent in talentSwitchNetEvents)
            {
                _matchDataService.GetPlayer(talentSwitchNetEvent.PlayerId).Spaceship.TalentsState.SelectedTalentIndex = talentSwitchNetEvent.NewTalentIndex;
                _cachedPresentationEventsService.TalentSwitchNetEvents.Add(talentSwitchNetEvent);
            }
        }

        public void ProcessEnvironmentSpringPlayerCollisionEvents(CapacityList<EnvironmentSpringPlayerCollisionNetEventS2C> environmentSpringPlayerCollisionNetEvents)
        {
            if (environmentSpringPlayerCollisionNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var environmentSpringPlayerCollisionNetEvent in environmentSpringPlayerCollisionNetEvents)
            {
                _cachedPresentationEventsService.EnvironmentSpringPlayerCollisionNetEvents.Add(environmentSpringPlayerCollisionNetEvent);
            }
        }

        public void ProcessGainBoltsNetEvents(CapacityList<GainBoltsNetEventS2C> gainBoltsNetEvents)
        {
            if (gainBoltsNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var gainBoltsNetEvent in gainBoltsNetEvents)
            {
                var playerTeamId = _matchDataService.GetPlayerTeamId(gainBoltsNetEvent.PlayerId);
                _matchDataService.SetTeamBolts(playerTeamId, gainBoltsNetEvent.TotalTeamBolts);
                _cachedPresentationEventsService.GainBoltsNetEvents.Add(gainBoltsNetEvent);
            }
        }

        public void ProcessPlayerToEnvironmentTeleportCollisionEvents(CapacityList<PlayerToEnvironmentTeleportGateCollisionNetEventS2C> playerToEnvironmentTeleportCollisionEvents)
        {
            if (playerToEnvironmentTeleportCollisionEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var playerToEnvironmentTeleportCollisionEvent in playerToEnvironmentTeleportCollisionEvents)
            {
                _matchDataService.GetPlayer(playerToEnvironmentTeleportCollisionEvent.PlayerId).Spaceship.Transform.Position = playerToEnvironmentTeleportCollisionEvent.ExitPoint;
                _cachedPresentationEventsService.PlayerToEnvironmentTeleportGateCollisionNetEvents.Add(playerToEnvironmentTeleportCollisionEvent);
            }
        }

        public void ProcessPreparationPhaseEndedEvents(CapacityList<PreparationPhaseEndedNetEventS2C> preparationPhaseEndedNetEvents)
        {
            if (preparationPhaseEndedNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var preparationPhaseEndedNetEvent in preparationPhaseEndedNetEvents)
            {
                _matchDataService.StartPhaseInitialTick = preparationPhaseEndedNetEvent.OccuredOnTick;
                _matchDataService.IsInPreparationPhase = false;
                _cachedPresentationEventsService.PreparationPhaseEndedNetEvents.Add(preparationPhaseEndedNetEvent);
            }
        }

        public void ProcessCreateSwapFieldEvents(CapacityList<CreateSwapFieldNetEventS2C> createSwapFieldNetEvents)
        {
            if (createSwapFieldNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var swapFieldCreatedEvent in createSwapFieldNetEvents)
            {
                _matchDataService.AddSwapField(swapFieldCreatedEvent.SwapFieldId,swapFieldCreatedEvent.CasterPlayerId, swapFieldCreatedEvent.OccuredOnTick, swapFieldCreatedEvent.EndOnTick, swapFieldCreatedEvent.MaxRadius);
                _cachedPresentationEventsService.CreateSwapFieldNetEvents.Add(swapFieldCreatedEvent);
            }
        }

        public void ProcessDeactivateSwapTalentEvents(CapacityList<DeactivateSwapTalentNetEventS2C> deactivateSwapTalentNetEvents)
        {
            if (deactivateSwapTalentNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in deactivateSwapTalentNetEvents)
            {
                var casterPlayer = _matchDataService.GetPlayer(netEvent.CasterPlayerId);
                var talents = casterPlayer.Spaceship.TalentsState.Talents;
                for (int i = 0; i < talents.Count; i++)
                {
                    ref var talent = ref talents.Get(i);
                    if (talent.TalentType == TalentType.Swap)
                    {
                        talent.NormalCooldown.CooldownEndTick = netEvent.TalentCooldownEndTick;
                        break;
                    }
                }
                _matchDataService.RemoveSwapField(netEvent.SwapFieldId);
                _cachedPresentationEventsService.DeactivateSwapTalentNetEvents.Add(netEvent);
            }
        }

        public void ProcessKOProjectHitPlayerEvents(CapacityList<KOProjectHitPlayerNetEventS2C> koProjectHitPlayerNetEvents)
        {
            if (koProjectHitPlayerNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in koProjectHitPlayerNetEvents)
            {
                _cachedPresentationEventsService.KOProjectHitPlayerNetEvents.Add(netEvent);
            }
        }

        public void ProcessCreateKOProjectileEvents(CapacityList<CreateKOProjectileNetEventS2C> createKOProjectileNetEvents)
        {
            if (createKOProjectileNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in createKOProjectileNetEvents)
            {
                _matchDataService.AddKOProjectile(netEvent.KoProjectile.Id, netEvent.KoProjectile.PlayerCasterId, netEvent.OccuredOnTick, netEvent.KoProjectile.Size);
                _cachedPresentationEventsService.CreateKOProjectileNetEvents.Add(netEvent);
            }
        }

        public void ProcessDeactivateKOTalentEvents(CapacityList<DeactivateKOTalentNetEventS2C> deactivateKOTalentNetEvents)
        {
            if (deactivateKOTalentNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in deactivateKOTalentNetEvents)
            {
                var casterPlayer = _matchDataService.GetPlayer(netEvent.CasterPlayerId);
                var talents = casterPlayer.Spaceship.TalentsState.Talents;
                for (int i = 0; i < talents.Count; i++)
                {
                    ref var talent = ref talents.Get(i);
                    if (talent.TalentType == TalentType.KO)
                    {
                        talent.NormalCooldown.CooldownEndTick = netEvent.TalentCooldownEndTick;
                        break;
                    }
                }
                _matchDataService.RemoveKOProjectile(netEvent.KoProjectileId);
                _cachedPresentationEventsService.DeactivateKOTalentNetEvents.Add(netEvent);
            }
        }

        public void ProcessPerformDashPulseEvents(CapacityList<PerformDashPulseNetEventS2C> performDashPulseNetEvents)
        {
            if (performDashPulseNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in performDashPulseNetEvents)
            {
                _cachedPresentationEventsService.PerformDashPulseNetEvents.Add(netEvent);
            }
        }

        public void ProcessUpdatePlayerTalentStocksEvents(CapacityList<UpdatePlayerTalentStocksNetEventS2C> updatePlayerTalentStocksEvents)
        {
            if (updatePlayerTalentStocksEvents.IsNullOrEmpty())
            {
                return;
            }
            
            var didFoundPlayerWithTalent = false;
            foreach (var netEvent in updatePlayerTalentStocksEvents)
            {
                var casterPlayer = _matchDataService.GetPlayer(netEvent.CasterPlayerId);
                var talents = casterPlayer.Spaceship.TalentsState.Talents;
                for (int i = 0; i < talents.Count; i++)
                {
                    ref var talent = ref talents.Get(i);
                    if (talent.TalentType == netEvent.TalentType)
                    {
                        talent.StocksCooldown.CurrentStocksAmount = netEvent.CurrentStocksAmount;
                        talent.StocksCooldown.RecieveNextStockOnTick = netEvent.RecieveNextStockOnTick;
                        didFoundPlayerWithTalent = true;
                        break;
                    }
                }

                if (didFoundPlayerWithTalent)
                {
                    _cachedPresentationEventsService.UpdatePlayerTalentStocksNetEvents.Add(netEvent);
                }
                else
                {
                    LogService.LogError($"Player with id {netEvent.CasterPlayerId} does not have talent {netEvent.TalentType}!");
                }
            }
        }

        public void ProcessActivateSentryGunTalentEvents(CapacityList<ActivateSentryGunTalentNetEventS2C> activateSentryGunTalentNetEvents)
        {
            if (activateSentryGunTalentNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in activateSentryGunTalentNetEvents)
            {
                if (IsTickProcessed(netEvent.OccuredOnTick))
                {
                    continue;
                }

                _cachedPresentationEventsService.ActivateSentryGunTalentNetEvents.Add(netEvent);
            }
        }

        public void ProcessDeactivateSentryGunTalentEvents(CapacityList<DeactivateSentryGunTalentNetEventS2C> deactivateSentryGunTalentNetEvents)
        {
            if (deactivateSentryGunTalentNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in deactivateSentryGunTalentNetEvents)
            {
                if (IsTickProcessed(netEvent.OccuredOnTick))
                {
                    continue;
                }

                _cachedPresentationEventsService.DeactivateSentryGunTalentNetEvents.Add(netEvent);
            }
        }
}
}
