using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Commands.Inputs
{
    public class ShootInputInvokedCommand : BaseCommand, ICommandVoid
    {
        public override void ResolveDependencies()
        {
            
        }

        public void Execute()
        {
            // send to server input
        }
    }
}
