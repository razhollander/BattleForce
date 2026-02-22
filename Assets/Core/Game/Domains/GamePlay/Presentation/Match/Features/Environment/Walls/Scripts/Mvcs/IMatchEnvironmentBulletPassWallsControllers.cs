namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Walls.Scripts.Mvcs
{
    public interface IMatchEnvironmentBulletPassWallsControllers
    {
        void InitEntryPoint();
        void CreateBulletPassWall(ushort wallId);
        void DestroyAll();
    }
}
