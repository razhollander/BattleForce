using System.Collections.Generic;
using UnityEngine;

namespace CoreDomain.Scripts.Mvc.WorldCamera
{
    public static class CameraFramingUtils
    {
        private const float PERCENT_TO_FRACTION = 0.01f;
        // Floor for the fraction of the screen the bounding box may occupy, guarding the divisions below
        // against non-positive margins if callers pass out-of-range percentages.
        private const float MIN_SCREEN_FRACTION = 0.05f;

        // Axis-aligned bounds enclosing every target expanded by its radius.
        public static void CalculateTargetsBounds(IReadOnlyList<CameraTarget> targets, out Vector2 center, out Vector2 extents)
        {
            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);

            for (var index = 0; index < targets.Count; index++)
            {
                var target = targets[index];
                var position = target.Transform.position;
                var radius = target.Radius;
                min.x = Mathf.Min(min.x, position.x - radius);
                min.y = Mathf.Min(min.y, position.y - radius);
                max.x = Mathf.Max(max.x, position.x + radius);
                max.y = Mathf.Max(max.y, position.y + radius);
            }

            center = (min + max) * 0.5f;
            extents = (max - min) * 0.5f;
        }

        // Orthographic size that fits the bounding box on screen, keeping the requested edge margin (and extra
        // bottom margin), never zooming closer than minOrthographicSize.
        public static float CalculateFramedOrthographicSize(Vector2 extents, float aspect, float edgeMarginPercentage, float extraBottomPercentage, float minOrthographicSize)
        {
            var boundingBoxScreenFraction = Mathf.Max(1f - edgeMarginPercentage * PERCENT_TO_FRACTION, MIN_SCREEN_FRACTION);
            var boundingBoxHeightFraction = Mathf.Max(boundingBoxScreenFraction - extraBottomPercentage * PERCENT_TO_FRACTION, MIN_SCREEN_FRACTION);
            var sizeForHeight = extents.y / boundingBoxHeightFraction;
            var sizeForWidth = extents.x / aspect / boundingBoxScreenFraction;
            var desiredSize = Mathf.Max(sizeForHeight, sizeForWidth);
            return Mathf.Max(desiredSize, minOrthographicSize);
        }

        // World-space downward shift that pushes the framed targets up, leaving the extra margin at the bottom.
        public static float CalculateBottomWorldOffset(float orthographicSize, float extraBottomPercentage)
        {
            return orthographicSize * extraBottomPercentage * PERCENT_TO_FRACTION;
        }

        // Largest orthographic size whose frustum still fits inside the given world bounds.
        public static float CalculateMaxOrthographicSizeInBounds(Vector2 boundsMin, Vector2 boundsMax, float aspect)
        {
            var maxSizeByHeight = (boundsMax.y - boundsMin.y) * 0.5f;
            var maxSizeByWidth = (boundsMax.x - boundsMin.x) * 0.5f / aspect;
            return Mathf.Min(maxSizeByHeight, maxSizeByWidth);
        }

        // Clamps a camera centre so its orthographic frustum stays within the given world bounds.
        // Assumes the frustum already fits (orthographic size clamped with CalculateMaxOrthographicSizeInBounds beforehand).
        public static Vector2 ClampPositionToBounds(Vector2 position, float orthographicSize, float aspect, Vector2 boundsMin, Vector2 boundsMax)
        {
            var halfHeight = orthographicSize;
            var halfWidth = orthographicSize * aspect;
            position.x = Mathf.Clamp(position.x, boundsMin.x + halfWidth, boundsMax.x - halfWidth);
            position.y = Mathf.Clamp(position.y, boundsMin.y + halfHeight, boundsMax.y - halfHeight);
            return position;
        }

        // Frame-rate independent exponential damping toward target. damping is roughly the time constant in seconds.
        public static float Damp(float current, float target, float damping, float deltaTime)
        {
            if (damping <= 0f)
            {
                return target;
            }

            var lerpFactor = 1f - Mathf.Exp(-deltaTime / damping);
            return Mathf.Lerp(current, target, lerpFactor);
        }
    }
}
