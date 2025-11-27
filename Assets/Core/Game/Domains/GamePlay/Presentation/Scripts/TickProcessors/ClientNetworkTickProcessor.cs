using Core.Game.Domains.GamePlay.Presentation.Scripts.Commands.Inputs;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.StateMachineService;
using CoreDomain.Scripts.Services.UpdateService;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Network
{
    public class ClientNetworkTickProcessor : ITickProcessor, IFixedUpdatable
    {
        public int CurrentTick { get; private set; }

        //private readonly ClientSimulationStateHandler _clientSimulationStateHandler;
        private readonly IUpdateSubscriptionService _updateSubscriptionService;
        private readonly ICommandFactory _commandFactory;
        private readonly IStateMachineService _stateMachineService;
        private SendInputsToServerCommand _saveInputsToServerCommand;
        private readonly IClientNetworkManager _networkManager;

        private TimerFixedThreaded _fixedTimer;

        public ClientNetworkTickProcessor(IClientNetworkManager networkManager,
            //ClientSimulationStateHandler clientSimulationStateHandler,
            IUpdateSubscriptionService updateSubscriptionService, ICommandFactory commandFactory)
        {
            _networkManager = networkManager;
            //_clientSimulationStateHandler = clientSimulationStateHandler;
            _updateSubscriptionService = updateSubscriptionService;
            _commandFactory = commandFactory;
        }

        public void InitEntryPoint()
        {
            _saveInputsToServerCommand = _commandFactory.CreateCommandVoid<SendInputsToServerCommand>();
            StartTick();
        }

        private void StartTick()
        {
            _updateSubscriptionService.RegisterFixedUpdatable(this);
        }
        
        public void StopTick()
        {
            _updateSubscriptionService.UnregisterFixedUpdatable(this);
        }

        public void ManagedFixedUpdate()
        {
            _networkManager.PollEvents();
            if (!_networkManager.IsPeerConnected)
            {
                return;
            }
            CurrentTick++;
            //ProccesEvents();
            //UpdateGameStateView();
            SendCurrentTickInputsToServer();
            //var SimulationStateForCurrentTick = _clientSimulationStateHandler.GetSortedStates(CurrentTick);
        }

        private void SendCurrentTickInputsToServer()
        {
            _saveInputsToServerCommand.Execute();
        }

        private void OnTick()
        {
            
            // Pass inputs to Simulator and update Current State
            //_serverState.Tick = CurrentTick;
            // Send current state to all players
            // _playerManager.LogicUpdate();
            // if (_serverTick % 2 == 0)
            // {
            //     
            //     int pCount = _playerManager.Count;
            //     
            //     foreach(ServerPlayer p in _playerManager)
            //     {
            //         SendStateToPlayer(p, pCount);
            //     }
            // }
        }

        // private void SendStateToPlayer(ServerPlayer p, int pCount)

        // {

        //     int statesMax = p.AssociatedPeer.GetMaxSinglePacketSize(DeliveryMethod.Unreliable) - ServerState.HeaderSize;

        //     statesMax /= PlayerState.Size;

        //         

        //     for (int s = 0; s < (pCount-1)/statesMax + 1; s++)

        //     {

        //         //TODO: divide

        //         _serverState.LastProcessedCommand = p.LastProcessedCommandId;

        //         _serverState.PlayerStatesCount = pCount;

        //         _serverState.StartState = s * statesMax;

        //         p.AssociatedPeer.Send(WriteSerializable(PacketType.ServerState, _serverState), DeliveryMethod.Unreliable);

        //     }

        // }


        // private NetDataWriter WriteSerializable<T>(PacketType type, T packet) where T : struct, INetSerializable

        // {

        //     _cachedWriter.Reset();

        //     _cachedWriter.Put((byte) type);

        //     packet.Serialize(_cachedWriter);

        //     return _cachedWriter;

        // }

        //

        // private NetDataWriter WritePacket<T>(T packet) where T : class, new()

        // {

        //     _cachedWriter.Reset();

        //     _cachedWriter.Put((byte) PacketType.Serialized);

        //     _packetProcessor.Write(_cachedWriter, packet);

        //     return _cachedWriter;

        // }
    }
}