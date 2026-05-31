namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig
{
    public interface ISimulationGamePlayConfigService
    {
        Configurations.SimulationGamePlayInnerConfig GamePlayConfig { get; }
        void OverrideGamePlayConfig(string configJson);
    }
}