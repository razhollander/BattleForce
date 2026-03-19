using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Timer;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands
{
    public class UpdatePlayersTalentsCooldownsDataCommand: BaseCommand, ICommandVoid
    {
        private int _currentServerTick;
        private IMatchDataService _matchDataService;
        private IMatchPlayerTimersService _matchPlayerTimersService;

        public UpdatePlayersTalentsCooldownsDataCommand SetTick(int currentServerTick)
        {
            _currentServerTick = currentServerTick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _matchPlayerTimersService = _diContainer.Resolve<IMatchPlayerTimersService>();
        }

        public void Execute()
        {
            foreach (var playerModel in _matchDataService.Players)
            {
                UpdateTalentsCooldownAccordingToTick(playerModel.PlayerId, playerModel.Spaceship.TalentsState.Talents, _currentServerTick);
            }
        }

        private void UpdateTalentsCooldownAccordingToTick(ushort playerId, FixedOrderedList<TalentStateS2C> playerTalents, int currentServerTick)
        {
            foreach (var playerTalent in playerTalents.AsSpan())
            {
                _matchPlayerTimersService.GetPlayerTalentTimer(playerId)
            }
        }
    }
}