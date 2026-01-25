namespace Core.Game.Domains.GamePlay.Presentation.Features.Environment.Walls
{
    public interface IMatchEnvironmentWallsControllers
    {
        void InitEntryPoint();
        void CreateWall(ushort wallId);
    }
}