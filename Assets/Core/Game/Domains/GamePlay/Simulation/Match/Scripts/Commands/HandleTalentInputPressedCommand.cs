using Core.Game.Domains.GamePlay.Shared.S2CModels;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class HandleTalentInputPressedCommand : BaseCommand, ICommandVoid
    {
        private PlayerStateS2C _playerState;
        private TalentStateS2C _talent;
        private int _processedTick;

        public HandleTalentInputPressedCommand SetPlayerState(PlayerStateS2C playerState)
        {
            _playerState = playerState;
            return this;
        }

        public HandleTalentInputPressedCommand SetTalent(TalentStateS2C talent)
        {
            _talent = talent;
            return this;
        }

        public HandleTalentInputPressedCommand SetTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }

        public override void ResolveDependencies()
        {
                
        }

        public void Execute()
        {
            switch (_talent.TalentType)
            {
                case TalentType.Swap: break;
            }
        }
    }
}