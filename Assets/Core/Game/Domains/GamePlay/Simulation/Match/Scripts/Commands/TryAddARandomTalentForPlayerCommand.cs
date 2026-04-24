
using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent;
using Core.Game.Domains.GamePlay.Simulation.Scripts.RNG;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class TryAddARandomTalentForPlayerCommand : BaseCommand, ICommandVoid
    {
        private IPlayersTalentsManager _playersTalentsManager;
        private IMatchDataService _matchDataService;
        
        private ushort _playerId;
        private List<TalentType> _allTalentValues;
        private List<TalentType> _cacheAvailableTalentValues;

        public TryAddARandomTalentForPlayerCommand SetPlayerId(ushort playerId)
        {
            _playerId = playerId;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _playersTalentsManager = _diContainer.Resolve<IPlayersTalentsManager>();
            
            _allTalentValues = ((TalentType[])System.Enum.GetValues(typeof(TalentType))).ToList();
            _allTalentValues.Remove(TalentType.None);
            _cacheAvailableTalentValues = new List<TalentType>();
        }

        public void Execute()
        {
            var player = _matchDataService.SimulationState.GetPlayerById(_playerId);
            _cacheAvailableTalentValues.Clear();
            _cacheAvailableTalentValues.AddRange(_allTalentValues);

            for (int i = 0; i < player.Spaceship.TalentsState.Talents.Count; i++)
            {
                _cacheAvailableTalentValues.Remove(player.Spaceship.TalentsState.Talents[i].TalentType);
            }
            
            var rndIndex = RNG.NextInt(0, _cacheAvailableTalentValues.Count);
            var randomlyChosenTalentType = _cacheAvailableTalentValues[rndIndex];
            _playersTalentsManager.TryAddTalentToPlayer(randomlyChosenTalentType, player.Id, 0, out _, out _);
        }
    }
}