using System;
using System.Collections.Generic;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public class StageEndNetEventS2C : INetSerializable, IComparable<StageEndNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort WinningTeamId;
        public Dictionary<ushort, int> JemsWonPerTeam;
        public Dictionary<ushort, int> TotalJemsPerTeam;

        public StageEndNetEventS2C(int maxTeamsAmount)
        {
            JemsWonPerTeam = new Dictionary<ushort, int>(maxTeamsAmount);
            TotalJemsPerTeam = new Dictionary<ushort, int>(maxTeamsAmount);
        }
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)WinningTeamId);

            writer.Put((byte)JemsWonPerTeam.Count);
            foreach (var kvp in JemsWonPerTeam)
            {
                writer.Put((byte)kvp.Key);
                writer.Put((byte)kvp.Value);
            }

            writer.Put((byte)TotalJemsPerTeam.Count);
            foreach (var kvp in TotalJemsPerTeam)
            {
                writer.Put((byte)kvp.Key);
                writer.Put((byte)kvp.Value);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            WinningTeamId = reader.GetByte();

            JemsWonPerTeam.Clear();
            var jemsWonCount = reader.GetByte();
            for (int i = 0; i < jemsWonCount; i++)
            {
                var teamId = reader.GetByte();
                var jems = reader.GetByte();
                JemsWonPerTeam.Add(teamId, jems);
            }

            TotalJemsPerTeam.Clear();
            var totalJemsCount = reader.GetByte();
            for (int i = 0; i < totalJemsCount; i++)
            {
                var teamId = reader.GetByte();
                var jems = reader.GetByte();
                TotalJemsPerTeam.Add(teamId, jems);
            }
        }

        public int CompareTo(StageEndNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
