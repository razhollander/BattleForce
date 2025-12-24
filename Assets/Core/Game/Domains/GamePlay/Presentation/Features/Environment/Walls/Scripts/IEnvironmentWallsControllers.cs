namespace Core.Game.Domains.GamePlay.Presentation.Features.Environment.Walls
{
    public interface IEnvironmentWallsControllers
    {
        void InitEntryPoint();
        void CreateWall(ushort wallId);
    }
}