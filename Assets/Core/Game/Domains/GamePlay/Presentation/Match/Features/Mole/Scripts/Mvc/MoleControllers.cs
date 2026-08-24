using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.Logger.Base;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Mole.Scripts.Mvc
{
    public class MoleControllers : IMoleControllers
    {
        private readonly MoleViewPool _molesViewPool;
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;
        private readonly IAudioService _audioService;
        private readonly Dictionary<ushort, MoleController> _controllerPerMoleHoleId = new Dictionary<ushort, MoleController>();
        private Transform _parent;

        public MoleControllers(MoleView moleViewPrefab, DiContainer diContainer, IStageCancellationTokenProvider stageCancellationTokenProvider, IAudioService audioService)
        {
            _molesViewPool = new MoleViewPool(moleViewPrefab, diContainer);
            _stageCancellationTokenProvider = stageCancellationTokenProvider;
            _audioService = audioService;
        }

        public void InitEntryPoint()
        {
            _parent = (new GameObject("MolesParent")).transform;
            _molesViewPool.InitPool();
        }

        public void CreateMoleAtSpawnPoint(ushort moleHoleId, Vector2 spawnPointPosition)
        {
            var controller = new MoleController(spawnPointPosition, _molesViewPool, _parent, _stageCancellationTokenProvider, _audioService);
            controller.CreateView();
            _controllerPerMoleHoleId[moleHoleId] = controller;
        }

        public void SetMoleEmergingFromHole(ushort moleId, ushort moleHoleId, float shakeDurationSeconds, bool isGolden, byte remainingLives, byte maxLives)
        {
            if (!_controllerPerMoleHoleId.TryGetValue(moleHoleId, out var controller))
            {
                LogService.LogError($"No mole hole {moleHoleId} to pop mole {moleId} out of!");
                return;
            }

            controller.SetEmergingFromHoleState(shakeDurationSeconds, isGolden, remainingLives, maxLives);
        }

        public void SetGoldenMoleDamaged(ushort moleId, ushort moleHoleId, byte remainingLives, byte maxLives)
        {
            if (!TryGetHoleOfActiveMole(moleId, moleHoleId, out var controller))
            {
                return;
            }
            
            controller.SetGoldenMoleDamaged(remainingLives, maxLives);
        }

        public void SetMoleKilled(ushort moleId, ushort moleHoleId)
        {
            if (!TryGetHoleOfActiveMole(moleId, moleHoleId, out var controller))
            {
                return;
            }

            controller.SetHitState();
        }

        public void SetMoleExpiring(ushort moleId, ushort moleHoleId, float shakeDurationSeconds)
        {
            if (!TryGetHoleOfActiveMole(moleId, moleHoleId, out var controller))
            {
                return;
            }

            controller.SetExpiringState(shakeDurationSeconds);
        }

        public void SetAllMolesInHole()
        {
            foreach (var controller in _controllerPerMoleHoleId.Values)
            {
                if (controller.HasActiveMole)
                {
                    controller.SetInHoleState(false);
                }
            }
        }

        public bool TryGetMoleHolePosition(ushort moleHoleId, out Vector2 position)
        {
            if (!_controllerPerMoleHoleId.TryGetValue(moleHoleId, out var controller))
            {
                position = Vector2.zero;
                return false;
            }

            position = controller.SpawnPointPosition;
            return true;
        }

        public void DestroyAll()
        {
            foreach (var controller in _controllerPerMoleHoleId.Values)
            {
                controller.DestroyView();
            }

            _controllerPerMoleHoleId.Clear();
        }

        private bool TryGetHoleOfActiveMole(ushort moleId, ushort moleHoleId, out MoleController moleController)
        {
            if (!_controllerPerMoleHoleId.TryGetValue(moleHoleId, out moleController))
            {
                LogService.LogError($"No mole hole {moleHoleId} for mole {moleId}!");
                return false;
            }

            return moleController.HasActiveMole;
        }
    }
}
