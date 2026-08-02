using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Network;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    public class StartStagePacketS2C: INetSerializable
    {
        public int OccuredOnTick;
        public MatchSimulationStateS2C InitialState;

        public StartStagePacketS2C()
        {
        }
        
        public StartStagePacketS2C(MaxCap maxCap, int maxTalentsPerPlayer, int maxTeams)
        {
            InitialState = new MatchSimulationStateS2C(maxCap.ConcurrentPlayers, maxCap.ConcurrentBullets, maxTalentsPerPlayer, maxCap.ConcurrentTalentCards, maxCap.ConcurrentPowerUpBalls, maxTeams, maxCap.ConcurrentChickenEggs, maxCap.ConcurrentGalacticForceFields, maxCap.ConcurrentFrigidBlocks, maxCap.ConcurrentMoles);
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