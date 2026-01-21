using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace CoreDomain.Scripts.Mvc.WorldCamera
{
    [RequireComponent(typeof(CinemachineTargetGroup))]
    public class WorldCameraView : MonoBehaviour
    {
        [SerializeField] private CinemachineTargetGroup _targetGroup;

        public void AddTarget(Transform target, float weight, float radius)
        {
            if (_targetGroup == null) return;
            _targetGroup.AddMember(target, weight, radius);
        }

        public void RemoveTarget(Transform target)
        {
            if (_targetGroup == null) return;
            _targetGroup.RemoveMember(target);
        }

        public void ManualUpdate()
        {
            // Cinemachine updates automatically, but we can add manual logic here if needed
        }

        private void Awake()
        {
            if (_targetGroup == null)
            {
                _targetGroup = GetComponent<CinemachineTargetGroup>();
            }
        }
    }
}
