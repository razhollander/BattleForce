using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    /// <summary>
    /// A chicken egg and a mole are both static bodies, and Box2D pairs a body only when at least one of the two is dynamic,
    /// so their overlap never reaches ProcessCachedCollisionsCommand. It is resolved here instead: an egg laid over an emerged
    /// mole - or a mole emerging under an egg - whacks the mole and breaks the egg, exactly like an egg a player ran into.
    /// </summary>
    public class TryBreakChickenEggsOnMolesCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IPhysicsSimulator _physicsSimulator;
        private INetEventsDataService _netEventsDataService;
        private ISimulationGamePlayConfigService _gamePlayConfigService;
        private TryHitMoleCommand _tryHitMoleCommand;

        private int _processedTick;

        public TryBreakChickenEggsOnMolesCommand SetProcessedTick(int processedTick)
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
            _tryHitMoleCommand = _diContainer.Resolve<ICommandFactory>().CreateCommandVoid<TryHitMoleCommand>();
        }

        public void Execute()
        {
            var simulationState = _matchDataService.SimulationState;

            if (simulationState.Moles.Count == 0 || simulationState.ChickenEggs.Count == 0)
            {
                return;
            }

            var moleRadius = _gamePlayConfigService.GamePlayConfig.WhacAMole.MoleRadius;

            for (int eggIndex = simulationState.ChickenEggs.Count - 1; eggIndex >= 0; eggIndex--) // backwards, since a broken egg is removed from the list
            {
                var egg = simulationState.ChickenEggs[eggIndex];
                var casterPlayerState = simulationState.GetPlayerById(egg.PlayerCasterId);
                var breakDistance = casterPlayerState.Spaceship.Transform.Radius + moleRadius; // an egg is created with its caster's radius
                var breakDistanceSquared = breakDistance * breakDistance;

                for (int moleIndex = simulationState.Moles.Count - 1; moleIndex >= 0; moleIndex--)
                {
                    var mole = simulationState.Moles[moleIndex];

                    if (!mole.IsEmerged || (mole.Position - egg.Position).LengthSquared() > breakDistanceSquared)
                    {
                        continue;
                    }

                    _tryHitMoleCommand
                        .SetMoleId(mole.Id)
                        .SetByPlayerId(egg.PlayerCasterId)
                        .SetByTeamId(casterPlayerState.TeamId)
                        .SetProcessedTick(_processedTick)
                        .Execute();

                    BreakEgg(egg);
                    break; // the egg is gone, so it cannot whack a second mole
                }
            }
        }

        private void BreakEgg(TalentChickenEggStateS2C egg)
        {
            _netEventsDataService.AddChickenEggHitNetEventS2C(_processedTick, egg.Id);
            _physicsSimulator.RemoveChickenEgg(egg.Id);
            _matchDataService.SimulationState.RemoveChickenEggById(egg.Id);
        }
    }
}
