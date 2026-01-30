namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Playback
{
    [System.Serializable]
    public class RecordedPacket
    {
        public ushort PlayerId;
        public byte[] Data;
    }
}