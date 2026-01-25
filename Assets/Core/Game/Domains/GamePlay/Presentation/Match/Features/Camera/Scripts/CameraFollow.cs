using System.Collections.Generic;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Camera.Scripts
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private List<Transform> _targets;

        [SerializeField] private Vector3 _offset;

        [SerializeField] private float _smoothTime = 0.5f;
        [SerializeField] private float _minZoom = 0.5f;
        [SerializeField] private float _maxZoom = 0.5f;
        [SerializeField] private float _zoomLimiter = 50f;

        private Vector3 _velocity;

        private UnityEngine.Camera _camera;
        // Start is called before the first frame update
        void Start()
        {
            _camera = GetComponent<UnityEngine.Camera>();
        }

        private void LateUpdate()
        {
            if (_targets.Count == 0) return;
            Move();
            Zoom();
        }

        private void Move()
        {
            var centerPoint = GetCenterPoint();
            var newPosition = centerPoint + _offset;
            transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref _velocity, _smoothTime);
        }

        private float GetGreatestDistance()
        {
            var bounds = new Bounds(_targets[0].position, Vector3.zero);

            for (int i = 0; i < _targets.Count; i++)
            {
                bounds.Encapsulate(_targets[i].position);
            }
        
            return bounds.size.x;
        }
        private void Zoom()
        {
            var newZoom = Mathf.Lerp(_maxZoom, _minZoom, _zoomLimiter/GetGreatestDistance());
            _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, newZoom, Time.deltaTime);
        }
    
        private Vector3 GetCenterPoint()
        {
            if (_targets.Count == 1)
            {
                return _targets[0].position;
            }

            var bounds = new Bounds(_targets[0].position, Vector3.zero);

            for (int i = 0; i < _targets.Count; i++)
            {
                bounds.Encapsulate(_targets[i].position);
            }

            return bounds.center;
        }
    }
}
