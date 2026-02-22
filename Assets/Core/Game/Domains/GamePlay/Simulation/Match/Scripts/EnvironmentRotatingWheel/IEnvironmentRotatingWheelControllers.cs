namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.EnvironmentRotatingWheel
{
    public interface IEnvironmentRotatingWheelControllers
    {
        void StepAllWheelsRotation(int tick, float deltaTime);
    }
}