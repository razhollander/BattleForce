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
            _targetGroup.AddMember(target, weight, radius);
        }

        public void RemoveTarget(Transform target)
        {
            _targetGroup.RemoveMember(target);
        }

        public void ManualUpdate()
        {
            // Cinemachine updates automatically, but we can add manual logic here if needed
        }
    }
}
