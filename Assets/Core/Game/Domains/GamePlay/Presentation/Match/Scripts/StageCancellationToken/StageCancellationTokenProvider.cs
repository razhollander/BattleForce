using System.Threading;
using CoreDomain.Scripts.Services.StateMachineService;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken
{
    public class StageCancellationTokenProvider : IStageCancellationTokenProvider
    {
        private readonly IStateMachineService _stateMachineService;
        private CancellationTokenSource _cancellationTokenSource;

        public CancellationTokenSource CancellationTokenSource => CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token);

        public StageCancellationTokenProvider(IStateMachineService stateMachineService)
        {
            _stateMachineService = stateMachineService;
            _cancellationTokenSource = _stateMachineService.CurrentState().CancellationTokenSource;
        }

        public void CancelAndRegenarateStageToken()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = _stateMachineService.CurrentState().CancellationTokenSource;
        }
    }
}