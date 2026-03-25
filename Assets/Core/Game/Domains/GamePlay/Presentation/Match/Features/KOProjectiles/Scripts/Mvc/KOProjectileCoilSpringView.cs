using CoreDomain.Scripts.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.KOProjectiles.Scripts.Mvc
{
    public class KOProjectileCoilSpringView : MonoBehaviour
    {
        [SerializeField] private float _wireThickness = 0.2f;
        [SerializeField] private float _coilWidth = 1f;
        [SerializeField] private int _numberOfCoils = 8;
        [SerializeField] private int _pointsPerCoil = 15;
        [SerializeField] private LineRenderer _lineRenderer;

        public void UpdateEndPoints(Vector2 startPoint, Vector2 endPoint)
        {
            var totalPoints = _numberOfCoils * _pointsPerCoil;
            ApplyLineThicknessAndCount(totalPoints);
            var perpendicularVector = MathUtils.CalculatePerpendicularDirection(startPoint, endPoint);
            SetSpringPoints(startPoint, endPoint, totalPoints, perpendicularVector);
        }

        private void ApplyLineThicknessAndCount(int totalPoints)
        {
            _lineRenderer.startWidth = _wireThickness;
            _lineRenderer.endWidth = _wireThickness;
            _lineRenderer.positionCount = totalPoints;
        }

        private void SetSpringPoints(Vector2 startPoint, Vector2 endPoint, int totalPoints, Vector3 perpendicular)
        {
            for (int i = 0; i < totalPoints; i++)
            {
                float t = (float)i / (totalPoints - 1);
                var finalPosition = CalculatePointPositionAlongWave(startPoint, endPoint, t, perpendicular);
                _lineRenderer.SetPosition(i, finalPosition);
            }
        }

        private Vector3 CalculatePointPositionAlongWave(Vector2 startPoint, Vector2 endPoint, float t, Vector3 perpendicular)
        {
            var basePosition = Vector3.Lerp(startPoint, endPoint, t);
            float currentAngle = t * _numberOfCoils * Mathf.PI * 2f;
            var sidewaysOffset = perpendicular * Mathf.Sin(currentAngle) * _coilWidth;
            return basePosition + sidewaysOffset;
        }
    }
}