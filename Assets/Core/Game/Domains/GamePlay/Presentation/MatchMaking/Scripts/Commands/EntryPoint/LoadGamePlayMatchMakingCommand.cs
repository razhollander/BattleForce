using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Initiator;
using CoreDomain.Scripts.Services.CommandFactory;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Commands.EntryPoint
{
    public class LoadGamePlayMatchMakingCommand : BaseCommand, ICommandAsync
    {
        private GamePlayMatchMakingInitiatorEnterData _enterData;

        public LoadGamePlayMatchMakingCommand SetEnterData(GamePlayMatchMakingInitiatorEnterData enterData)
        {
            _enterData = enterData;
            return this;
        }
        
        public override void ResolveDependencies()
        {
        }

        public async Awaitable Execute(CancellationTokenSource cancellationTokenSource)
        {
        }
    }
}
