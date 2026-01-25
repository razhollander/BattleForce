namespace Core.Game.Domains.GamePlay.Presentation.Features.Environment.Walls
{
    public interface IMatchMakingEnvironmentWallsControllers
    {
        void InitEntryPoint();
        void CreateWall(ushort wallId);
    }
}