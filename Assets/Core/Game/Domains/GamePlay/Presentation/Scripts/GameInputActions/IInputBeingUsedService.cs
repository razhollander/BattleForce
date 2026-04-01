namespace Core.Game.Domains.GamePlay.Presentation.Scripts.GameInputActions
{
    public interface IInputBeingUsedService
    {
        AimInputType AimInputType { get; }
        void InitEntryPoint();
        void ExitEntryPoint();
    }
}