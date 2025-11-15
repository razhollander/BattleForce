namespace Core.Game.Domains.GamePlay.Presentation.Features.UI
{
    public class ChooseNetworkRoleUIController : IChooseNetworkRoleUIController
    {
        private readonly ChooseNetworkRoleUIView _uiView;

        public ChooseNetworkRoleUIController(ChooseNetworkRoleUIView uiView)
        {
            _uiView = uiView;
        }

        public void InitEntryPoint()
        {
            _uiView.Setup(OnClientClicked, OnHostClicked);
        }

        private void OnHostClicked()
        {
            
        }

        private void OnClientClicked()
        {
            
        }
    }
}