namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Spikes.Scripts.Mvc
{
    public interface IEnvironmentSpikeControllers
    {
        void InitEntryPoint();
        void CreateSpike(ushort spikeId);
        void DestroyAll();
        void PlaySpikeHitAnimation(ushort spikeId);
        void UpdateSpikeTransform(ushort spikeId);
    }
}
