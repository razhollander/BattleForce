// using System.Collections.Generic;
// using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
// using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
// using UnityEngine;
// using Zenject;
//
// namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.LockOnHeartSights.Scripts
// {
//     public class LockOnHeartSightsEffectController : ILockOnHeartSightsEffectController
//     {
//         private readonly IMatchDataService _matchDataService;
//         private readonly IMatchPlayerControllers _matchPlayerControllers;
//         private readonly LockOnHeartSightsEffectPool _pool;
//
//         // Maps target player id -> list of active sight effects on them
//         private readonly Dictionary<ushort, List<LockOnHeartSightView>> _activeSights = new Dictionary<ushort, List<LockOnHeartSightView>>();
//         private readonly Dictionary<ushort, int> _expectedSightsCount = new Dictionary<ushort, int>();
//         private readonly List<ushort> _playersToRemove = new List<ushort>();
//
//
//         public LockOnHeartSightsEffectController(IMatchDataService matchDataService, IMatchPlayerControllers matchPlayerControllers, LockOnHeartSightView prefab, DiContainer diContainer)
//         {
//             _matchDataService = matchDataService;
//             _matchPlayerControllers = matchPlayerControllers;
//             _pool = new LockOnHeartSightsEffectPool(prefab, diContainer);
//         }
//
//         public void InitEntryPoint()
//         {
//             _pool.InitPool();
//         }
//
//         public void ManagedUpdate()
//         {
//             _expectedSightsCount.Clear();
//
//             // Count how many sights should be on each player
//             foreach (var player in _matchDataService.Players)
//             {
//                 var heartsOnTarget = player.Spaceship.PlayerHeartsOnTarget;
//                 if (heartsOnTarget == null) continue;
//
//                 for (int i = 0; i < heartsOnTarget.Count; i++)
//                 {
//                     var targetId = heartsOnTarget[i];
//                     if (!_expectedSightsCount.ContainsKey(targetId))
//                     {
//                         _expectedSightsCount[targetId] = 0;
//                     }
//                     _expectedSightsCount[targetId]++;
//                 }
//             }
//
//             // Sync the active sights
//             _playersToRemove.Clear();
//
//             // 1. Remove excess sights
//             foreach (var kvp in _activeSights)
//             {
//                 var targetId = kvp.Key;
//                 var sights = kvp.Value;
//                 int expectedCount = _expectedSightsCount.ContainsKey(targetId) ? _expectedSightsCount[targetId] : 0;
//
//                 while (sights.Count > expectedCount)
//                 {
//                     var sight = sights[sights.Count - 1];
//                     sight.Despawn();
//                     sights.RemoveAt(sights.Count - 1);
//                 }
//
//                 if (sights.Count == 0)
//                 {
//                     playersToRemove.Add(targetId);
//                 }
//             }
//
//             foreach (var id in _playersToRemove)
//             {
//                 _activeSights.Remove(id);
//             }
//
//             // 2. Add missing sights
//             foreach (var kvp in _expectedSightsCount)
//             {
//                 var targetId = kvp.Key;
//                 var expectedCount = kvp.Value;
//
//                 if (!_activeSights.ContainsKey(targetId))
//                 {
//                     _activeSights[targetId] = new List<LockOnHeartSightView>();
//                 }
//
//                 var sights = _activeSights[targetId];
//                 while (sights.Count < expectedCount)
//                 {
//                     var sight = _pool.Spawn();
//                     sights.Add(sight);
//                 }
//             }
//
//             // 3. Update positions
//             foreach (var kvp in _activeSights)
//             {
//                 var targetId = kvp.Key;
//                 var sights = kvp.Value;
//                 if (sights.Count == 0) continue;
//
//                 var heartTransform = _matchPlayerControllers.GetPlayerHeartTransform(targetId);
//                 if (heartTransform != null)
//                 {
//                     foreach (var sight in sights)
//                     {
//                         sight.transform.position = heartTransform.position;
//                     }
//                 }
//             }
//         }
//     }
// }
