namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Controllers
{
    public interface IStartMatchButtonController
    {
        void InitEntryPoint();
        void StartMatchCountdown(float duration);
        void StopMatchCountdown();
    }
}