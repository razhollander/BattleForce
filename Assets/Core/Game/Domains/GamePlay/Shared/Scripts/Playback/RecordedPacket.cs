namespace Core.Game.Domains.GamePlay.Shared.Scripts.Playback
{
    [System.Serializable]
    public class RecordedPacket
    {
        public long ClientId;
        public byte[] Data;
    }
}