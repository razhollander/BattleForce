namespace Core.Game.Domains.GamePlay.Presentation.Scripts.InputBeingUsed
{
    public interface IInputBeingUsedService
    {
        SupportedInputType InputTypeBeingUsed { get; }
        void InitEntryPoint();
        void InitExitPoint();
    }
}