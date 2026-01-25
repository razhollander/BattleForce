using CoreDomain.Scripts.Services.CommandFactory;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands
{
    public class MoveInputInvokedCommand : BaseCommand, ICommandVoid
    {
        private Vector2 _moveValue;

        public MoveInputInvokedCommand SetMoveValue(Vector2 moveValue)
        {
            _moveValue = moveValue;
            return this;
        }

        public override void ResolveDependencies()
        {
            
        }

        public void Execute()
        {
            // send to server input
        }
    }
}
