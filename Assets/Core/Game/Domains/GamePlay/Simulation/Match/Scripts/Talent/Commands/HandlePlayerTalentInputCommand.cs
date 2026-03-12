using Core.Game.Domains.GamePlay.Shared.S2CModels;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent.Commands
{
    public class HandlePlayerTalentInputCommand : BaseCommand, ICommandVoid
    {
        private IPlayersTalentsManager _playersTalentsManager;
        
        private ushort _playerId;
        private TalentType _talentType;
        private int _tick;
        private bool _isTalentInputPressed;
        private float _deltaTime;

        public HandlePlayerTalentInputCommand SetPlayerId(ushort playerId)
        {
            _playerId = playerId;
            return this;
        }

        public HandlePlayerTalentInputCommand SetTalent(TalentType talentType)
        {
            _talentType = talentType;
            return this;
        }

        public HandlePlayerTalentInputCommand SetTick(int tick)
        {
            _tick = tick;
            return this;
        }

        public HandlePlayerTalentInputCommand SetIsTalentInputPressed(bool isTalentInputPressed)
        {
            _isTalentInputPressed = isTalentInputPressed;
            return this;
        }
        
        public HandlePlayerTalentInputCommand SetDeltaTime(float deltaTime)
        {
            _deltaTime = deltaTime;
            return this;
        }
        
        public override void ResolveDependencies()
        {
            _playersTalentsManager = _diContainer.Resolve<IPlayersTalentsManager>();
        }

        public void Execute()
        {
            _playersTalentsManager.ProcessPlayerTalentInput(_playerId, _talentType, _tick, _isTalentInputPressed, _deltaTime);
        }
    }
}