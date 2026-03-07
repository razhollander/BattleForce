namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.FieldBarriers.Scripts
{
    public interface IEnvironmentFieldBarrierControllers
    {
        void CreateFieldBarrier(ushort id);
        void DestroyAll();
        void HideAll();
    }
}
