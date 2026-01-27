namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.States
{
    public interface IState
    {
        void Enter(object enterData);
        void Exit();
    }
}
