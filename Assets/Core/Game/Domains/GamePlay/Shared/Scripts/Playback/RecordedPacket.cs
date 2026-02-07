namespace Core.Game.Domains.GamePlay.Shared.Scripts.Playback
{
    [System.Serializable]
    public class RecordedPacket
    {
        public ushort PlayerId;
        public byte[] Data;
    }
}