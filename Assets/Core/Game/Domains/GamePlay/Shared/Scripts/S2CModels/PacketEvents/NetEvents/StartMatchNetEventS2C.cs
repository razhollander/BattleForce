using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Scripts.Network;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public class StartMatchNetEventS2C : INetSerializable
    {
        public int OccuredOnTick;
        public MatchSimulationStateS2C InitialState;

        private MaxCap _maxCap;
        private SharedGamePlayConfig _sharedGamePlayConfig;

        public StartMatchNetEventS2C()
        {
        }

        public StartMatchNetEventS2C(MaxCap maxCap, SharedGamePlayConfig sharedGamePlayConfig)
        {
            _maxCap = maxCap;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            InitialState = new MatchSimulationStateS2C(maxCap.ConcurrentPlayers, maxCap.ConcurrentBullets, sharedGamePlayConfig.MaxConcurrentTalentsForPlayer, maxCap.ConcurrentTalentCards, maxCap.ConcurrentPowerUpBalls);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            InitialState.Serialize(writer);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            InitialState.Deserialize(reader);
        }
    }
}
