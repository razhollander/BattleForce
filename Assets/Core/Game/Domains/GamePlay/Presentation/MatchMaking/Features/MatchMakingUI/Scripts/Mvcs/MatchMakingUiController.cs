using Core.Scripts.Utils;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.MatchMakingUI.Scripts.Mvcs
{
    public class MatchMakingUiController : IMatchMakingUiController
    {
        private readonly MatchMakingUiView _view;

        public MatchMakingUiController(MatchMakingUiView view)
        {
            _view = view;
        }

        public void InitEntryPoint(string ipAddress, int port, bool isHost)
        {
            _view.Setup(isHost ? NetworkUtils.GetLocalIPAddress() : ipAddress, port.ToString());
        }
    }
}