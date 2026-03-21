using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.LayerOrders;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using CoreDomain.Scripts.Services.CommandFactory;
using UnityEngine;
using DG.Tweening;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleSwapFieldNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchPlayerControllers _matchPlayerControllers;
        private PresentationGamePlayConfig _presentationGamePlayConfig;

        // Map casterId to their visual GameObject
        private Dictionary<ushort, GameObject> _activeFields = new Dictionary<ushort, GameObject>();

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _matchPlayerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _presentationGamePlayConfig = _diContainer.Resolve<PresentationGamePlayConfig>();
        }

        public void Execute()
        {
            ProcessCreateEvents();
            ProcessDestroyEvents();
        }

        private void ProcessCreateEvents()
        {
            if (_cachedPresentationEventsService.CreateSwapFieldNetEvents.Count == 0) return;

            foreach (var evt in _cachedPresentationEventsService.CreateSwapFieldNetEvents)
            {
                if (_activeFields.ContainsKey(evt.CasterPlayerId))
                    continue;

                var playerTransform = _matchPlayerControllers.GetPlayerTransform(evt.CasterPlayerId);
                    

                // Create visual representation
                var fieldGO = new GameObject($"SwapField_{evt.CasterPlayerId}");
                fieldGO.transform.position = playerTransform.position;
                fieldGO.transform.SetParent(playerTransform); // Attach to player

                // Use a primitive sphere for visualization since we don't have a sprite config yet
                var primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                var mf = fieldGO.AddComponent<MeshFilter>();
                mf.sharedMesh = primitive.GetComponent<MeshFilter>().sharedMesh;
                Object.Destroy(primitive); // Cleanup the temp primitive

                var mr = fieldGO.AddComponent<MeshRenderer>();
                mr.material = new Material(Shader.Find("Sprites/Default"));
                mr.material.color = new Color(0.5f, 0.5f, 1f, 0.5f);
                mr.sortingOrder = LayerOrder.SwapField; // Behind player

                // Initial scale
                fieldGO.transform.localScale = Vector3.zero;

                // Animation using Simulation config
                // Since this runs on Presentation, you should ideally have a shared config or presentation config for it
                // Using hardcoded fallback values if we don't have access to simulation config here easily.
                var config = _sharedGamePlayConfig..SwapTalentConfig;
                float maxRadius = 10f; // This should match Simulation config
                float growDurationSeconds = 1f; // This should match Simulation config
                Ease growEase = Ease.Linear;

                // Assuming we can pass these down or we'd add them to PresentationGamePlayConfig

                // Diameter = Radius * 2
                fieldGO.transform.DOScale(maxRadius * 2f, growDurationSeconds).SetEase(growEase);

                _activeFields[evt.CasterPlayerId] = fieldGO;
            }

            _cachedPresentationEventsService.CreateSwapFieldNetEvents.Clear();
        }

        private void ProcessDestroyEvents()
        {
            if (_cachedPresentationEventsService.DestroySwapFieldNetEvents.Count == 0) return;

            foreach (var evt in _cachedPresentationEventsService.DestroySwapFieldNetEvents)
            {
                if (_activeFields.TryGetValue(evt.CasterPlayerId, out var fieldGO))
                {
                    fieldGO.transform.DOKill();
                    Object.Destroy(fieldGO);
                    _activeFields.Remove(evt.CasterPlayerId);
                }
            }

            _cachedPresentationEventsService.DestroySwapFieldNetEvents.Clear();
        }
    }
}
