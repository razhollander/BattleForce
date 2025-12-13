using Core.Game.Domains.GamePlay.Presentation.Scripts.Commands.Inputs;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.StateMachineService;
using CoreDomain.Scripts.Services.UpdateService;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Network
{
    public class ClientNetworkTickProcessor : ITickProcessor, IFixedUpdatable
    {
        public void SetTick(int tickOnServer)
        {
            CurrentTick = tickOnServer;
        }

        public int CurrentTick { get; private set; }

        //private readonly ClientSimulationStateHandler _clientSimulationStateHandler;
        private readonly IUpdateSubscriptionService _updateSubscriptionService;
        private readonly ICommandFactory _commandFactory;
        private readonly IFullTickPacketsHandler _fullTickPacketsHandler;
        private readonly IMatchDataService _matchDataService;
        private readonly IStateMachineService _stateMachineService;
        private SendInputsToServerCommand _sendInputsToServerCommand;
        private readonly IClientNetworkManager _networkManager;

        private TimerFixedThreaded _fixedTimer;

        public ClientNetworkTickProcessor(IClientNetworkManager networkManager,
            //ClientSimulationStateHandler clientSimulationStateHandler,
            IUpdateSubscriptionService updateSubscriptionService, ICommandFactory commandFactory,
            IFullTickPacketsHandler fullTickPacketsHandler, IMatchDataService matchDataService)
        {
            _networkManager = networkManager;
            //_clientSimulationStateHandler = clientSimulationStateHandler;
            _updateSubscriptionService = updateSubscriptionService;
            _commandFactory = commandFactory;
            _fullTickPacketsHandler = fullTickPacketsHandler;
            _matchDataService = matchDataService;
        }

        public void InitEntryPoint()
        {
            _sendInputsToServerCommand = _commandFactory.CreateCommandVoid<SendInputsToServerCommand>();
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
            CurrentTick = _fullTickPacketsHandler.ProcessStateLatestTick(CurrentTick);
            
            if (_matchDataService.IsPlayerJoined) 
            {
                SendCurrentTickInputsToServer();
            }
        }

        private void SendCurrentTickInputsToServer()
        {
            _sendInputsToServerCommand.Execute();
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