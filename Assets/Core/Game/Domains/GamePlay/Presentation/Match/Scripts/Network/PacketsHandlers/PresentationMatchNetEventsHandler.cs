using System;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Game.Domains.GamePlay.Shared.Scripts.LocalEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents;
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
                foreach (var playerState in playerRejoinAcceptNetEvent.Players.AsSpan())       
                {
                    var isLocalPlayer = playerRejoinAcceptNetEvent.IsLocal;
                    LogService.LogTopic(
                        $"Join packet accepted processed,  isLocalPlayer:{isLocalPlayer}, player id: " + playerState.Id,
                        LogTopicType.ClientNetwork);

                    if (!isLocalPlayer)
                    {
                        _addMatchPlayerCommand.SetPlayerState(playerState).SetCurrentServerTick(currentServerTick).Execute();
                    }
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
                    bulletSpawnNetEvent.Position, bulletSpawnNetEvent.Velocity, bulletSpawnNetEvent.BulletRadius, bulletSpawnNetEvent.OccuredOnTick);
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

        public void ProcessPlayerSpinnedStartedEvents(CapacityList<PlayerSpinnedStartedNetEventS2C> playerSpinnedStartedNetEvents)
        {
            if (playerSpinnedStartedNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var playerSpinnedStartedNetEvent in playerSpinnedStartedNetEvents)
            {
                var player = _matchDataService.GetPlayer(playerSpinnedStartedNetEvent.PlayerId);
                player.Spaceship.IsSpinned = true;
                
                _cachedPresentationEventsService.PlayerSpinnedStartedNetEvents.Add(playerSpinnedStartedNetEvent);
            }
        }

        public void ProcessPlayerSpinnedEndedEvents(CapacityList<PlayerSpinnedEndedNetEventS2C> playerSpinnedEndedNetEvents)
        {
            if (playerSpinnedEndedNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var playerSpinnedEndedNetEvent in playerSpinnedEndedNetEvents)
            {
                var player = _matchDataService.GetPlayer(playerSpinnedEndedNetEvent.PlayerId);
                player.Spaceship.IsSpinned = false;

                _cachedPresentationEventsService.PlayerSpinnedEndedNetEvents.Add(playerSpinnedEndedNetEvent);
            }
        }

        public void ProcessPlayerStartedExposedToLavaEvents(CapacityList<PlayerStartedExposedToLavaNetEventS2C> playerStartedExposedToLavaNetEvents)
        {
            if (playerStartedExposedToLavaNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var playerStartedExposedToLavaNetEvent in playerStartedExposedToLavaNetEvents)
            {
                var player = _matchDataService.GetPlayer(playerStartedExposedToLavaNetEvent.PlayerId);
                player.Spaceship.IsExposedToLava = true;

                _cachedPresentationEventsService.PlayerStartedExposedToLavaNetEvents.Add(playerStartedExposedToLavaNetEvent);
            }
        }

        public void ProcessPlayerEndedExposedToLavaEvents(CapacityList<PlayerEndedExposedToLavaNetEventS2C> playerEndedExposedToLavaNetEvents)
        {
            if (playerEndedExposedToLavaNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var playerEndedExposedToLavaNetEvent in playerEndedExposedToLavaNetEvents)
            {
                var player = _matchDataService.GetPlayer(playerEndedExposedToLavaNetEvent.PlayerId);
                player.Spaceship.IsExposedToLava = false;

                _cachedPresentationEventsService.PlayerEndedExposedToLavaNetEvents.Add(playerEndedExposedToLavaNetEvent);
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

        public void ProcessMoleSpawnedEvents(CapacityList<MoleSpawnedNetEventS2C> moleSpawnedNetEvents)
        {
            if (moleSpawnedNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var moleSpawnedNetEvent in moleSpawnedNetEvents)
            {
                _matchDataService.AddMole(moleSpawnedNetEvent.MoleId, moleSpawnedNetEvent.Position.ToUnityVector2(),
                    moleSpawnedNetEvent.IsGolden, moleSpawnedNetEvent.MaxLives, moleSpawnedNetEvent.MaxLives);
                _cachedPresentationEventsService.MoleSpawnedNetEvents.Add(moleSpawnedNetEvent);
            }
        }

        public void ProcessMoleHitEvents(CapacityList<MoleHitNetEventS2C> moleHitNetEvents)
        {
            if (moleHitNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var moleHitNetEvent in moleHitNetEvents)
            {
                _matchDataService.RemoveMole(moleHitNetEvent.MoleId);
                _matchDataService.SetTeamMolesHit(moleHitNetEvent.ByTeamId, moleHitNetEvent.TeamMolesHitTotal);
                _matchDataService.SetPlayerMolesHitScore(moleHitNetEvent.ByPlayerId, moleHitNetEvent.ByPlayerMolesHitScoreTotal);
                _cachedPresentationEventsService.MoleHitNetEvents.Add(moleHitNetEvent);
            }
        }

        // Bonus-score storage is shared with Whac-A-Mole (SetTeamMolesHit / SetPlayerMolesHitScore), so a GatePass pass
        // updates the same team board and per-player UI. The gate tint uses the client gate model, updated here too.
        public void ProcessScoreGatePassedEvents(CapacityList<ScoreGatePassedNetEventS2C> scoreGatePassedNetEvents)
        {
            if (scoreGatePassedNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var scoreGatePassedNetEvent in scoreGatePassedNetEvents)
            {
                _matchDataService.SetTeamMolesHit(scoreGatePassedNetEvent.ByTeamId, scoreGatePassedNetEvent.TeamBonusScoreTotal);
                _matchDataService.SetPlayerMolesHitScore(scoreGatePassedNetEvent.ByPlayerId, scoreGatePassedNetEvent.ByPlayerBonusScoreTotal);
                _matchDataService.SetScoreGateLastScoredTeam(scoreGatePassedNetEvent.ScoreGateId, scoreGatePassedNetEvent.ByTeamId);
                _matchDataService.SetScoreGateMultiplier(scoreGatePassedNetEvent.ScoreGateId, scoreGatePassedNetEvent.NewScoreMultiplier);
                _cachedPresentationEventsService.ScoreGatePassedNetEvents.Add(scoreGatePassedNetEvent);
            }
        }

        // The trap's whole cycle is derived from this one event, so the model only needs the state it puts the trap in;
        // UpdateGateTraps walks it from Closing all the way back to Open on its own.
        public void ProcessGateTrapClosingEvents(CapacityList<GateTrapClosingNetEventS2C> gateTrapClosingNetEvents)
        {
            if (gateTrapClosingNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var gateTrapClosingNetEvent in gateTrapClosingNetEvents)
            {
                var gateTrapModel = _matchDataService.GetGateTrap(gateTrapClosingNetEvent.GateTrapId);

                if (gateTrapModel == null)
                {
                    continue;
                }

                gateTrapModel.State = GateTrapState.Closing;
                gateTrapModel.StateEndTick = gateTrapClosingNetEvent.ClosedOnTick;
                gateTrapModel.IsWaitingForOpenCooldown = false;
            }
        }

        // An expired mole is not gone yet, it only starts its pre-hide shake and stays hittable until it goes into its
        // hole, so the model keeps it here. A mole caught during that shake is dropped by ProcessMoleHitEvents, and one
        // that hides on its own leaves no event, so its stale model entry is cleared with the rest on the next full sync.
        public void ProcessMoleExpiredEvents(CapacityList<MoleExpiredNetEventS2C> moleExpiredNetEvents)
        {
            if (moleExpiredNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var moleExpiredNetEvent in moleExpiredNetEvents)
            {
                _cachedPresentationEventsService.MoleExpiredNetEvents.Add(moleExpiredNetEvent);
            }
        }

        public void ProcessGoldenMoleDamagedEvents(CapacityList<GoldenMoleDamagedNetEventS2C> goldenMoleDamagedNetEvents)
        {
            if (goldenMoleDamagedNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var goldenMoleDamagedNetEvent in goldenMoleDamagedNetEvents)
            {
                var moleModel = _matchDataService.GetMole(goldenMoleDamagedNetEvent.MoleId);

                if (moleModel != null)
                {
                    moleModel.RemainingLives = goldenMoleDamagedNetEvent.RemainingLives;
                }

                _cachedPresentationEventsService.GoldenMoleDamagedNetEvents.Add(goldenMoleDamagedNetEvent);
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
                _matchDataService.CurrentStageWinnerTeamId = stageEndNetEvent.WinningTeamId;
                _matchDataService.IsInShowoffWinners = true;
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

        public void ProcessEnvironmentSpikePlayerCollisionEvents(CapacityList<EnvironmentSpikePlayerCollisionNetEventS2C> environmentSpikePlayerCollisionNetEvents)
        {
            if (environmentSpikePlayerCollisionNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var environmentSpikePlayerCollisionNetEvent in environmentSpikePlayerCollisionNetEvents)
            {
                _cachedPresentationEventsService.EnvironmentSpikePlayerCollisionNetEvents.Add(environmentSpikePlayerCollisionNetEvent);
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
                _matchDataService.PreperationPhaseEndedOnTick = preparationPhaseEndedNetEvent.OccuredOnTick;
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

            foreach (var netEvent in createSwapFieldNetEvents)
            {
                SetPlayerTalentActive(netEvent.CasterPlayerId, TalentType.Swap);
                _matchDataService.AddSwapField(netEvent.SwapFieldId,netEvent.CasterPlayerId, netEvent.OccuredOnTick, netEvent.EndOnTick, netEvent.MaxRadius);
                _cachedPresentationEventsService.CreateSwapFieldNetEvents.Add(netEvent);
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
                SetPlayerTalentDeactive(netEvent.CasterPlayerId, TalentType.Swap, netEvent.TalentCooldownEndTick);
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
                SetPlayerTalentActive(netEvent.KoProjectile.PlayerCasterId, TalentType.KO);
                _matchDataService.AddKOProjectile(netEvent.KoProjectile.Id, netEvent.KoProjectile.PlayerCasterId, netEvent.OccuredOnTick, netEvent.KoProjectile.Size);
                _cachedPresentationEventsService.CreateKOProjectileNetEvents.Add(netEvent);
            }
        }

        public void ProcessCreatePlayerGrapplingHookProjectileEvents(CapacityList<CreateGrapplingHookProjectileNetEventS2C> events)
        {
            if (events.IsNullOrEmpty())
            {
                return;
            }
            
            foreach (var netEvent in events)
            {
                var grapplingHookState = netEvent.GrapplingHookProjectile;
                SetPlayerTalentActive(grapplingHookState.PlayerCasterId, TalentType.GrapplingHook);
                _matchDataService.AddGrapplingHookProjectile(grapplingHookState.Id, grapplingHookState.PlayerCasterId, grapplingHookState.Position);
                _cachedPresentationEventsService.CreateGrapplingHookProjectileNetEvents.Add(netEvent);
            }
        }

        public void ProcessGrapplingHookHitWallEvents(CapacityList<GrapplingHookHitWallNetEventS2C> events)
        {
            if (events.IsNullOrEmpty())
            {
                return;
            }
            
            foreach (var netEvent in events)
            {
                _cachedPresentationEventsService.GrapplingHookHitWallNetEvents.Add(netEvent);
            }
        }

        public void ProcessDeactivateGrapplingHookTalentEvents(CapacityList<DeactivateGrapplingHookTalentNetEventS2C> events)
        {
            if (events.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in events)
            {
                SetPlayerTalentDeactive(netEvent.CasterPlayerId, TalentType.GrapplingHook, netEvent.TalentCooldownEndTick);
                _cachedPresentationEventsService.DeactivateGrapplingHookTalentNetEvents.Add(netEvent);
            }
        }

        public void ProcessCreateFishingRodProjectileEvents(CapacityList<CreateFishingRodProjectileNetEventS2C> events)
        {
            if (events.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in events)
            {
                var tipState = netEvent.FishingRodProjectile;
                SetPlayerTalentActive(tipState.PlayerCasterId, TalentType.FishingRod);
                _matchDataService.AddFishingRodTip(tipState.Id, tipState.PlayerCasterId, tipState.Position, tipState.Phase, tipState.CaughtEnemyId, tipState.CaughtEnemyType);
                _cachedPresentationEventsService.CreateFishingRodProjectileNetEvents.Add(netEvent);
            }
        }

        public void ProcessFishingRodCaughtEnemyEvents(CapacityList<FishingRodCaughtEnemyNetEventS2C> events)
        {
            if (events.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in events)
            {
                var tipModel = _matchDataService.GetFishingRodTip(netEvent.ProjectileId);
                tipModel.Phase = FishingRodTipPhase.CaughtEnemy;
                tipModel.CaughtEnemyId = netEvent.CaughtEnemyId;
                tipModel.CaughtEnemyType = netEvent.CaughtEnemyType;
                _cachedPresentationEventsService.FishingRodCaughtEnemyNetEvents.Add(netEvent);
            }
        }

        public void ProcessFishingRodTipHitWallEvents(CapacityList<FishingRodTipHitWallNetEventS2C> events)
        {
            if (events.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in events)
            {
                //_matchDataService.GetFishingRodTip(netEvent.ProjectileId).Phase = FishingRodTipPhase.ReturningBackwards; no need since the client doesn't care, FishingRodTipPhase.ReturningBackwards is only used for server side
                _cachedPresentationEventsService.FishingRodTipHitWallNetEvents.Add(netEvent);
            }
        }

        public void ProcessFishingRodThrowEvents(CapacityList<FishingRodThrowNetEventS2C> events)
        {
            if (events.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in events)
            {
                _cachedPresentationEventsService.FishingRodThrowNetEvents.Add(netEvent);
            }
        }

        public void ProcessDeactivateFishingRodTalentEvents(CapacityList<DeactivateFishingRodTalentNetEventS2C> events)
        {
            if (events.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in events)
            {
                SetPlayerTalentDeactive(netEvent.CasterPlayerId, TalentType.FishingRod, netEvent.TalentCooldownEndTick);
                _cachedPresentationEventsService.DeactivateFishingRodTalentNetEvents.Add(netEvent);
            }
        }

        public void ProcessCreateSoulGhostEvents(CapacityList<CreateSoulGhostNetEventS2C> events)
        {
            if (events.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in events)
            {
                var ghostState = netEvent.SoulGhost;
                SetPlayerTalentActive(ghostState.PlayerCasterId, TalentType.Soul);
                _matchDataService.AddSoulGhost(ghostState.Id, ghostState.PlayerCasterId, ghostState.Position, ghostState.Direction);
                _cachedPresentationEventsService.CreateSoulGhostNetEvents.Add(netEvent);
            }
        }

        public void ProcessDeactivateSoulTalentEvents(CapacityList<DeactivateSoulTalentNetEventS2C> events)
        {
            if (events.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in events)
            {
                SetPlayerTalentDeactive(netEvent.CasterPlayerId, TalentType.Soul, netEvent.TalentCooldownEndTick);
                _matchDataService.RemoveSoulGhost(netEvent.GhostId);
                _cachedPresentationEventsService.DeactivateSoulTalentNetEvents.Add(netEvent);
            }
        }

        public void ProcessShootFrigidBlockEvents(CapacityList<ShootFrigidBlockNetEventS2C> events)
        {
            if (events.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in events)
            {
                var block = netEvent.FrigidBlock;
                SetPlayerTalentDeactive(block.PlayerCasterId, TalentType.FrigidBlock, netEvent.CooldownEndTick);
                _matchDataService.AddFrigidBlock(block.Id, block.PlayerCasterId, block.Position, block.Rotation);
                _cachedPresentationEventsService.ShootFrigidBlockNetEvents.Add(netEvent);
            }
        }

        public void ProcessDestroyFrigidBlockEvents(CapacityList<DestroyFrigidBlockNetEventS2C> events)
        {
            if (events.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in events)
            {
                _matchDataService.RemoveFrigidBlock(netEvent.BlockId);
                _cachedPresentationEventsService.DestroyFrigidBlockNetEvents.Add(netEvent);
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
                SetPlayerTalentDeactive(netEvent.CasterPlayerId, TalentType.KO, netEvent.TalentCooldownEndTick);
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

        public void ProcessActivateUmbrellaTalentEvents(CapacityList<ActivateUmbrellaTalentNetEventS2C> netEvents)
        {
            if (netEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in netEvents)
            {
                SetPlayerTalentActive(netEvent.CasterPlayerId, TalentType.Umbrella);
                _cachedPresentationEventsService.ActivateUmbrellaTalentNetEvents.Add(netEvent);
            }
        }

        public void ProcessLayChickenEggEvents(CapacityList<LayChickenEggNetEventS2C> netEvents)
        {
            if (netEvents.IsNullOrEmpty()) return;
            foreach (var netEvent in netEvents)
            {
                _matchDataService.AddChickenEgg(netEvent.EggId, netEvent.CasterPlayerId, netEvent.Position.ToUnityVector2());
                _cachedPresentationEventsService.LayChickenEggNetEvents.Add(netEvent);
            }
        }

        public void ProcessChickenEggHitEvents(CapacityList<ChickenEggHitNetEventS2C> netEvents)
        {
            if (netEvents.IsNullOrEmpty()) return;
            foreach (var netEvent in netEvents)
            {
                _matchDataService.RemoveChickenEgg(netEvent.EggId);
                _cachedPresentationEventsService.ChickenEggHitNetEvents.Add(netEvent);
            }
        }
        
        public void ProcessDeactivateUmbrellaTalentEvents(CapacityList<DeactivateUmbrellaTalentNetEventS2C> netEvents)
        {
            if (netEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in netEvents)
            {
                SetPlayerTalentDeactive(netEvent.CasterPlayerId, TalentType.Umbrella, netEvent.TalentCooldownEndTick);
                _cachedPresentationEventsService.DeactivateUmbrellaTalentNetEvents.Add(netEvent);
            }
        }

        public void ProcessActivateWaterGunTalentEvents(CapacityList<ActivateWaterGunTalentNetEventS2C> netEvents)
        {
            if (netEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in netEvents)
            {
                SetPlayerTalentActive(netEvent.CasterPlayerId, TalentType.WaterGun);
                _cachedPresentationEventsService.ActivateWaterGunTalentNetEvents.Add(netEvent);
            }
        }

        public void ProcessDeactivateWaterGunTalentEvents(CapacityList<DeactivateWaterGunTalentNetEventS2C> netEvents)
        {
            if (netEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in netEvents)
            {
                SetPlayerTalentDeactive(netEvent.CasterPlayerId, TalentType.WaterGun, netEvent.TalentCooldownEndTick);
                _cachedPresentationEventsService.DeactivateWaterGunTalentNetEvents.Add(netEvent);
            }
        }

        public void ProcessPlayerMaxShootCooldownChangedEvents(CapacityList<PlayerMaxShootCooldownChangedNetEventS2C> netEvents)
        {
            if (netEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in netEvents)
            {
                var player = _matchDataService.GetPlayer(netEvent.PlayerId);
                var shoot = player.Spaceship.Shoot;
                shoot.MaxCooldown = netEvent.MaxShootCooldown;
                shoot.CooldownSecondsLeft = netEvent.ShootCooldownSecondsLeft;
                player.Spaceship.Shoot = shoot;
            }
        }

        public void ProcessCreateMagenticPullFieldEvents(CapacityList<CreateMagneticPullFieldNetEventS2C> netEvents)
        {
            if (netEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in netEvents)
            {
                SetPlayerTalentDeactive(netEvent.CasterPlayerId, TalentType.MagneticPull, netEvent.TalentCooldownEndTick);
                _cachedPresentationEventsService.CreateMagenticPullFieldNetEvents.Add(netEvent);
            }
        }

        public void ProcessActivateYearsOfPainTalentEvents(CapacityList<ActivateYearsOfPainTalentNetEventS2C> netEvents)
        {
            if (netEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in netEvents)
            {
                SetPlayerTalentDeactive(netEvent.CasterPlayerId, TalentType.YearsOfPain, netEvent.TalentCooldownEndTick);
                _cachedPresentationEventsService.ActivateYearsOfPainTalentNetEvents.Add(netEvent);
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

        public void ProcessPlayerSelectedTalentFinishedCooldownEvents(CapacityList<PlayerSelectedTalentFinishedCooldownLocalEvent> playerSelectedTalentFinishedCooldownEvents)
        {
            if (playerSelectedTalentFinishedCooldownEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var localEvent in playerSelectedTalentFinishedCooldownEvents)
            {
                _cachedPresentationEventsService.PlayerSelectedTalentFinishedCooldownLocalEvents.Add(localEvent);
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
                SetPlayerTalentActive(netEvent.CasterPlayerId, TalentType.SentryGun);
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
                SetPlayerTalentDeactive(netEvent.CasterPlayerId, TalentType.SentryGun, netEvent.TalentCooldownEndTick);
                _cachedPresentationEventsService.DeactivateSentryGunTalentNetEvents.Add(netEvent);
            }
        }

        public void ProcessActivateRockTalentEvents(CapacityList<ActivateRockTalentNetEventS2C> activateRockTalentNetEvents)
        {
            if (activateRockTalentNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in activateRockTalentNetEvents)
            {
                SetPlayerTalentActive(netEvent.CasterPlayerId, TalentType.Rock);
                _cachedPresentationEventsService.ActivateRockTalentNetEvents.Add(netEvent);
            }
        }

        public void ProcessDeactivateRockTalentEvents(CapacityList<DeactivateRockTalentNetEventS2C> deactivateRockTalentNetEvents)
        {
            if (deactivateRockTalentNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in deactivateRockTalentNetEvents)
            {
                SetPlayerTalentDeactive(netEvent.CasterPlayerId, TalentType.Rock, netEvent.TalentCooldownEndTick);
                _cachedPresentationEventsService.DeactivateRockTalentNetEvents.Add(netEvent);
            }
        }

        public void ProcessActivateFrozenTalentEvents(CapacityList<ActivateFrozenTalentNetEventS2C> activateFrozenTalentNetEvents)
        {
            if (activateFrozenTalentNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in activateFrozenTalentNetEvents)
            {
                SetPlayerTalentActive(netEvent.CasterPlayerId, TalentType.Frozen);
                _cachedPresentationEventsService.ActivateFrozenTalentNetEvents.Add(netEvent);
            }
        }

        public void ProcessDeactivateFrozenTalentEvents(CapacityList<DeactivateFrozenTalentNetEventS2C> deactivateFrozenTalentNetEvents)
        {
            if (deactivateFrozenTalentNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in deactivateFrozenTalentNetEvents)
            {
                SetPlayerTalentDeactive(netEvent.CasterPlayerId, TalentType.Frozen, netEvent.TalentCooldownEndTick);
                _cachedPresentationEventsService.DeactivateFrozenTalentNetEvents.Add(netEvent);
            }
        }

        private void SetPlayerTalentActive(ushort playerId, TalentType talentType)
        {
            var casterPlayer = _matchDataService.GetPlayer(playerId);
            var talents = casterPlayer.Spaceship.TalentsState.Talents;
            for (int i = 0; i < talents.Count; i++)
            {
                ref var talent = ref talents.Get(i);
                if (talent.TalentType == talentType)
                {
                    talent.IsCurrentlyActive = true;
                    break;
                }
            }
        }
        
        private void SetPlayerTalentDeactive(ushort playerId, TalentType talentType, int talentCooldownEndTick)
        {
            var casterPlayer = _matchDataService.GetPlayer(playerId);
            var talents = casterPlayer.Spaceship.TalentsState.Talents;
            for (int i = 0; i < talents.Count; i++)
            {
                ref var talent = ref talents.Get(i);
                if (talent.TalentType == talentType)
                {
                    talent.NormalCooldown.CooldownEndTick = talentCooldownEndTick;
                    talent.IsCurrentlyActive = false;
                    break;
                }
            }
        }

        public void ProcessPlayerLockOnTargetsChangedEvents(CapacityList<PlayerLockOnTargetsChangedNetEventS2C> playerLockOnTargetsChangedNetEvents)
        {
            if (playerLockOnTargetsChangedNetEvents.IsNullOrEmpty())
            {
                return;
            }
            
            foreach (var netEvent in playerLockOnTargetsChangedNetEvents)
            {
                var player = _matchDataService.GetPlayer(netEvent.PlayerId);
                player.Spaceship.LockOnTargetObjects.Clear();
                
                for (int i = 0; i < netEvent.LockedOnTargetObjects.Count; i++)
                {
                    ref var objectLockedOnTarget = ref player.Spaceship.LockOnTargetObjects.AddAndGet();
                    objectLockedOnTarget = netEvent.LockedOnTargetObjects[i];
                }
                
                _cachedPresentationEventsService.PlayerLockOnTargetsChangedNetEvents.Add(netEvent);
            }
        }

        public void ProcessPlayerLockedOnTargetHitEvents(CapacityList<PlayerLockedOnTargetHitNetEventS2C> playerLockedOnTargetHitNetEvents)
        {
            if (playerLockedOnTargetHitNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in playerLockedOnTargetHitNetEvents)
            {
                _cachedPresentationEventsService.PlayerLockedOnTargetHitNetEvents.Add(netEvent);
            }
        }

        public void ProcessPlayerPowerUpChangedEvents(CapacityList<PlayerPowerUpChangedNetEventS2C> playerPowerUpChangedNetEvents)
        {
            if (playerPowerUpChangedNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in playerPowerUpChangedNetEvents)
            {
                _matchDataService.GetPlayer(netEvent.PlayerId).Spaceship.CurrentPowerUp = netEvent.PowerUp;
                _cachedPresentationEventsService.PlayerPowerUpChangedNetEvents.Add(netEvent);
            }
        }

        public void ProcessActivateSonicSlapEvents(CapacityList<ActivateSonicSlapNetEventS2C> sonicSlapActivatedNetEvents)
        {
            if (sonicSlapActivatedNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in sonicSlapActivatedNetEvents)
            {
                _cachedPresentationEventsService.ActivateSonicSlapNetEvents.Add(netEvent);
            }
        }

        public void ProcessStartPowerUpGrantingPhaseEvents(CapacityList<StartPowerUpGrantingPhaseNetEventS2C> events)
        {
            if (events.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in events)
            {
                _matchDataService.GetPlayer(netEvent.PlayerId).Spaceship.IsCurrentlyInGrantingPowerUpPhase = true;
                _cachedPresentationEventsService.StartPowerUpGrantingPhaseNetEvents.Add(netEvent);
            }
        }

        public void ProcessEndPowerUpGrantingPhaseEvents(CapacityList<EndPowerUpGrantingPhaseNetEventS2C> events)
        {
            if (events.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in events)
            {
                var spaceship = _matchDataService.GetPlayer(netEvent.PlayerId).Spaceship;
                spaceship.IsCurrentlyInGrantingPowerUpPhase = false;
                spaceship.CurrentPowerUp = netEvent.GrantedPowerUp;
                _cachedPresentationEventsService.EndPowerUpGrantingPhaseNetEvents.Add(netEvent);
            }
        }

        public void ProcessPerformGalacticPullEvents(CapacityList<PerformGalacticPullNetEventS2C> events)
        {
            if (events.IsNullOrEmpty()) return;
            foreach (var netEvent in events)
                _cachedPresentationEventsService.PerformGalacticPullNetEvents.Add(netEvent);
        }

        public void ProcessDeactivateGalacticForceFieldEvents(CapacityList<DeactivateGalacticForceFieldNetEventS2C> events)
        {
            if (events.IsNullOrEmpty()) return;
            foreach (var netEvent in events)
                _cachedPresentationEventsService.DeactivateGalacticForceFieldNetEvents.Add(netEvent);
        }

        public void ProcessActivateNukePowerUpEvents(CapacityList<ActivateNukePowerUpNetEventS2C> events)
        {
            if (events.IsNullOrEmpty()) return;
            foreach (var netEvent in events)
                _cachedPresentationEventsService.ActivateNukePowerUpNetEvents.Add(netEvent);
        }

        public void ProcessDeactivateShufflePowerUpEvents(CapacityList<DeactivateShufflePowerUpNetEventS2C> events)
        {
            if (events.IsNullOrEmpty()) return;
            foreach (var netEvent in events)
                _cachedPresentationEventsService.DeactivateShufflePowerUpNetEvents.Add(netEvent);
        }

        public void ProcessShuffleSwapPlayerPositionEvents(CapacityList<ShuffleSwapPlayerPositionNetEventS2C> events)
        {
            if (events.IsNullOrEmpty()) return;
            foreach (var netEvent in events)
                _cachedPresentationEventsService.ShuffleSwapPlayerPositionNetEvents.Add(netEvent);
        }

        public void ProcessActivateShuffleEvents(CapacityList<ActivateShuffleNetEventS2C> events)
        {
            if (events.IsNullOrEmpty()) return;
            foreach (var netEvent in events)
                _cachedPresentationEventsService.ActivateShuffleNetEvents.Add(netEvent);
        }

        public void ProcessActivateHeadbuttChargingEvents(CapacityList<ActivateHeadbuttChargingNetEventS2C> netEvents)
        {
            if (netEvents.IsNullOrEmpty()) return;

            foreach (var netEvent in netEvents)
            {
                SetPlayerTalentActive(netEvent.CasterPlayerId, TalentType.Headbutt);
                _cachedPresentationEventsService.ActivateHeadbuttChargingNetEvents.Add(netEvent);
            }
        }

        public void ProcessPerformHeadbuttDashEvents(CapacityList<PerformHeadbuttDashNetEventS2C> netEvents)
        {
            if (netEvents.IsNullOrEmpty()) return;

            foreach (var netEvent in netEvents)
            {
                _cachedPresentationEventsService.PerformHeadbuttDashNetEvents.Add(netEvent);
            }
        }

        public void ProcessPerformBarrelDashEvents(CapacityList<PerformBarrelDashNetEventS2C> netEvents)
        {
            if (netEvents.IsNullOrEmpty()) return;

            foreach (var netEvent in netEvents)
            {
                _cachedPresentationEventsService.PerformBarrelDashNetEvents.Add(netEvent);
            }
        }

        public void ProcessHeadbuttHitEnemyEvents(CapacityList<HeadbuttHitEnemyNetEventS2C> netEvents)
        {
            if (netEvents.IsNullOrEmpty()) return;

            foreach (var netEvent in netEvents)
            {
                _cachedPresentationEventsService.HeadbuttHitEnemyNetEvents.Add(netEvent);
            }
        }

        public void ProcessDeactivateHeadbuttTalentEvents(CapacityList<DeactivateHeadbuttTalentNetEventS2C> netEvents)
        {
            if (netEvents.IsNullOrEmpty()) return;

            foreach (var netEvent in netEvents)
            {
                SetPlayerTalentDeactive(netEvent.CasterPlayerId, TalentType.Headbutt, netEvent.TalentCooldownEndTick);
                _cachedPresentationEventsService.DeactivateHeadbuttTalentNetEvents.Add(netEvent);
            }
        }
    }
}
