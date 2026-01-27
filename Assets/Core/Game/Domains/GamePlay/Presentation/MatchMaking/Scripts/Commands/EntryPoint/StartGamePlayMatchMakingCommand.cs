using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Bullets;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Environment.TeamFloor.Scripts.Mvcs;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Environment.Walls.Scripts.Mvcs;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.StartMatchButton.Scripts.Mvcs;
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
        private IMatchMakingEnvironmentTeamFloorControllers _environmentTeamFloorControllers;
        private IStartMatchButtonController _startMatchButtonController;
        private IStartMatchPacketsHandler _startMatchPacketsHandler;

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
            _environmentTeamFloorControllers = _diContainer.Resolve<IMatchMakingEnvironmentTeamFloorControllers>();
            _startMatchButtonController = _diContainer.Resolve<IStartMatchButtonController>();
            _startMatchPacketsHandler = _diContainer.Resolve<IStartMatchPacketsHandler>();
        }

        public async Awaitable Execute(CancellationTokenSource cancellationTokenSource)
        {
            _startMatchPacketsHandler.InitEntryPoint();
            _fullTickPacketsHandler.InitEntryPoint();
            _playerControllers.InitEntryPoint();
            _bulletControllers.InitEntryPoint();
            _tickProcessor.InitEntryPoint();
            _environmentWallsControllers.InitEntryPoint();
            _environmentTeamFloorControllers.InitEntryPoint();
            _startMatchButtonController.InitEntryPoint();
        }
    }
}
