namespace Core.Game.Domains.GamePlay.Presentation.Features.Environment.Walls
{
    public interface IEnvironmentLavaWallsControllers
    {
        void InitEntryPoint();
        void CreateLavaWall(ushort wallId);
    }
}