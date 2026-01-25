using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.ContextInstaller
{
    public class ServerEntryPointCommand : BaseCommand, ICommandVoid
    {
        private IServerNetworkManager _serverNetworkManager;
        private IPhysicsSimulator _physicsSimulator;

        public override void ResolveDependencies()
        {
            _serverNetworkManager = _diContainer.Resolve<IServerNetworkManager>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
        }

        public void Execute()
        {
            _serverNetworkManager.InitEntryPoint();
            _physicsSimulator.InitEntryPoint();
            StartMatchMaking();
        }

        private void StartMatchMaking()
        {
            ReflectionUtils.CreateInstace("Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.Initiator.ServerMatchMakingInstaller", "SimulationMatchMakingAssembly", _diContainer);
        }
    }
}