using System;
using System.Collections.Generic;
using Core.Scripts.Network;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public struct TeamLostNetEventS2C : INetSerializable, IComparable<TeamLostNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort LosingTeamId;
        public Dictionary<ushort, int> TotalGemsPerTeam;
        public Dictionary<ushort, int> GemsGainedPerTeam;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(LosingTeamId);

            writer.Put((byte)TotalGemsPerTeam.Count);
            foreach (var kvp in TotalGemsPerTeam)
            {
                writer.Put(kvp.Key);
                writer.Put(kvp.Value);
            }

            writer.Put((byte)GemsGainedPerTeam.Count);
            foreach (var kvp in GemsGainedPerTeam)
            {
                writer.Put(kvp.Key);
                writer.Put(kvp.Value);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            LosingTeamId = reader.GetUShort();

            var gemsCount = reader.GetByte();
            TotalGemsPerTeam = new Dictionary<ushort, int>(gemsCount);
            for (int i = 0; i < gemsCount; i++)
            {
                var teamId = reader.GetUShort();
                var gems = reader.GetInt();
                TotalGemsPerTeam.Add(teamId, gems);
            }

            var gainedCount = reader.GetByte();
            GemsGainedPerTeam = new Dictionary<ushort, int>(gainedCount);
            for (int i = 0; i < gainedCount; i++)
            {
                var teamId = reader.GetUShort();
                var gained = reader.GetInt();
                GemsGainedPerTeam.Add(teamId, gained);
            }
        }

        public int CompareTo(TeamLostNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
