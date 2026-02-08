using CoreDomain.Scripts.CoreInitiator.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.ContextInstaller
{
    public class ServerInitiatorEnterData : IInitiatorEnterData
    {
        public readonly bool IsPlaybackEnabled;
        public readonly string PlaybackFileName;

        public ServerInitiatorEnterData(bool isPlaybackEnabled, string playbackFileName)
        {
            IsPlaybackEnabled = isPlaybackEnabled;
            PlaybackFileName = playbackFileName;
        }
    }
}