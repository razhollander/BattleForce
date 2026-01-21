using System;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Features.TalentCards.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Commands.NetEventsCommands;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Presentation;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers
{
    public class SimulationNetEventsHandler
    {
        private readonly IMatchDataService _matchDataService;
        private readonly ICachedPresentationEventsService _cachedPresentationEventsService;
        private readonly IClientNetworkManager _networkManager;
        private readonly IPlayerControllers _playerControllers;
        private readonly NetworkConfig _networkConfig;
        private readonly IClientPresentationTickProcessor _clientPresentationTickProcessor;
        private readonly ICommandFactory _commandFactory;

        public SimulationNetEventsHandler(IMatchDataService matchDataService,
            ICachedPresentationEventsService iCachedPresentationEventsService, IClientNetworkManager networkManager,
            IPlayerControllers playerControllers, NetworkConfig networkConfig,
            IClientPresentationTickProcessor clientPresentationTickProcessor, ICommandFactory commandFactory)
        {
            _matchDataService = matchDataService;
            _cachedPresentationEventsService = iCachedPresentationEventsService;
            _networkManager = networkManager;
            _playerControllers = playerControllers;
            _networkConfig = networkConfig;
            _clientPresentationTickProcessor = clientPresentationTickProcessor;
            _commandFactory = commandFactory;
        }

        public void ProcessPlayerJoinedEvents(CapacityList<PlayerJoinAcceptPacketS2C> playerJoinAcceptNetEvents, ref int clientTick)
        {
            foreach (var playerJoinAcceptNetEvent in playerJoinAcceptNetEvents)
            {
                var playerId = playerJoinAcceptNetEvent.PlayerState.Id;
                var isLocalPlayer = playerJoinAcceptNetEvent.IsLocal;
                LogService.LogTopic(
                    $"Join packet accepted processed,  isLocalPlayer:{isLocalPlayer}, player id: " + playerId,
                    LogTopicType.ClientNetwork);
                
                if (isLocalPlayer)
                {
                    _commandFactory.CreateCommandVoid<SyncSimulationStateCommand>()
                        .SetSimulationState(playerJoinAcceptNetEvent.SimulationState).Execute();
                    SyncTickToServer(out clientTick, playerJoinAcceptNetEvent);
                    SetupLocalPlayer(playerId);
                }
                else
                {
                    var playerModel = _matchDataService.AddPlayer(playerJoinAcceptNetEvent.PlayerState);
                    _playerControllers.CreatePlayer(playerModel.PlayerId);
                }
            }
        }

        private void SyncTickToServer(out int clientTick, PlayerJoinAcceptPacketS2C playerJoinAcceptNetEvent)
        {
            var ticksPassedSinceServerSendPacket = (_networkManager.Ping / 1000f) / _networkConfig.DeltaTime;
            var tickWouldBeOnServerWhenReceiveMyPackets = (int)(ticksPassedSinceServerSendPacket * 2) + playerJoinAcceptNetEvent.OccuredOnTick;
            clientTick = tickWouldBeOnServerWhenReceiveMyPackets;
        }

        private void SetupLocalPlayer(int playerId)
        {
            _matchDataService.SetLocalPlayer(playerId);
            _clientPresentationTickProcessor.StartTick();
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
                playerModel.Spaceship.Health.CurrentHealth = Math.Min(playerModel.Spaceship.Health.CurrentHealth, playerTakeDamageEvent.PlayerHealth);// we do Min because the player may get hit multiple times the same frame
                LogService.LogTopic($"Player lose {playerTakeDamageEvent.HitDamage} health, and now has {playerModel.Spaceship.Health.CurrentHealth}");
                _cachedPresentationEventsService.PlayerTakeDamageNetEvents.Add(playerTakeDamageEvent);
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
                _cachedPresentationEventsService.TalentCardObtainedNetEvents.Add(talentCardObtainedNetEvent);
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
    }
}