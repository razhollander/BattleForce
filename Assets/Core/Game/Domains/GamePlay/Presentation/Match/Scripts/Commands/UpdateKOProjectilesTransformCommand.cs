using Core.Game.Domains.GamePlay.Presentation.Match.Features.KOProjectiles.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands
{
    public class UpdateKOProjectilesTransformCommand : BaseCommand, ICommandVoid
    {
        private IKOProjectilesControllers _koProjectilesControllers;
        private IMatchPlayerControllers _playerControllers;
        private IMatchDataService _matchDataService;

        public override void ResolveDependencies()
        {
            _koProjectilesControllers = _diContainer.Resolve<IKOProjectilesControllers>();
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
        }

        public void Execute()
        {
            foreach (var koProjectileModel in _matchDataService.KOProjectiles)
            {
                var playerCasterPosition = _playerControllers.GetPlayerPosition(koProjectileModel.CasterPlayerId);
                _koProjectilesControllers.InterpulateKOProjectileTransform(koProjectileModel.Id, koProjectileModel.Position, koProjectileModel.Rotation.ToQuaternion(), playerCasterPosition);
            }
        }
    }
}
