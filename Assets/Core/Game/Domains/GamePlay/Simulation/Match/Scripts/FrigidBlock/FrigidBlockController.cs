using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.FrigidBlock
{
    public class FrigidBlockController
    {
        private readonly IMatchDataService _matchDataService;
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;

        private ushort _blockId;
        private bool _hasBecomeIdle;
        private int _idleStartTick;

        public ushort BlockId => _blockId;

        public FrigidBlockController(IMatchDataService matchDataService, ISimulationGamePlayConfigService gamePlayConfigService)
        {
            _matchDataService = matchDataService;
            _gamePlayConfigService = gamePlayConfigService;
        }

        public void Init(ushort blockId)
        {
            _blockId = blockId;
            _hasBecomeIdle = false;
            _idleStartTick = 0;
        }
        
        public bool IsIdleLongEnoughToBeDestroyed(int tick, float deltaTime)
        {
            if (!_matchDataService.SimulationState.TryGetFrigidBlockById(_blockId, out var block))
            {
                return true;
            }

            var config = _gamePlayConfigService.GamePlayConfig.Talents.FrigidBlockTalentConfig;
            var isLinearIdle = block.Velocity.LengthSquared() <= config.IdleLinearVelocityThreshold;
            var isAngularIdle = System.MathF.Abs(block.AngularVelocity) <= config.IdleAngularVelocityThreshold;
            var isIdle = isLinearIdle && isAngularIdle;

            if (!isIdle)
            {
                _hasBecomeIdle = false;
                return false;
            }

            if (!_hasBecomeIdle)
            {
                _hasBecomeIdle = true;
                _idleStartTick = tick;
                return false;
            }

            var destroyTick = TickUtils.GetTickPassedAfterDuration(_idleStartTick, config.SecondsIdleUntilDestroy, deltaTime);
            var isIdleLongEnoughToBeDestroyed = tick >= destroyTick;
            return isIdleLongEnoughToBeDestroyed;
        }
    }
}
