using System.Collections.Generic;
using Core.Scripts.Extensions.Linq;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayerLockOnTarget
{
    public class PlayerLockOnTargetTimers
    {
        public ushort CasterId { get; }
        
        private readonly Dictionary<ushort, float> _targetTimers;
        private readonly List<ushort> _cachedTargetsToRemoveBuffer;

        public PlayerLockOnTargetTimers(ushort casterId)
        {
            CasterId = casterId;
            _targetTimers = new Dictionary<ushort, float>();
            _cachedTargetsToRemoveBuffer = new List<ushort>();
        }

        public void StepTimers(FixedUnorderedList<ushort> targetedIds, float deltaTime)
        {
            _cachedTargetsToRemoveBuffer.Clear();

            foreach (var targetId in _targetTimers.Keys)
            {
                if (!targetedIds.Contains(targetId))
                {
                    _cachedTargetsToRemoveBuffer.Add(targetId);
                }
            }

            for (int i = 0; i < _cachedTargetsToRemoveBuffer.Count; i++)
            {
                _targetTimers.Remove(_cachedTargetsToRemoveBuffer[i]);
            }

            for (int i = 0; i < targetedIds.Count; i++)
            {
                var targetId = targetedIds[i];
                if (_targetTimers.TryGetValue(targetId, out var timer))
                {
                    _targetTimers[targetId] = timer + deltaTime;
                }
                else
                {
                    _targetTimers[targetId] = deltaTime;
                }
            }
        }

        public void CollectPlayersToDamage(float durationLimit, List<(ushort CasterId, ushort TargetId)> outputList)
        {
            foreach (var kvp in _targetTimers)
            {
                if (kvp.Value >= durationLimit)
                {
                    outputList.Add((CasterId, kvp.Key));
                }
            }
        }

        public void ResetTimer(ushort targetId)
        {
            if (_targetTimers.ContainsKey(targetId))
            {
                _targetTimers[targetId] = 0f;
            }
        }

        public void ResetAllTimers()
        {
            _targetTimers.Clear();
        }
    }
}