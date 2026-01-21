namespace Core.Game.Domains.GamePlay.Presentation.Features.UI.Match.Scripts
{
    public class MatchUIController : IMatchUIController
    {
        private readonly MatchPlayersUIController _playersUIController;
        private readonly MatchUIView _uiView;

        public MatchUIController(MatchPlayersUIController playersUIController, MatchUIView uiView)
        {
            _playersUIController = playersUIController;
            _uiView = uiView;
        }

        public void InitEntryPoint()
        {
            if (_uiView != null)
            {
                _playersUIController.SetContainer(_uiView.PlayersContainer);
            }
        }

        public void UpdateUI()
        {
            _playersUIController.UpdateUI();
        }
    }
}