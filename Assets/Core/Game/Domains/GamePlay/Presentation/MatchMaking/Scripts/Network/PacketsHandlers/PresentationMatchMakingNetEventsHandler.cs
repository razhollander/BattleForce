using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.StartMatchButton.Scripts.Mvcs;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Commands;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.MatchMaking.PacketEvents.NetEvents;
using Core.Scripts.Extensions;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Network.PacketsHandlers
{
    public class PresentationMatchMakingNetEventsHandler
    {
        private readonly IMatchMakingDataService _matchDataService;
        private readonly ICachedPresentationEventsService _cachedPresentationEventsService;
        private readonly ICommandFactory _commandFactory;
        private readonly IStartMatchButtonController _startMatchButtonController;
        private readonly AddMatchMakingPlayerCommand _addMatchMakingPlayerCommand;

        public PresentationMatchMakingNetEventsHandler(IMatchMakingDataService matchDataService,
            ICachedPresentationEventsService cachedPresentationEventsService, ICommandFactory commandFactory, IStartMatchButtonController startMatchButtonController)
        {
            _matchDataService = matchDataService;
            _cachedPresentationEventsService = cachedPresentationEventsService;
            _commandFactory = commandFactory;
            _startMatchButtonController = startMatchButtonController;
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
                
                if (!isLocalPlayer)
                {
                    _addMatchMakingPlayerCommand.SetPlayerState(playerJoinAcceptNetEvent.PlayerState).Execute();
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

        public void ProcessPlayerSwitchTeamEvents(CapacityList<PlayerSwitchTeamNetEventS2C> playerSwitchTeamNetEvents)
        {
            if (playerSwitchTeamNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in playerSwitchTeamNetEvents)
            {
                _matchDataService.UpdatePlayerTeam(netEvent.PlayerId, netEvent.TeamId);
                _cachedPresentationEventsService.PlayerSwitchTeamNetEvents.Add(netEvent);
            }
        }

        public void ProcessStartMatchCountdownEvents(CapacityList<StartMatchCountdownNetEventS2C> startMatchCountdownNetEvents)
        {
            if (startMatchCountdownNetEvents.IsNullOrEmpty())
            {
                return;
            }
            
            foreach (var startMatchCountdownNetEvent in startMatchCountdownNetEvents)
            {
                _startMatchButtonController.StartMatchCountdown(startMatchCountdownNetEvent.CountdownSeconds);
            }
        }

        public void ProcessStopMatchCountdownEvents(CapacityList<StopMatchCountdownNetEventS2C> stopMatchCountdownNetEvents)
        {
            if (stopMatchCountdownNetEvents.IsNullOrEmpty())
            {
                return;
            }
            
            foreach (var _ in stopMatchCountdownNetEvents)
            {
                _startMatchButtonController.StopMatchCountdown();
            }
        }

        public void ProcessStartMatchEligibleChangedEvents(CapacityList<StartMatchEligibleChangedNetEventS2C> startMatchEligibleChangedEvents)
        {
            if (startMatchEligibleChangedEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var evt in startMatchEligibleChangedEvents)
            {
                _startMatchButtonController.SetIsEnabled(evt.IsEligible);
            }
        }
    }
}