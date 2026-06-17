using Core.Game.Domains.GamePlay.Presentation.Scripts.Services.DataService;
using Core.Scripts.Services.HapticsService;
using CoreDomain.Scripts.Services.CommandFactory;
using UnityEngine.InputSystem;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class PlayHapticsForPlayerCommand : BaseCommand, ICommandVoid
    {
        private ILocalPlayersDataService _localPlayersDataService;
        private IHapticsService _hapticsService;
        private HapticProfileType _hapticProfileType;
        
        private ushort _playerId;

        public PlayHapticsForPlayerCommand SetPlayerId(ushort playerId)
        {
            _playerId = playerId;
            return this;
        }
        
        public PlayHapticsForPlayerCommand SetHapticProfileType(HapticProfileType hapticProfileType)
        {
            _hapticProfileType = hapticProfileType;
            return this;
        }
        
        public override void ResolveDependencies()
        {
            _localPlayersDataService = _diContainer.Resolve<ILocalPlayersDataService>();
            _hapticsService = _diContainer.Resolve<IHapticsService>();
        }

        public void Execute()
        {
            if (_localPlayersDataService.TryGetLocalPlayerInputDevice(_playerId, out var inputDevice) && inputDevice is Gamepad gamepad)
            {
                _hapticsService.PlayHaptics(_hapticProfileType, gamepad);
            }
        }
    }
}