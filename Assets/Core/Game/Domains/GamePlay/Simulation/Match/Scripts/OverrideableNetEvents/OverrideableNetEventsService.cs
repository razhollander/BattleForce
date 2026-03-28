using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.OverrideableNetEvents
{
    public class OverrideableNetEventsService : IOverrideableNetEventsService
    {
        private readonly INetEventsDataService _netEventsDataService;
        private FixedUnorderedList<UpdatePlayerTalentStocksNetEventS2C> _updatePlayerTalentStocksNetEvents;

        public OverrideableNetEventsService(INetEventsDataService netEventsDataService, NetworkConfig networkConfig, SharedGamePlayConfig sharedGamePlayConfig)
        {
            _netEventsDataService = netEventsDataService;
            _updatePlayerTalentStocksNetEvents = new FixedUnorderedList<UpdatePlayerTalentStocksNetEventS2C>(networkConfig.MaxCap.ConcurrentPlayers * sharedGamePlayConfig.MaxConcurrentTalentsForPlayer);
        }

        public void RegisterAllOverridableNetEvents()
        {
            ProcessUpdatePlayerTalentStocksNetEvents();
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
                netEvent.CasterPlayerId = casterPlayerId;
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
    }
}