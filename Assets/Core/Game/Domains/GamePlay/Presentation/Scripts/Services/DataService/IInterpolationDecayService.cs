namespace Core.Game.Domains.GamePlay.Presentation.Scripts.DataService
{
    public interface IInterpolationDecayService
    {
        float CurrentDecay { get; }
        void UpdateDecayBasedOnTicks(int ticksAdvancedSinceLastProcessedState);
    }
}
