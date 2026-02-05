using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Playback
{
    public interface IPlaybackRecorderService
    {
        int Seed { get; }
        int InitialTick { get; }
        bool IsPlaybackEnabled { get; }
        SimulationMatchEnterData.PlayerData[] LoadedPlayers { get; }
        void SetPlaybackInfo(bool isEnabled, string playbackFileName);
        void StartRecording(int seed, SimulationMatchEnterData.PlayerData[] players);
        void LoadRecording();
        void InitEntryPoint();
        void InitExitPoint();
        List<RecordedPacket> GetPacketsForTick(int tick);
    }
}