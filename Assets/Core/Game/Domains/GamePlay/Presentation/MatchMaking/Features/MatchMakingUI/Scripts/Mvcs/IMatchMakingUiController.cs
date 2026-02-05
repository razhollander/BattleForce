namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.MatchMakingUI.Scripts.Mvcs
{
    public interface IMatchMakingUiController
    {
        void InitEntryPoint(string ipAddress, int port, bool isHost);
    }
}