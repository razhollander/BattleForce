using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Initiator;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.EntryPoint
{
    public class LoadGamePlayMatchCommand : BaseCommand, ICommandAsync
    {
        private IAudioService _audioService;
        private ICommandFactory _commandFactory;
        private GamePlayMatchInitiatorEnterData _enterData;

        public LoadGamePlayMatchCommand SetEnterData(GamePlayMatchInitiatorEnterData enterData)
        {
            _enterData = enterData;
            return this;
        }
        
        public override void ResolveDependencies()
        {
            _audioService = _diContainer.Resolve<IAudioService>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
        }
        
        public async Awaitable Execute(CancellationTokenSource cancellationTokenSource)
        {
//            _audioService.AddAudioClips(_gamePlayAudioClipsScriptableObject);
            //          await _commandFactory.CreateCommandAsync<LoadLevelCommand>().SetEnterData(new LoadLevelCommandData(_enterData.LevelNumberToEnter)).Execute(cancellationTokenSource);
        }
    }
}