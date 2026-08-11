using Core.Game.Domains.GamePlay.Presentation.Scripts.Services.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Scripts.Services.HapticsService;
using CoreDomain.Scripts.Services.CommandFactory;
using UnityEngine.InputSystem;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class PlayHapticsForPlayerCommand : BaseCommand, ICommandVoid
    {
        private ILocalPlayersDataService _localPlayersDataService;
        private IHapticsService _hapticsService;
        private PresentationGamePlayConfig _gamePlayConfig;
        private HapticType _hapticType;
        
        private ushort _playerId;

        public PlayHapticsForPlayerCommand SetPlayerId(ushort playerId)
        {
            _playerId = playerId;
            return this;
        }
        
        public PlayHapticsForPlayerCommand SetHapticProfileType(HapticType hapticType)
        {
            _hapticType = hapticType;
            return this;
        }
        
        public override void ResolveDependencies()
        {
            _localPlayersDataService = _diContainer.Resolve<ILocalPlayersDataService>();
            _hapticsService = _diContainer.Resolve<IHapticsService>();
            _gamePlayConfig = _diContainer.Resolve<PresentationGamePlayConfig>();
        }

        public void Execute()
        {
            if (!_gamePlayConfig.IsHapticsEnabled)
            {
                return;
            }

            if (_localPlayersDataService.TryGetLocalPlayerInputDevice(_playerId, out var inputDevice) && inputDevice is Gamepad gamepad)
            {
                _hapticsService.PlayHaptics(_hapticType, gamepad);
            }
        }
    }
}