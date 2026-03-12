using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class TryEndStagePreparationPhaseCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private INetEventsDataService _netEventsDataService;
        private IStageDataService _stageDataService;
        private IPreparationPhaseTimerService _preparationPhaseTimerService;
        
        private int _tick;

        public TryEndStagePreparationPhaseCommand SetProcessedTick(int tick)
        {
            _tick = tick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
            _preparationPhaseTimerService = _diContainer.Resolve<IPreparationPhaseTimerService>();
            _stageDataService = _diContainer.Resolve<IStageDataService>();
        }

        public void Execute()
        {
            if (!_preparationPhaseTimerService.IsTimerCompleted() || !_matchDataService.SimulationState.IsInPreparationPhase)
            {
                return;
            }

            _matchDataService.SimulationState.IsInPreparationPhase = false;
            _matchDataService.SimulationState.StartPhaseInitialTick = _tick;
            _matchDataService.EnvironmentData.RemoveAllFieldBarriers();
            _netEventsDataService.AddPreparationPhaseEndedNetEvent(_tick);
        }
    }
}
