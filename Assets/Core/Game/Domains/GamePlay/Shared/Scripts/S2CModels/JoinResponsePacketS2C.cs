using System.Collections.Generic;
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
        public Dictionary<ushort, int> PlayerIdToDeviceIdDictionary;
        public bool IsSuccess;
        public MatchMakingSimulationStateS2C MatchMakingSimulationState;
        public MatchSimulationStateS2C MatchSimulationState;

        public JoinResponsePacketS2C(MaxCap maxCap, int maxTalentsPerPlayer, int maxTeams)
        {
            MatchMakingSimulationState = new MatchMakingSimulationStateS2C(maxCap.ConcurrentPlayers, maxCap.ConcurrentBullets);
            MatchSimulationState = new MatchSimulationStateS2C(maxCap.ConcurrentPlayers, maxCap.ConcurrentBullets, maxTalentsPerPlayer, maxCap.ConcurrentTalentCards, maxCap.ConcurrentPowerUpBalls, maxTeams, maxCap.ConcurrentChickenEggs, maxCap.ConcurrentGalacticForceFields);
            PlayerIdToDeviceIdDictionary = new Dictionary<ushort, int>(maxCap.ConcurrentPlayers);
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
            writer.Put((byte)PlayerIdToDeviceIdDictionary.Count);
            foreach (var kvp in PlayerIdToDeviceIdDictionary)
            {
                writer.Put((byte)kvp.Key);
                writer.Put(kvp.Value);
            }

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
            
            PlayerIdToDeviceIdDictionary.Clear();
            var playersCount = (int)reader.GetByte();
            for (int i = 0; i < playersCount; i++)
            {
                var playerId = reader.GetByte();
                var deviceId = reader.GetInt();
                PlayerIdToDeviceIdDictionary.Add(playerId, deviceId);
            }
            
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
            PlayerIdToDeviceIdDictionary.Clear();
            IsSuccess = default;
            MatchMakingSimulationState = default;
            MatchSimulationState = default;
        }
    }
}