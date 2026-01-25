using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Features.UI;
using Core.Game.Domains.GamePlay.Presentation.Features.UI.ChooseNetworkRole.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Scripts.GameInputActions;
using CoreDomain.GameDomain.Scripts.States.GamePlayState;
using CoreDomain.Scripts.Services.CommandFactory;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Commands.EntryPoint
{
    public class StartGamePlayStateCommand : BaseCommand, ICommandAsync
    {
        private IGameInputActionsController _gameInputActionsController;
        private IChooseNetworkRoleUIController _chooseNetworkRoleUIController;

        private GamePlayInitiatorEnterData _enterData; // kept this for future use

        public StartGamePlayStateCommand SetEnterData(GamePlayInitiatorEnterData enterData)
        {
            _enterData = enterData;
            return this;
        }
        
        public override void ResolveDependencies()
        {
            _gameInputActionsController = _diContainer.Resolve<IGameInputActionsController>();
            _chooseNetworkRoleUIController = _diContainer.Resolve<IChooseNetworkRoleUIController>();
        
        }

        public async Awaitable Execute(CancellationTokenSource cancellationTokenSource)
        {
            _gameInputActionsController.EnableInputs();
            _chooseNetworkRoleUIController.InitEntryPoint();
        }
    }
}