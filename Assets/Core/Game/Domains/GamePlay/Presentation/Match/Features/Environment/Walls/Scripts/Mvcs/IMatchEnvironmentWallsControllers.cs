namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Walls.Scripts.Mvcs
{
    public interface IMatchEnvironmentWallsControllers
    {
        void InitEntryPoint();
        void CreateWall(ushort wallId);
        void DestroyAll();
    }
}