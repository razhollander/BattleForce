using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.OverrideableNetEvents;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent.TalentController
{
    public class SentryGunTalentController : ITalentController
    {
        private readonly IOverrideableNetEventsService _overrideableNetEventsService;
        private readonly IMatchDataService _matchDataService;
        private ushort _casterPlayerId;
        private bool _isActive;
        private bool _wasInputPressedPreviousTick;
        private float _cachedOriginalMaxCooldown;

        public SentryGunTalentController(IOverrideableNetEventsService overrideableNetEventsService, IMatchDataService matchDataService)
        {
            _overrideableNetEventsService = overrideableNetEventsService;
            _matchDataService = matchDataService;
        }

        public void SetCasterId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
        }

        public TalentType TalentType => TalentType.SentryGun;
        public bool IsCurrentlyActive => _isActive;
        public void ProcessTalentInput(bool isTalentInputPressed, int tick, float deltaTime)
        {
            if (isTalentInputPressed && !_wasInputPressedPreviousTick)
            {
                _isActive = !_isActive;
                var player = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
                var shoot = player.Spaceship.Shoot;

                if (_isActive)
                {
                    _cachedOriginalMaxCooldown = shoot.MaxCooldown;
                    shoot.MaxCooldown = 0.5f; // simulated change
                }
                else
                {
                    shoot.MaxCooldown = _cachedOriginalMaxCooldown > 0 ? _cachedOriginalMaxCooldown : 1.5f;
                }

                player.Spaceship.Shoot = shoot;
                _overrideableNetEventsService.OverridePlayerMaxShootCooldownChangedEvent(tick, _casterPlayerId, shoot.MaxCooldown);
            }
            
            _wasInputPressedPreviousTick = isTalentInputPressed;
        }

        public void StopIfActive(int tick)
        {
            if (!_isActive) return;

            _isActive = false;
            var player = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
            var shoot = player.Spaceship.Shoot;
            shoot.MaxCooldown = _cachedOriginalMaxCooldown > 0 ? _cachedOriginalMaxCooldown : 1.5f;
            player.Spaceship.Shoot = shoot;
            _overrideableNetEventsService.OverridePlayerMaxShootCooldownChangedEvent(tick, _casterPlayerId, shoot.MaxCooldown);
        }

        public void OnTick(int tick, float deltaTime)
        {
            
        }

        public void ResetData()
        {
            
        }
    }
}