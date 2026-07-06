using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class TryDeactivateEndedGalacticFieldsCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private INetEventsDataService _netEventsDataService;

        private int _tick;

        public TryDeactivateEndedGalacticFieldsCommand SetTick(int tick)
        {
            _tick = tick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
        }

        public void Execute()
        {
            var fields = _matchDataService.SimulationState.GalacticForceFields;
            if (fields.Count == 0)
            {
                return;
            }

            for (int i = fields.Count - 1; i >= 0; i--)
            {
                var field = fields[i];

                var didEnd = _tick >= field.EndTick;
                if (didEnd)
                {
                    _matchDataService.SimulationState.RemoveGalacticForceFieldById(field.Id);
                    _netEventsDataService.AddDeactivateGalacticForceFieldNetEvent(_tick, field.Id);
                }
            }
        }
    }
}
