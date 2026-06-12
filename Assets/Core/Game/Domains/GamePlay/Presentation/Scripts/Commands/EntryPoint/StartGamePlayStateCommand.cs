using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Features.UI;
using Core.Game.Domains.GamePlay.Presentation.Features.UI.ChooseNetworkRole.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Scripts.GameInputActions;
using Core.Game.Domains.GamePlay.Presentation.Scripts.InputBeingUsed;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Scripts.Utils;
using CoreDomain.GameDomain.Scripts.States.GamePlayState;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.DataPersistence;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Commands.EntryPoint
{
    public class StartGamePlayStateCommand : BaseCommand, ICommandAsync
    {
        private IGameInputActionsController _gameInputActionsController;
        private IChooseNetworkRoleUIController _chooseNetworkRoleUIController;
        private IDataPersistence _dataPersistence;
        private IJoinResponsePacketHandler _joinResponsePacketHandler;
        private IInputDeviceChangedListenerService _inputDeviceChangedListenerService;

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
            _joinResponsePacketHandler = _diContainer.Resolve<IJoinResponsePacketHandler>();
            _dataPersistence = _diContainer.Resolve<IDataPersistence>();
            _inputDeviceChangedListenerService = _diContainer.Resolve<IInputDeviceChangedListenerService>();
        }

        public async Awaitable Execute(CancellationTokenSource cancellationTokenSource)
        {
            _gameInputActionsController.EnableInputs();
            _inputDeviceChangedListenerService.InitEntryPoint();
            _chooseNetworkRoleUIController.InitEntryPoint();
            _joinResponsePacketHandler.InitEntryPoint();
        }
    }
}