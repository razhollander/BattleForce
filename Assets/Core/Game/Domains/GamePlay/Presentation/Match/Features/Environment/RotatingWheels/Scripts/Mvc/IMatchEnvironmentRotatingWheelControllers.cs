namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.RotatingWheels.Scripts.Mvc
{
    public interface IMatchEnvironmentRotatingWheelControllers
    {
        void InitEntryPoint();
        void CreateRotatingWheels();
        void UpdateRotation();
        void DestroyAll();
    }
}