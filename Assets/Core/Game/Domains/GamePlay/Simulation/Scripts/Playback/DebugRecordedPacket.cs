using System;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Playback
{
    [Serializable]
    public class DebugRecordedPacket
    {
        public ushort PlayerId;
        // Depending on packet type, we might want to deserialize differently.
        // Assuming mostly PlayerInputPacketC2S for now as that's the main input.
        public string PacketData;
        // public string RawDataHex; // Optional: verify raw data
    }
}