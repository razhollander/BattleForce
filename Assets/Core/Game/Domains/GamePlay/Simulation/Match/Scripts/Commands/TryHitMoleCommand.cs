using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MolesSpawner;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    /// <summary>
    /// The single place a mole is whacked: every hit source (bullet, spin hit, locked on target) funnels through here.
    /// </summary>
    public class TryHitMoleCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IPhysicsSimulator _physicsSimulator;
        private INetEventsDataService _netEventsDataService;
        private ISimulationGamePlayConfigService _gamePlayConfigService;
        private IMolesSpawnCooldownService _molesSpawnCooldownService;
        private SharedGamePlayConfig _sharedGamePlayConfig;

        private ushort _moleId;
        private ushort _byPlayerId;
        private ushort _byTeamId;
        private int _processedTick;

        public TryHitMoleCommand SetMoleId(ushort moleId)
        {
            _moleId = moleId;
            return this;
        }

        public TryHitMoleCommand SetByPlayerId(ushort byPlayerId)
        {
            _byPlayerId = byPlayerId;
            return this;
        }

        public TryHitMoleCommand SetByTeamId(ushort byTeamId)
        {
            _byTeamId = byTeamId;
            return this;
        }

        public TryHitMoleCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
            _gamePlayConfigService = _diContainer.Resolve<ISimulationGamePlayConfigService>();
            _molesSpawnCooldownService = _diContainer.Resolve<IMolesSpawnCooldownService>();
            _sharedGamePlayConfig = _diContainer.Resolve<SharedGamePlayConfig>();
        }

        public void Execute()
        {
            var simulationState = _matchDataService.SimulationState;

            if (!simulationState.TryGetMoleIndexById(_moleId, out _))
            {
                LogService.LogTopic($"Mole {_moleId} was already removed in this frame!", LogTopicType.ServerPhysics);
                return;
            }

            ref var mole = ref simulationState.GetMoleById(_moleId);

            if (!mole.IsEmerged)
            {
                LogService.LogTopic($"Mole {_moleId} is still hiding in its shaking hole!", LogTopicType.ServerPhysics);
                return;
            }

            var goldenMoleDamagePerHit = _sharedGamePlayConfig.GoldenMoleDamagePerHit;

            if (mole.RemainingLives > goldenMoleDamagePerHit) // only a golden mole can survive a hit, its remaining life is shown on its health bar
            {
                mole.RemainingLives -= goldenMoleDamagePerHit;
                _netEventsDataService.AddGoldenMoleDamagedNetEvent(_processedTick, _moleId, mole.MoleHoleId, mole.RemainingLives, mole.MaxLives);
                return;
            }

            var whacAMoleConfig = _gamePlayConfigService.GamePlayConfig.WhacAMole;
            var isGolden = mole.IsGolden;
            var moleHoleId = mole.MoleHoleId;
            var score = isGolden ? whacAMoleConfig.GoldenMoleScoreOnKill : whacAMoleConfig.ScorePerMoleHit;
            simulationState.RemoveMoleById(_moleId);
            _molesSpawnCooldownService.RegisterMoleHoleToBeOnCooldown(moleHoleId, _processedTick);
            _physicsSimulator.RemoveMole(_moleId);
            simulationState.AddMolesHitForTeam(_byTeamId, score);
            var byPlayerMolesHitScoreTotal = simulationState.AddMolesHitScoreForPlayer(_byPlayerId, score);
            _netEventsDataService.AddMoleHitNetEvent(_processedTick, _moleId, moleHoleId, _byPlayerId, _byTeamId, (byte)score, simulationState.MolesHitPerTeamId[_byTeamId], byPlayerMolesHitScoreTotal, isGolden);
        }
    }
}
