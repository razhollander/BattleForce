using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.OverrideableNetEvents
{
    /// <summary>
    /// Net Events that we want to be sent only once per tick are registered here
    /// </summary>
    public class OverrideableNetEventsService : IOverrideableNetEventsService
    {
        private readonly INetEventsDataService _netEventsDataService;
        private FixedUnorderedList<UpdatePlayerTalentStocksNetEventS2C> _updatePlayerTalentStocksNetEvents;
        private FixedUnorderedList<PlayerMaxShootCooldownChangedNetEventS2C> _playerMaxShootCooldownChangedNetEvents;

        public OverrideableNetEventsService(INetEventsDataService netEventsDataService, NetworkConfig networkConfig, SharedGamePlayConfig sharedGamePlayConfig)
        {
            _netEventsDataService = netEventsDataService;
            _updatePlayerTalentStocksNetEvents = new FixedUnorderedList<UpdatePlayerTalentStocksNetEventS2C>(networkConfig.MaxCap.ConcurrentPlayers * sharedGamePlayConfig.MaxConcurrentTalentsForPlayer);
            _playerMaxShootCooldownChangedNetEvents = new FixedUnorderedList<PlayerMaxShootCooldownChangedNetEventS2C>(networkConfig.MaxCap.ConcurrentPlayers);
        }

        public void RegisterAllOverridableNetEvents()
        {
            ProcessUpdatePlayerTalentStocksNetEvents();
            ProcessPlayerMaxShootCooldownChangedNetEvents();
        }

        private void ProcessPlayerMaxShootCooldownChangedNetEvents()
        {
            foreach (var netEvent in _playerMaxShootCooldownChangedNetEvents.AsSpan())
            {
                _netEventsDataService.AddPlayerMaxShootCooldownChangedNetEvent(netEvent.OccuredOnTick, netEvent.PlayerId, netEvent.MaxShootCooldown, netEvent.ShootCooldownSecondsLeft);
            }

            _playerMaxShootCooldownChangedNetEvents.Clear();
        }

        private void ProcessUpdatePlayerTalentStocksNetEvents()
        {
            foreach (var netEvent in _updatePlayerTalentStocksNetEvents.AsSpan())
            {
                _netEventsDataService.AddUpdatePlayerTalentStocksNetEventS2C(netEvent.OccuredOnTick, netEvent.CasterPlayerId, netEvent.TalentType, netEvent.CurrentStocksAmount, netEvent.RecieveNextStockOnTick);
            }

            _updatePlayerTalentStocksNetEvents.Clear();
        }

        public void OverrideUpdateTalentStockEvent(int onTick, ushort casterPlayerId, TalentType talentType, int currentStocksAmount, int recieveNextStockOnTick)
        {
            for (int i = 0; i < _updatePlayerTalentStocksNetEvents.Count; i++)
            {
                var updatePlayerTalentStocksNetEvent = _updatePlayerTalentStocksNetEvents[i];
                var doesEventAlreadyExist = updatePlayerTalentStocksNetEvent.CasterPlayerId == casterPlayerId
                                            && updatePlayerTalentStocksNetEvent.TalentType == talentType;

                if (!doesEventAlreadyExist)
                {
                    continue;
                }

                ref var netEvent = ref _updatePlayerTalentStocksNetEvents.GetByIndex(i);
                netEvent.OccuredOnTick = onTick;
                netEvent.TalentType = talentType;
                netEvent.CurrentStocksAmount = currentStocksAmount;
                netEvent.RecieveNextStockOnTick = recieveNextStockOnTick;
                return;
            }

            ref var packet = ref _updatePlayerTalentStocksNetEvents.AddAndGet();
            packet.OccuredOnTick = onTick;
            packet.CasterPlayerId = casterPlayerId;
            packet.TalentType = talentType;
            packet.CurrentStocksAmount = currentStocksAmount;
            packet.RecieveNextStockOnTick = recieveNextStockOnTick;
        }

        public void OverridePlayerMaxShootCooldownChangedEvent(int onTick, ushort playerId, float maxShootCooldown, float cooldownSecondsLeft)
        {
            for (int i = 0; i < _playerMaxShootCooldownChangedNetEvents.Count; i++)
            {
                var evt = _playerMaxShootCooldownChangedNetEvents[i];
                if (evt.PlayerId != playerId)
                {
                    continue;
                }

                ref var netEvent = ref _playerMaxShootCooldownChangedNetEvents.GetByIndex(i);
                netEvent.OccuredOnTick = onTick;
                netEvent.MaxShootCooldown = maxShootCooldown;
                netEvent.ShootCooldownSecondsLeft = cooldownSecondsLeft;
                return;
            }

            ref var packet = ref _playerMaxShootCooldownChangedNetEvents.AddAndGet();
            packet.PlayerId = playerId;
            packet.OccuredOnTick = onTick;
            packet.MaxShootCooldown = maxShootCooldown;
            packet.ShootCooldownSecondsLeft = cooldownSecondsLeft;
        }
    }
}