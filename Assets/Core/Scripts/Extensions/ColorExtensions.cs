using UnityEngine;

namespace Core.Scripts.Extensions
{
    public static class ColorExtensions
    {
        /// <summary>
        /// Darkens the color by a specified percentage.
        /// </summary>
        /// <param name="color">The original Unity Color.</param>
        /// <param name="darkenPercent">Value from 0 to 1 (e.g., 0.2f is 20% darker).</param>
        /// <returns>A darker version of the color.</returns>
        public static Color Darken(this Color color, float darkenPercent)
        {
            darkenPercent = Mathf.Clamp01(darkenPercent);
            var factor = 1.0f - darkenPercent;

            // In Unity, we can multiply the RGB part directly by a float
            var darkerColor = new Color(
                color.r * factor,
                color.g * factor,
                color.b * factor,
                color.a
            );

            return darkerColor;
        }
    }
}