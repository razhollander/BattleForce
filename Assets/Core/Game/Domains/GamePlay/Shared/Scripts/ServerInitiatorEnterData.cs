using CoreDomain.Scripts.CoreInitiator.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.ContextInstaller
{
    public class ServerInitiatorEnterData : IInitiatorEnterData
    {
        public readonly bool IsPlaybackEnabled;
        public readonly string PlaybackFileName;
        public readonly int Port;

        public ServerInitiatorEnterData(bool isPlaybackEnabled, string playbackFileName, int port)
        {
            IsPlaybackEnabled = isPlaybackEnabled;
            PlaybackFileName = playbackFileName;
            Port = port;
        }
    }
}