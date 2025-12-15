using System.Threading;
using System.Threading.Tasks;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Presentation;
using Core.Game.Domains.GamePlay.Simulation.NetworkManager.PacketsHandlers;
using Core.Game.Domains.GamePlay.Simulation.Scripts.ContextInstaller;
using CoreDomain.Scripts.Services.Logger.Base;
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
        private readonly IFullTickPacketsHandler _fullTickPacketsHandler;
        private readonly IClientPresentationTickProcessor _clientPresentationTickProcessor;

        public ChooseNetworkRoleUIController(ChooseNetworkRoleUIView uiView, ISceneLoaderService sceneLoaderService,
            IStateMachineService stateMachineService, IClientNetworkManager clientNetworkManager, IFullTickPacketsHandler fullTickPacketsHandler, IClientPresentationTickProcessor clientPresentationTickProcessor)
        {
            _uiView = uiView;
            _sceneLoaderService = sceneLoaderService;
            _stateMachineService = stateMachineService;
            _clientNetworkManager = clientNetworkManager;
            _fullTickPacketsHandler = fullTickPacketsHandler;
            _clientPresentationTickProcessor = clientPresentationTickProcessor;
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
            await Awaitable.WaitForSecondsAsync(2, cancellationToken:cancellationTokenSource.Token);
            StartClient();
        }

        private async Awaitable StartServer(ServerInitiatorEnterData enterData, CancellationTokenSource cancellationTokenSource)
        {
            LogService.LogTopic("Starting Server", LogTopicType.ClientNetwork);
            await _sceneLoaderService.TryLoadScene(SceneType.ServerScene, enterData, cancellationTokenSource);
            await _sceneLoaderService.StartScene(SceneType.ServerScene, enterData, cancellationTokenSource);
            LogService.LogTopic("Finished starting Server", LogTopicType.ClientNetwork);
        }

        private void StartClient()
        {
            LogService.LogTopic("Starting Client", LogTopicType.ClientNetwork);
            _clientNetworkManager.StartClient();
            _fullTickPacketsHandler.RegisterListeners();
            _uiView.Hide();
            LogService.LogTopic("Finished starting Client", LogTopicType.ClientNetwork);
        }

        private void OnClientClicked()
        {
            StartClient();
        }
    }
}