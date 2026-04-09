using UnityEngine;
using Core.Scripts.Extensions;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc
{
    public class PlayerAssistLineView : MonoBehaviour
    {
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private int _pointsCount = 30;

        public void OnCreated()
        {
            _lineRenderer.positionCount = _pointsCount;
        }

        public void UpdateLine(Vector2 headPos, Vector2 direction, float angularVelocity, float velocityLength, float lineLength)
        {
            if (_pointsCount <= 1 || Mathf.Approximately(lineLength, 0f))
            {
                _lineRenderer.positionCount = 0;
                return;
            }

            _lineRenderer.positionCount = _pointsCount;

            float distanceStep = lineLength / (_pointsCount - 1);

            float speed = Mathf.Max(velocityLength, 5f); // Use a minimum speed to always show a line
            float timeStep = distanceStep / speed;

            Vector2 currentPos = headPos;
            Vector2 currentDir = direction;

            for (int i = 0; i < _pointsCount; i++)
            {
                _lineRenderer.SetPosition(i, new Vector3(currentPos.x, currentPos.y, 0f));

                // Move forward
                currentPos += currentDir * distanceStep;

                // Rotate direction based on angular velocity (degrees per second)
                if (Mathf.Abs(angularVelocity) > 0.01f)
                {
                    float rotationAngle = angularVelocity * timeStep;
                    currentDir = currentDir.Rotate(rotationAngle);
                }
            }
        }

        public void SetColor(Color color)
        {
            if (_lineRenderer != null)
            {
                _lineRenderer.startColor = color;
                _lineRenderer.endColor = color;
            }
        }

        public void SetIsShown(bool isShown)
        {
            gameObject.SetActive(isShown);
        }
    }
}
