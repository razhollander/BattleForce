using System;
using System.Reflection;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.ContextInstaller
{
    public class ServerEntryPointCommand : BaseCommand, ICommandVoid
    {
        private IServerNetworkManager _serverNetworkManager;
        private ITickProcessor _tickProcessor;
        private IPhysicsSimulator _physicsSimulator;
        private ICommandFactory _commandFactory;

        public override void ResolveDependencies()
        {
            _serverNetworkManager = _diContainer.Resolve<IServerNetworkManager>();
            _tickProcessor = _diContainer.Resolve<ITickProcessor>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
        }

        public void Execute()
        {
            _serverNetworkManager.InitEntryPoint();
            _tickProcessor.InitEntryPoint();
            _physicsSimulator.InitEntryPoint();
            StartMatchMaking();
        }

        private void StartMatchMaking()
        {
            // 1. Ensure the type name is a string, not a type reference.
//            string typeName = "Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.Initiator.ServerMatchMakingInstaller";

            // 2. Get the assembly (ensure SimulationMatchMakingAssembly variable exists).
            // SimulationMatchMakingAssembly should be of type System.Reflection.Assembly
  //          Type installerType = SimulationMatchMakingAssembly.GetType(typeName);
            var installerType = Type.GetType("Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.Initiator.ServerMatchMakingInstaller, SimulationMatchMakingAssembly");

            if (installerType != null)
            {
                // 3. Find the method (assuming it's named "InstallBindings" and takes one parameter)
                MethodInfo method = installerType.GetMethod("InstallBindings");

                if (method != null)
                {
                    // 4. Create instance of the class (if not static)
                    object installerInstance = Activator.CreateInstance(installerType);

                    // 5. Invoke with correct parameters as an array
                    method.Invoke(installerInstance, new object[] { _diContainer });
                }
                else
                {
                    LogService.LogError("No InstallBindings method found!");
                }
            }
            else
            {
                LogService.LogError("No installer found!");
            }
        }
    }
}