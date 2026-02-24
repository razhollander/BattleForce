namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.LavaWalls.Scripts
{
    public interface IEnvironmentLavaWallsControllers
    {
        void InitEntryPoint();
        void CreateLavaWall(ushort wallId);
        void DestroyAll();
        void UpdateLavaWallTransform(ushort lavaWallId);
    }
}