namespace Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.StartMatchWall
{
    public interface IStartMatchWallController
    {
        void Initialize(float radius);
        void TryToggleCountdownState(int tick);
        void TryStopCountdown(int tick);
        void StepTimer(float deltaTime);
        bool DidFinishCountingDown { get; }
    }
}