using CoreDomain.Scripts.CoreInitiator.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.ContextInstaller
{
    public class ServerInitiatorEnterData : IInitiatorEnterData
    {
        public bool IsPlaybackEnabled;
        public string PlaybackFileName;

        public ServerInitiatorEnterData(bool isPlaybackEnabled = false, string playbackFileName = "")
        {
            IsPlaybackEnabled = isPlaybackEnabled;
            PlaybackFileName = playbackFileName;
        }
    }
}