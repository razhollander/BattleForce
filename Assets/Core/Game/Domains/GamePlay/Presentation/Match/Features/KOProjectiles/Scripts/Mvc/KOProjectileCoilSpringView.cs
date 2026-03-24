using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.KOProjectiles.Scripts.Mvc
{
    public class KOProjectileCoilSpringView : MonoBehaviour
    {
        [SerializeField] private float _wireThickness = 0.2f;
        [SerializeField] private float _coilWidth = 1f; // How wide the zig-zags are
        [SerializeField] private int _numberOfCoils = 8;
        [SerializeField] private int _pointsPerCoil = 15; // Higher = smoother curves
        [SerializeField] private LineRenderer _lineRenderer;

        public void UpdateEndPoints(Vector2 startPoint, Vector2 endPoint)
        {
            // 1. Lock the thickness
            _lineRenderer.startWidth = _wireThickness;
            _lineRenderer.endWidth = _wireThickness;
        
            int totalPoints = _numberOfCoils * _pointsPerCoil;
            _lineRenderer.positionCount = totalPoints;
            
            // Find the direction and length between the two points
            Vector3 direction = (endPoint - startPoint);
            Vector3 normalizedDir = direction.normalized;
        
            // Calculate the perpendicular vector (for the coil's sideways bounce)
            // This math rotates the direction vector 90 degrees in 2D
            Vector3 perpendicular = new Vector3(-normalizedDir.y, normalizedDir.x, 0);
        
            // 2. Plot the points using a Sine Wave
            for (int i = 0; i < totalPoints; i++)
            {
                float t = (float)i / (totalPoints - 1); // Goes from 0.0 to 1.0
            
                // The straight line position
                Vector3 basePosition = Vector3.Lerp(startPoint, endPoint, t);
            
                // The sine wave offset
                float currentAngle = t * _numberOfCoils * Mathf.PI * 2f;
                Vector3 sidewaysOffset = perpendicular * Mathf.Sin(currentAngle) * _coilWidth;
            
                _lineRenderer.SetPosition(i, basePosition + sidewaysOffset);
            }
        }
    }
}
