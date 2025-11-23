using System.Threading;
using System.Threading.Tasks;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Simulation.Scripts.ContextInstaller;
using CoreDomain.Scripts.Services.SceneService;
using CoreDomain.Scripts.Services.StateMachineService;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.UI
{
    public class ChooseNetworkRoleUIController : IChooseNetworkRoleUIController
    {
        private readonly ChooseNetworkRoleUIView _uiView;
        private readonly ISceneLoaderService _sceneLoaderService;
        private readonly IStateMachineService _stateMachineService;
        private readonly IClientNetworkManager _clientNetworkManager;

        public ChooseNetworkRoleUIController(ChooseNetworkRoleUIView uiView, ISceneLoaderService sceneLoaderService, IStateMachineService stateMachineService, IClientNetworkManager clientNetworkManager)
        {
            _uiView = uiView;
            _sceneLoaderService = sceneLoaderService;
            _stateMachineService = stateMachineService;
            _clientNetworkManager = clientNetworkManager;
        }

        public void InitEntryPoint()
        {
            _uiView.Setup(OnClientClicked, OnHostClicked);
        }

        private void OnHostClicked()
        {
            _ = OnHostClickedAsync();
        }

        private async Awaitable OnHostClickedAsync()
        {
            var enterData = new ServerInitiatorEnterData();
            var cancellationTokenSource = _stateMachineService.CurrentState().CancellationTokenSource;
            await StartServer(enterData, cancellationTokenSource);
            StartClient();
        }

        private async Awaitable StartServer(ServerInitiatorEnterData enterData, CancellationTokenSource cancellationTokenSource)
        {
            await _sceneLoaderService.TryLoadScene(SceneType.ServerScene, enterData, cancellationTokenSource);
            await _sceneLoaderService.StartScene(SceneType.ServerScene, enterData, cancellationTokenSource);
        }

        private void StartClient()
        {
            _clientNetworkManager.StartClient();
        }

        private void OnClientClicked()
        {
            StartClient();
        }
    }
}