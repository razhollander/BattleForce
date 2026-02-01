using System.Collections.Generic;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public class StageEndNetEventS2C : INetSerializable
    {
        public int OccuredOnTick;
        public ushort WinningTeamId;
        public Dictionary<ushort, int> JemsWonPerTeam = new Dictionary<ushort, int>();
        public Dictionary<ushort, int> TotalJemsPerTeam = new Dictionary<ushort, int>();

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(WinningTeamId);

            writer.Put((ushort)JemsWonPerTeam.Count);
            foreach (var kvp in JemsWonPerTeam)
            {
                writer.Put(kvp.Key);
                writer.Put(kvp.Value);
            }

            writer.Put((ushort)TotalJemsPerTeam.Count);
            foreach (var kvp in TotalJemsPerTeam)
            {
                writer.Put(kvp.Key);
                writer.Put(kvp.Value);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            WinningTeamId = reader.GetUShort();

            JemsWonPerTeam.Clear();
            var jemsWonCount = reader.GetUShort();
            for (int i = 0; i < jemsWonCount; i++)
            {
                var teamId = reader.GetUShort();
                var jems = reader.GetInt();
                JemsWonPerTeam.Add(teamId, jems);
            }

            TotalJemsPerTeam.Clear();
            var totalJemsCount = reader.GetUShort();
            for (int i = 0; i < totalJemsCount; i++)
            {
                var teamId = reader.GetUShort();
                var jems = reader.GetInt();
                TotalJemsPerTeam.Add(teamId, jems);
            }
        }
    }
}
