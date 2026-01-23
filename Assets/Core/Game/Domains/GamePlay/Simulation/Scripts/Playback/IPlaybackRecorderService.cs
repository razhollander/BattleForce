using System.Collections.Generic;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Playback
{
    public interface IPlaybackRecorderService
    {
        int Seed { get; }
        bool IsPlaybackEnabled { get; }
        void StartRecording(int seed);
        void RecordPacket(ushort playerId, byte[] data);
        void SaveRecording();
        void LoadRecording();
        void InitEntryPoint();
        List<RecordedPacket> GetPacketsForTick(int tick);
    }
}