using Core.Scripts.Network;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents
{
    public class PlayerJoinAcceptPacketS2C : INetSerializable
    {
        public int OccuredOnTick;
        public bool IsLocal;
        public PlayerStateS2C PlayerState;
        public MatchSimulationStateS2C SimulationState;

        public PlayerJoinAcceptPacketS2C(MaxCap maxCap, int maxTalentsPerPlayer)
        {
            PlayerState = new PlayerStateS2C(maxTalentsPerPlayer);
            SimulationState = new MatchSimulationStateS2C(maxCap.ConcurrentPlayers, maxCap.ConcurrentBullets, maxTalentsPerPlayer, maxCap.ConcurrentTalentCards, maxCap.ConcurrentPowerUpBalls);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(IsLocal);
            PlayerState.Serialize(writer);

            if (IsLocal)
            {
                SimulationState.Serialize(writer);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            IsLocal = reader.GetBool();
            PlayerState.Deserialize(reader);
            if (IsLocal)
            {
                SimulationState.Deserialize(reader);
            }
        }
    }
}