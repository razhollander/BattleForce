using Core.Game.Domains.GamePlay.Presentation.Match.Features.KOProjectiles.Scripts.Mvc;
using CoreDomain.Scripts.Services.CommandFactory;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands
{
    public class UpdateKOProjectilesTransformCommand : BaseCommand, ICommandVoid
    {
        private DiContainer _diContainer;
        private IKOProjectilesControllers _koProjectilesControllers;

        [Inject]
        public void Construct(DiContainer diContainer)
        {
            _diContainer = diContainer;
            _koProjectilesControllers = _diContainer.Resolve<IKOProjectilesControllers>();
        }

        public void Execute()
        {
            _koProjectilesControllers.UpdateKOProjectilesTransform();
        }
    }
}
