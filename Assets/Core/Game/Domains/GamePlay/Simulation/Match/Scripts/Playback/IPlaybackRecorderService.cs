using System.Collections.Generic;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Playback
{
    public interface IPlaybackRecorderService
    {
        int Seed { get; }
        int InitialTick { get; }
        bool IsPlaybackEnabled { get; }
        void StartRecording(int seed);
        void LoadRecording();
        void InitEntryPoint();
        void InitExitPoint();
        List<RecordedPacket> GetPacketsForTick(int tick);
    }
}