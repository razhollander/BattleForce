using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts;
using CoreDomain.Scripts.Services.UpdateService;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Presentation
{
    public class ClientPresentationTickProcessor : IUpdatable, IClientPresentationTickProcessor
    {
        private readonly IUpdateSubscriptionService _updateSubscriptionService;
        private readonly IPlayerControllers _playerControllers;

        public ClientPresentationTickProcessor(IUpdateSubscriptionService updateSubscriptionService, IPlayerControllers playerControllers)
        {
            _updateSubscriptionService = updateSubscriptionService;
            _playerControllers = playerControllers;
        }
        
        public void StartTick()
        {
            _updateSubscriptionService.RegisterUpdatable(this);
        }
        
        public void StopTick()
        {
            _updateSubscriptionService.UnregisterUpdatable(this);
        }

        public void ManagedUpdate()
        {
            _playerControllers.UpdatePlayersTransform();
        }
    }
}