using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.Scripts;
using Core.Game.Domains.GamePlay.Shared.Scripts.MatchInitData;
using Core.Game.Domains.GamePlay.Shared.Scripts.Playback;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Playback
{
    public interface IPlaybackRecorderService
    {
        int Seed { get; }
        int InitialTick { get; }
        bool IsPlaybackEnabled { get; }
        EnterMatchPlayerData[] Players { get; }
        void InitEntryPoint(bool isEnabled, string playbackFileName);
        void StartRecording(int seed, EnterMatchPlayerData[] players);
        void LoadPlayback(PlaybackFile playbackFile);
        void StopRecording();
        List<RecordedPacket> GetPacketsForTick(int tick);
    }
}