using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MolesSpawner;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class TryHideExpiredMolesCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IMolesSpawnCooldownService _molesSpawnCooldownService;
        private IPhysicsSimulator _physicsSimulator;
        private INetEventsDataService _netEventsDataService;
        private SharedGamePlayConfig _sharedGamePlayConfig;
        private NetworkConfig _networkConfig;

        private int _processedTick;

        public TryHideExpiredMolesCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _molesSpawnCooldownService = _diContainer.Resolve<IMolesSpawnCooldownService>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
            _sharedGamePlayConfig = _diContainer.Resolve<SharedGamePlayConfig>();
            _networkConfig = _diContainer.Resolve<NetworkConfig>();
        }

        public void Execute()
        {
            if (_matchDataService.SimulationState.StageType != StageType.WhacAMole)
            {
                return;
            }

            var moles = _matchDataService.SimulationState.Moles;

            for (var i = moles.Count - 1; i >= 0; i--)
            {
                if (moles.GetByIndex(i).IsShakingBeforeHiding)
                {
                    TryHideShakingMole(moles, i);
                    continue;
                }

                TryStartHideShake(ref moles.GetByIndex(i));
            }
        }

        private void TryHideShakingMole(FixedUnorderedList<MoleStateS2C> moles, int moleIndex)
        {
            ref var mole = ref moles.GetByIndex(moleIndex);

            if (_processedTick < mole.HideOnTick)
            {
                return;
            }

            if (mole.IsEmerged)
            {
                _physicsSimulator.RemoveMole(mole.Id);
            }

            _molesSpawnCooldownService.RegisterMoleHoleToBeOnCooldown(mole.MoleHoleId, _processedTick);
            moles.RemoveAt(moleIndex);
        }

        private void TryStartHideShake(ref MoleStateS2C mole)
        {
            var hasReachedLifetimeEnd = mole.HasLifetimeEnd && _processedTick >= mole.DisappearOnTick;

            if (!hasReachedLifetimeEnd)
            {
                return;
            }

            mole.HideOnTick = _processedTick + CalculateHideShakeTicks();
            _netEventsDataService.AddMoleExpiredNetEvent(_processedTick, mole.Id, mole.MoleHoleId, mole.HideOnTick);
        }

        private int CalculateHideShakeTicks()
        {
            return (int)System.MathF.Ceiling(_sharedGamePlayConfig.MoleHideShakeDurationSeconds * _networkConfig.TicksPerSeconds);
        }
    }
}
