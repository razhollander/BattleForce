using System.Threading;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken
{
    public interface IStageCancellationTokenProvider
    {
        CancellationTokenSource CancellationTokenSource { get; }
        void CancelAndRegenarateStageToken();
    }
}