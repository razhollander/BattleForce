using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;
using Core.Scripts.Network;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    public class JoinResponsePacketS2C : INetSerializable
    {
        public int OccuredOnTick;
        public bool IsMatchMaking;
        public ushort LocalPlayerId;
        public bool IsSuccess;
        public MatchMakingSimulationStateS2C MatchMakingSimulationState;
        public MatchSimulationStateS2C MatchSimulationState;

        public JoinResponsePacketS2C(MaxCap maxCap, int maxTalentsPerPlayer)
        {
            MatchMakingSimulationState = new MatchMakingSimulationStateS2C(maxCap.ConcurrentPlayers, maxCap.ConcurrentBullets);
            MatchSimulationState = new MatchSimulationStateS2C(maxCap.ConcurrentPlayers, maxCap.ConcurrentBullets, maxTalentsPerPlayer, maxCap.ConcurrentTalentCards, maxCap.ConcurrentPowerUpBalls);
        }
        
        public JoinResponsePacketS2C()
        {
        }
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(IsSuccess);
            if (!IsSuccess)
            {
                return;
            }

            writer.Put(IsMatchMaking);
            writer.Put(LocalPlayerId);

            if (IsMatchMaking)
            {
                MatchMakingSimulationState.Serialize(writer);
            }
            else
            {
                MatchSimulationState.Serialize(writer);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            IsSuccess = reader.GetBool();
            if (!IsSuccess)
            {
                return;
            }
            
            IsMatchMaking = reader.GetBool();
            LocalPlayerId = reader.GetUShort();
            
            if (IsMatchMaking)
            {
                MatchMakingSimulationState.Deserialize(reader);
            }
            else
            {
                MatchSimulationState.Deserialize(reader);
            }
        }

        public void Clear()
        {
            OccuredOnTick = default;
            IsMatchMaking = default;
            LocalPlayerId = default;
            IsSuccess = default;
            MatchMakingSimulationState = default;
            MatchSimulationState = default;
        }
    }
}