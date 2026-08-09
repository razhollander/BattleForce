using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Game.Domains.GamePlay.Simulation.Scripts.RNG;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage
{
    public class BonusStageRotationService : IBonusStageRotationService
    {
        private const StageType FALLBACK_BONUS_STAGE_TYPE = StageType.WhacAMole; // used only if nothing is configured

        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private readonly List<StageType> _candidateBonusStageTypes;
        private StageType _lastPlayedBonusStageType = StageType.None;

        public BonusStageRotationService(ISimulationGamePlayConfigService gamePlayConfigService)
        {
            _gamePlayConfigService = gamePlayConfigService;
            _candidateBonusStageTypes = new List<StageType>(8);
        }

        public StageType ResolveNextBonusStageType()
        {
            var enabledBonusStageTypes = _gamePlayConfigService.GamePlayConfig.EnabledBonusStageTypes;
            BuildCandidatesExcludingLastPlayed(enabledBonusStageTypes);

            if (_candidateBonusStageTypes.Count == 0)
            {
                return FALLBACK_BONUS_STAGE_TYPE;
            }

            var randomIndex = RNG.NextInt(0, _candidateBonusStageTypes.Count);
            var chosenBonusStageType = _candidateBonusStageTypes[randomIndex];
            _lastPlayedBonusStageType = chosenBonusStageType;
            return chosenBonusStageType;
        }

        public void ResetData()
        {
            _lastPlayedBonusStageType = StageType.None;
        }

        // On the first pick (_lastPlayedBonusStageType == None) nothing is excluded, so the choice is fully random.
        // When only one type is enabled, excluding it empties the list, so we refill with the full enabled set and
        // keep returning that single type instead of deadlocking.
        private void BuildCandidatesExcludingLastPlayed(List<StageType> enabledBonusStageTypes)
        {
            _candidateBonusStageTypes.Clear();

            foreach (var bonusStageType in enabledBonusStageTypes)
            {
                if (bonusStageType != _lastPlayedBonusStageType)
                {
                    _candidateBonusStageTypes.Add(bonusStageType);
                }
            }

            if (_candidateBonusStageTypes.Count == 0)
            {
                _candidateBonusStageTypes.AddRange(enabledBonusStageTypes);
            }
        }
    }
}
