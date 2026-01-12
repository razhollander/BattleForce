using System;
using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Commands.NetEventsCommands;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Presentation;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
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
        private readonly IMatchNetEventsDataService _matchNetEventsDataService;
        private readonly IClientNetworkManager _networkManager;
        private readonly IPlayerControllers _playerControllers;
        private readonly NetworkConfig _networkConfig;
        private readonly IClientPresentationTickProcessor _clientPresentationTickProcessor;
        private readonly ICommandFactory _commandFactory;
        private ITalentCardControllers _talentCardControllers;
        private ITalentCardObtainedEffectController _talentCardObtainedEffectController;

        public SimulationNetEventsHandler(IMatchDataService matchDataService,
            IMatchNetEventsDataService matchNetEventsDataService, IClientNetworkManager networkManager,
            IPlayerControllers playerControllers, NetworkConfig networkConfig,
            IClientPresentationTickProcessor clientPresentationTickProcessor, ICommandFactory commandFactory,
            ITalentCardControllers talentCardControllers, ITalentCardObtainedEffectController talentCardObtainedEffectController)
        {
            _matchDataService = matchDataService;
            _matchNetEventsDataService = matchNetEventsDataService;
            _networkManager = networkManager;
            _playerControllers = playerControllers;
            _networkConfig = networkConfig;
            _clientPresentationTickProcessor = clientPresentationTickProcessor;
            _commandFactory = commandFactory;
            _talentCardControllers = talentCardControllers;
            _talentCardObtainedEffectController = talentCardObtainedEffectController;
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
                _matchNetEventsDataService.BulletSpawnNetEvents.Add(bulletSpawnNetEvent);
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
                _matchNetEventsDataService.PlayerTakeDamageNetEvents.Add(playerTakeDamageEvent);
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
                _matchNetEventsDataService.BulletDestroyedNetEvents.Add(bulletDestroyedEvent);
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
                _matchNetEventsDataService.PlayerSwapNetEvents.Add(playerSwapEvent);
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
                var cardId = talentCardObtainedNetEvent.CardId;
                _matchNetEventsDataService.AddTalentCardObtainedNetEvent(talentCardObtainedNetEvent.OccuredOnTick, cardId, talentCardObtainedNetEvent.PlayerId);

                var player = _matchDataService.GetPlayer(talentCardObtainedNetEvent.PlayerId);
                if (player == null)
                {
                    LogService.LogError($"Player with id {talentCardObtainedNetEvent.PlayerId} not found when processing TalentCardObtainedNetEvent");
                    continue;
                }

                // Assuming card position is available or passed.
                // The event doesn't carry position, so we might need to know where the card was.
                // However, since the card is about to be destroyed, we might want to get its position from the view or model BEFORE it is destroyed?
                // But the model might already have it removed if we synced state?
                // Actually the model is simulation state, which is updated via full tick packets.
                // If we are processing this event, it means we received a full tick packet.
                // The FullTickPacket contains CurrentSimulationState.

                // However, the task says "draw a line renderer from the card center and the player who hit it".
                // If the card is destroyed in simulation, its position might still be available if we haven't processed the state update that removes it yet?
                // Or maybe we can't get it easily from simulation state if it's already gone.
                // BUT, `TalentCardControllers` manages the views. The view has a position.
                // We should get position from the view before destroying it.
                // But `TalentCardControllers` doesn't expose a way to get card position easily by ID.
                // I updated `TalentCardControllers` to have a list of controllers.

                // I will add a method to `ITalentCardControllers` to get position or just handle the effect?
                // But `TalentCardObtainedEffectController` is separate.

                // Let's assume we can get the card position from `_matchDataService` if it's not removed yet?
                // The simulation state in `FullTickPacket` is the *current* state. If the card was obtained, it might be gone from that state.
                // However, we are in the client. The client maintains `_matchDataService`.
                // `ProcessTalentCardObtainedEvents` is called in `ProcessStateLatestTick` *before* `UpdatePlayersDeltas` and `UpdateBulletsTransform`?
                // Wait, `ProcessStateLatestTick` calls `Process...Events` then uses `latestFullTickPacket.CurrentSimulationState`.

                // The `MatchDataService` on client has `TalentCards`?
                // `MatchDataService` seems to hold players and bullets. Does it hold TalentCards?
                // Let's check `IMatchDataService`.

                // If `MatchDataService` doesn't hold TalentCards, `TalentCardControllers` definitely holds the views.

                // I'll modify `ITalentCardControllers` to `TryGetCardPosition(ushort cardId, out Vector2 position)`.

                if (_talentCardControllers.TryGetCardPosition(cardId, out var cardPosition))
                {
                    _talentCardObtainedEffectController.PlayEffect(cardPosition, player.Spaceship.Transform.Position.ToUnityVector2());
                }

                _talentCardControllers.DestroyTalentCard(cardId);
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
                _matchNetEventsDataService.AddTalentCardHitNetEvent(talentCardHitNetEvent.OccuredOnTick, talentCardHitNetEvent.CardId, talentCardHitNetEvent.BulletBelongToPlayerId);
                _talentCardControllers.SetTalentCardDamaged(talentCardHitNetEvent.CardId);
            }
        }
    }
}