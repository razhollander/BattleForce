using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Commands;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.TickProcessor;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Presentation.Scripts.TickProcessors;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Network.PacketsHandlers
{
    public class PresentationMatchMakingNetEventsHandler
    {
        private readonly IMatchMakingDataService _matchDataService;
        private readonly ICachedPresentationEventsService _cachedPresentationEventsService;
        private readonly IClientNetworkManager _networkManager;
        private readonly NetworkConfig _networkConfig;
        private readonly IClientMatchMakingPresentationTickProcessor _clientPresentationTickProcessor;
        private readonly ICommandFactory _commandFactory;
        private readonly ITickCounterService _tickCounterService;
        private readonly AddMatchMakingPlayerCommand _addMatchMakingPlayerCommand;
        
        public PresentationMatchMakingNetEventsHandler(IMatchMakingDataService matchDataService,
            ICachedPresentationEventsService iCachedPresentationEventsService, IClientNetworkManager networkManager,
            NetworkConfig networkConfig,
            IClientMatchMakingPresentationTickProcessor clientPresentationTickProcessor, ICommandFactory commandFactory, ITickCounterService tickCounterService)
        {
            _matchDataService = matchDataService;
            _cachedPresentationEventsService = iCachedPresentationEventsService;
            _networkManager = networkManager;
            _networkConfig = networkConfig;
            _clientPresentationTickProcessor = clientPresentationTickProcessor;
            _commandFactory = commandFactory;
            _tickCounterService = tickCounterService;
            _addMatchMakingPlayerCommand = _commandFactory.CreateCommandVoid<AddMatchMakingPlayerCommand>();
        }

        public void ProcessPlayerJoinedEvents(CapacityList<MatchMakingPlayerJoinAcceptPacketS2C> playerJoinAcceptNetEvents)
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
                    _commandFactory.CreateCommandVoid<SyncMatchMakingSimulationStateCommand>()
                        .SetSimulationState(playerJoinAcceptNetEvent.SimulationState).Execute();
                    SyncTickToServer(playerJoinAcceptNetEvent);
                    SetupLocalPlayer(playerId);
                }
                else
                {
                    _addMatchMakingPlayerCommand.SetPlayerState(playerJoinAcceptNetEvent.PlayerState).Execute();
                }
            }
        }

        private void SyncTickToServer(MatchMakingPlayerJoinAcceptPacketS2C playerJoinAcceptNetEvent)
        {
            var ticksPassedSinceServerSendPacket = (_networkManager.Ping / 1000f) / _networkConfig.DeltaTime;
            var tickWouldBeOnServerWhenReceiveMyPackets = (int)(ticksPassedSinceServerSendPacket * 2) + playerJoinAcceptNetEvent.OccuredOnTick;
            _tickCounterService.SetTick(tickWouldBeOnServerWhenReceiveMyPackets);
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
    }
}