namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.StartMatchButton.Scripts.Mvcs
{
    public interface IStartMatchButtonController
    {
        void InitEntryPoint();
        void StartMatchCountdown(float duration);
        void StopMatchCountdown();
    }
}