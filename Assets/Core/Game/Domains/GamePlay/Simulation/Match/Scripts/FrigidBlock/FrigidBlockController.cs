using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Scripts.Network;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.FrigidBlock
{
    /// <summary>
    /// Owns the per-tick logic of a single FrigidBlock: detecting when it has come to rest
    /// (both linear and angular velocity below their idle thresholds) and signalling when it
    /// has stayed idle long enough to be destroyed. Physics (movement + deceleration) is owned
    /// by the Box2D body; this controller only reads the block's state.
    /// </summary>
    public class FrigidBlockController
    {
        private readonly IMatchDataService _matchDataService;
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private readonly NetworkConfig _networkConfig;

        private ushort _blockId;
        private bool _hasBecomeIdle;
        private int _idleStartTick;

        public ushort BlockId => _blockId;

        public FrigidBlockController(IMatchDataService matchDataService, ISimulationGamePlayConfigService gamePlayConfigService, NetworkConfig networkConfig)
        {
            _matchDataService = matchDataService;
            _gamePlayConfigService = gamePlayConfigService;
            _networkConfig = networkConfig;
        }

        public void Init(ushort blockId)
        {
            _blockId = blockId;
            _hasBecomeIdle = false;
            _idleStartTick = 0;
        }

        /// <returns>True when the block has stayed idle long enough and should be destroyed.</returns>
        public bool OnTick(int tick, float deltaTime)
        {
            if (!_matchDataService.SimulationState.TryGetFrigidBlockById(_blockId, out var block))
            {
                return true;
            }

            var config = _gamePlayConfigService.GamePlayConfig.Talents.FrigidBlockTalentConfig;
            var isLinearIdle = block.Velocity.LengthSquared() <= config.IdleLinearVelocityThreshold * config.IdleLinearVelocityThreshold;
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

            var destroyTick = TickUtils.GetTickPassedAfterDuration(_idleStartTick, config.SecondsIdleUntilDestroy, _networkConfig.DeltaTime);
            return tick >= destroyTick;
        }
    }
}
