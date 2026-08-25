using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUp;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Inputs;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    // Switching and activating are one sequence: the talent this tick selects is the talent this tick may fire, so
    // nothing may run the activation without the switch that precedes it.
    public class ApplyPlayerTalentsInputCommand : BaseCommand, ICommandVoid
    {
        private const int TALENT_A_INDEX = 0;
        private const int TALENT_B_INDEX = 1;
        private const int TALENT_C_INDEX = 2;
        private const int MIN_TALENTS_TO_SWITCH_BETWEEN = 2;
        private const int NO_SWITCHED_TALENT_INDEX = -1;

        private IMatchDataService _matchDataService;
        private ISimulationInputService _simulationInputService;
        private IPlayersTalentsManager _playersTalentsManager;
        private IPlayersPowerUpsManager _playersPowerUpsManager;
        private INetEventsDataService _netEventsDataService;

        private ushort _playerId;
        private int _processedTick;
        private float _deltaTime;
        private bool _isTalentAInputPressed;
        private bool _isTalentBInputPressed;
        private bool _isTalentCInputPressed;

        public ApplyPlayerTalentsInputCommand SetPlayerId(ushort playerId)
        {
            _playerId = playerId;
            return this;
        }

        public ApplyPlayerTalentsInputCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }

        public ApplyPlayerTalentsInputCommand SetDeltaTime(float deltaTime)
        {
            _deltaTime = deltaTime;
            return this;
        }

        public ApplyPlayerTalentsInputCommand SetTalentInputsPressed(bool isTalentAInputPressed, bool isTalentBInputPressed, bool isTalentCInputPressed)
        {
            _isTalentAInputPressed = isTalentAInputPressed;
            _isTalentBInputPressed = isTalentBInputPressed;
            _isTalentCInputPressed = isTalentCInputPressed;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _simulationInputService = _diContainer.Resolve<ISimulationInputService>();
            _playersTalentsManager = _diContainer.Resolve<IPlayersTalentsManager>();
            _playersPowerUpsManager = _diContainer.Resolve<IPlayersPowerUpsManager>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
        }

        public void Execute()
        {
            var playerState = _matchDataService.SimulationState.GetPlayerById(_playerId);
            TrySwitchTalent(playerState);
            ProcessSelectedTalentInput(playerState);
        }

        private void TrySwitchTalent(PlayerStateS2C playerState)
        {
            var doesPlayerHaveLessThan2Talents = playerState.Spaceship.TalentsState.Talents.Count < MIN_TALENTS_TO_SWITCH_BETWEEN;
            if (doesPlayerHaveLessThan2Talents)
            {
                return;
            }

            var currentSelectedTalentIndex = playerState.Spaceship.TalentsState.SelectedTalentIndex;
            var switchedTalentIndex = NO_SWITCHED_TALENT_INDEX;

            TrySwitchToTalent(TALENT_A_INDEX, PlayerInputType.TalentAInput, currentSelectedTalentIndex, ref switchedTalentIndex);
            TrySwitchToTalent(TALENT_B_INDEX, PlayerInputType.TalentBInput, currentSelectedTalentIndex, ref switchedTalentIndex);
            TrySwitchToTalent(TALENT_C_INDEX, PlayerInputType.TalentCInput, currentSelectedTalentIndex, ref switchedTalentIndex);

            var didSwitchToAnyTalent = switchedTalentIndex != NO_SWITCHED_TALENT_INDEX;
            if (didSwitchToAnyTalent)
            {
                _netEventsDataService.AddTalentSwitchNetEvent(_processedTick, _playerId, switchedTalentIndex);
            }
        }

        private void TrySwitchToTalent(int talentIndex, PlayerInputType talentInputType, int currentSelectedTalentIndex, ref int switchedTalentIndex)
        {
            var wasTalentInputDownThisTick = _simulationInputService.WasInputDownThisTick(_playerId, talentInputType);
            var isTalentAlreadySelected = currentSelectedTalentIndex == talentIndex;
            if (!wasTalentInputDownThisTick || isTalentAlreadySelected)
            {
                return;
            }

            if (_playersTalentsManager.TrySwitchToTalent(_playerId, talentIndex, _processedTick))
            {
                switchedTalentIndex = talentIndex;
            }
        }

        private void ProcessSelectedTalentInput(PlayerStateS2C playerState)
        {
            if (!playerState.Spaceship.TalentsState.TryGetCurrentSelectedTalent(out var currentSelectedTalent))
            {
                return;
            }

            if (_playersPowerUpsManager.IsPowerUpActiveForPlayer(_playerId))
            {
                return;
            }

            if (currentSelectedTalent.IsOnCooldown())
            {
                return;
            }

            var selectedTalentInputType = GetSelectedTalentInputType(playerState, out var isSelectedTalentInputPressed);
            var wasSelectedTalentInputReleased = _simulationInputService.WasInputReleasedThisTick(_playerId, selectedTalentInputType);
            var wasSelectedTalentInputDown = _simulationInputService.WasInputDownThisTick(_playerId, selectedTalentInputType);

            _playersTalentsManager.ProcessPlayerTalentInput(_playerId, currentSelectedTalent.TalentType, _processedTick, wasSelectedTalentInputDown, isSelectedTalentInputPressed, wasSelectedTalentInputReleased, _deltaTime);
        }

        private PlayerInputType GetSelectedTalentInputType(PlayerStateS2C playerState, out bool isSelectedTalentInputPressed)
        {
            switch (playerState.Spaceship.TalentsState.SelectedTalentIndex)
            {
                case TALENT_B_INDEX:
                    isSelectedTalentInputPressed = _isTalentBInputPressed;
                    return PlayerInputType.TalentBInput;
                case TALENT_C_INDEX:
                    isSelectedTalentInputPressed = _isTalentCInputPressed;
                    return PlayerInputType.TalentCInput;
                default:
                    isSelectedTalentInputPressed = _isTalentAInputPressed;
                    return PlayerInputType.TalentAInput;
            }
        }
    }
}
