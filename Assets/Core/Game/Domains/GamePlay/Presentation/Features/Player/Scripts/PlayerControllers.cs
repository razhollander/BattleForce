using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Shared;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts
{
    public class PlayerControllers : IPlayerControllers
    {
        private readonly IMatchDataService _matchDataService;
        private readonly PlayerView _playerViewPrefab;
        private readonly List<PlayerController> _playerControllers = new ();
        private GameObject _playersParent;

        public PlayerControllers(IMatchDataService matchDataService, PlayerView playerViewPrefab)
        {
            _matchDataService = matchDataService;
            _playerViewPrefab = playerViewPrefab;
        }

        public void InitEntryPoint()
        {
            _playersParent = new GameObject("PlayersParent");
        }

        public void CreatePlayer(int playerId)
        {
            var playerController = new PlayerController(playerId, _matchDataService);
            playerController.CreatePlayerView(_playerViewPrefab, _playersParent.transform);
            _playerControllers.Add(playerController);
        }
    }
}