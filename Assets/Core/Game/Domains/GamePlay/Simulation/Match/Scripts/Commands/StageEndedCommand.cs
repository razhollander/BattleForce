using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class StageEndedCommand : BaseCommand, ICommandVoid
    {
        private ushort _winningTeamId;
        private int _processedTick;

        private IMatchDataService _matchDataService;
        private INetEventsDataService _netEventsDataService;
        private IStageDataService _stageDataService;

        public StageEndedCommand SetWinningTeamId(ushort winningTeamId)
        {
            _winningTeamId = winningTeamId;
            return this;
        }

        public StageEndedCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
            _stageDataService = _diContainer.Resolve<IStageDataService>();
        }

        public void Execute()
        {
            LogService.LogTopic($"Match Ended! Winning Team: {_winningTeamId}", LogTopicType.ServerNetwork);

            var jemsPerTeam = _matchDataService.SimulationState.JemsPerTeamId;
            _stageDataService.AddWinnerTeam(_winningTeamId);
            var jemsWonPerTeam = _stageDataService.GetJemsCollectedPerTeam();
            _netEventsDataService.AddStageEndNetEvent(_processedTick, _winningTeamId, jemsWonPerTeam, jemsPerTeam);
            _stageDataService.IsMatchEnded = true;
        }
    }
}
