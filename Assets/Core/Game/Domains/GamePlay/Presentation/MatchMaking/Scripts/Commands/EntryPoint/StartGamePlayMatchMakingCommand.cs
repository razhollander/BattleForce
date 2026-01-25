using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Bullets;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Environment.Walls.Scripts.Mvcs;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Initiator;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.Scripts.TickProcessors;
using CoreDomain.Scripts.Services.CommandFactory;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Commands.EntryPoint
{
    public class StartGamePlayMatchMakingCommand : BaseCommand, ICommandAsync
    {
        private GamePlayMatchMakingInitiatorEnterData _enterData;
        private IMatchMakingPlayerControllers _playerControllers;
        private IMatchMakingBulletControllers _bulletControllers;
        private ITickProcessor _tickProcessor;
        private IMatchMakingEnvironmentWallsControllers _environmentWallsControllers;
        private IFullTickPacketsHandler _fullTickPacketsHandler;

        public StartGamePlayMatchMakingCommand SetEnterData(GamePlayMatchMakingInitiatorEnterData enterData)
        {
            _enterData = enterData;
            return this;
        }

        public override void ResolveDependencies()
        {
            _playerControllers = _diContainer.Resolve<IMatchMakingPlayerControllers>();
            _bulletControllers = _diContainer.Resolve<IMatchMakingBulletControllers>();
            _tickProcessor = _diContainer.Resolve<ITickProcessor>();
            _environmentWallsControllers = _diContainer.Resolve<IMatchMakingEnvironmentWallsControllers>();
            _fullTickPacketsHandler = _diContainer.Resolve<IFullTickPacketsHandler>();
        }

        public async Awaitable Execute(CancellationTokenSource cancellationTokenSource)
        {
            _fullTickPacketsHandler.InitEntryPoint();
            _playerControllers.InitEntryPoint();
            _bulletControllers.InitEntryPoint();
            _tickProcessor.InitEntryPoint();
            _environmentWallsControllers.InitEntryPoint();
        }
    }
}
