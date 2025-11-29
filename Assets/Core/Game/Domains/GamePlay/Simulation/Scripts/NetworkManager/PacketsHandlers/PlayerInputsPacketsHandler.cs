using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using Core.Game.Domains.GamePlay.Shared.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.NetworkManager.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using CoreDomain.Scripts.Extensions;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.PacketsHandlers
{
    public class PlayerInputsPacketsHandler : IPlayerInputsPacketsHandler
    {
        private readonly IServerNetworkManager _networkManager;
        private readonly IMatchDataService _matchDataService;
        private readonly GamePlayConfig _gamePlayConfig;
        private readonly NetworkConfig _networkConfig;
        private readonly Dictionary<int, Dictionary<int, PlayerInputPacketC2S>> _inputsByTick = new ();

        public PlayerInputsPacketsHandler(IServerNetworkManager networkManager, IMatchDataService matchDataService, GamePlayConfig gamePlayConfig, NetworkConfig networkConfig)
        {
            _networkManager = networkManager;
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
            _networkConfig = networkConfig;
        }

        public void InitEntryPoint()
        {
            _networkManager.SubscribeNetSerializable<PlayerInputPacketC2S>(OnPlayerInputReceived);
        }

        public void InitExitPoint()
        {
            _networkManager.RemoveSubscription<PlayerInputPacketC2S>();
        }

        public Dictionary<int, PlayerInputPacketC2S> ProcessInputsInTick(int tick)
        {
            if (!_inputsByTick.TryGetValue(tick, out var inputsInTick))
            {
                return new Dictionary<int, PlayerInputPacketC2S>();
            }

            foreach (var (playerId, inputs) in inputsInTick)
            {
                var playerModel = _matchDataService.GetPlayer(playerId);
                var rotationDelta = _gamePlayConfig.PlayerSpaceship.RotationSpeed * _networkConfig.DeltaTime;
                var rotationAngle = (inputs.IsMoveLeftInputPressed.ToInt() - inputs.IsMoveRightInputPressed.ToInt()) * rotationDelta;
                playerModel.Spaceship.Transform.RotationVector.Rotate(rotationAngle);
                playerModel.Spaceship.Transform.Position += playerModel.Spaceship.Transform.RotationVector *
                                                            _gamePlayConfig.PlayerSpaceship.MovementSpeed *
                                                            _networkConfig.DeltaTime;
            }

            return inputsInTick;
        }
        
        private void OnPlayerInputReceived(PlayerInputPacketC2S playerInputPacket, int playerId)
        {
            var tick = playerInputPacket.Tick;
            _inputsByTick.TryAdd(tick, new Dictionary<int, PlayerInputPacketC2S>());
            if (!_inputsByTick[tick].TryAdd(playerId, playerInputPacket))
            {
                _inputsByTick[tick][playerId] = playerInputPacket;
            }
            LogService.LogTopic("Input packet received from player id" + playerId, LogTopicType.ServerNetwork);
        }
    }
}