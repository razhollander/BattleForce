using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Playback;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Playback
{
    public interface IPlaybackRecorderService
    {
        int Seed { get; }
        int InitialTick { get; }
        bool IsPlaybackEnabled { get; }
        MatchSimulationStateS2C InitialSimulationState { get; }
        SimulationMatchEnterData.PlayerData[] LoadedPlayers { get; }
        void InitEntryPoint(bool isEnabled, string playbackFileName);
        void StartRecording(int seed, MatchSimulationStateS2C initialSimulationState);
        void LoadPlayback(PlaybackFile playbackFile);
        void StopRecording();
        List<RecordedPacket> GetPacketsForTick(int tick);
    }
}