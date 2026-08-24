using Core.Game.Domains.GamePlay.Shared.S2CModels;
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

            if (mole.IsGolden)
            {
                var goldenMoleDamagePerHit = _sharedGamePlayConfig.GoldenMoleDamagePerHit;
                var isGoldenMoleAliveAfterHit = mole.RemainingLives > goldenMoleDamagePerHit;

                if (isGoldenMoleAliveAfterHit)
                {
                    DamageMole(ref mole, goldenMoleDamagePerHit);
                    return;
                }
            }

            KillMole(mole);
        }

        private void DamageMole(ref MoleStateS2C mole, byte damageAmount)
        {
            mole.RemainingLives -= damageAmount;
            _netEventsDataService.AddGoldenMoleDamagedNetEvent(_processedTick, _moleId, mole.MoleHoleId, mole.RemainingLives, mole.MaxLives);
        }

        private void KillMole(MoleStateS2C mole)
        {
            var simulationState = _matchDataService.SimulationState;
            var whacAMoleConfig = _gamePlayConfigService.GamePlayConfig.WhacAMole;
            var isGolden = mole.IsGolden;
            var moleHoleId = mole.MoleHoleId;
            var score = isGolden ? whacAMoleConfig.GoldenMoleScoreOnKill : whacAMoleConfig.ScorePerMoleKilled;
            simulationState.RemoveMoleById(_moleId);
            _molesSpawnCooldownService.RegisterMoleHoleToBeOnCooldown(moleHoleId, _processedTick);
            _physicsSimulator.RemoveMole(_moleId);
            simulationState.AddStageScoreForTeam(_byTeamId, score);
            var byPlayerMolesKilledScoreTotal = simulationState.AddStageScoreForPlayer(_byPlayerId, score);
            _netEventsDataService.AddMoleKilledNetEvent(_processedTick, _moleId, moleHoleId, _byPlayerId, (byte)score, simulationState.StageScorePerTeamId[_byTeamId], byPlayerMolesKilledScoreTotal, isGolden);
        }
    }
}
