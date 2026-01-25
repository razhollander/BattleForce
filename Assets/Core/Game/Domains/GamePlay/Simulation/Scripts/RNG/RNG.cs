using System;
using System.Collections.Generic;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.RNG
{
    public static class RNG
    {
        private static Random _rng;
        private static int _seed;
        private static bool _initialized;

        /// <summary>
        /// Call once on server startup.
        /// </summary>
        public static void Init(int seed)
        {
            _seed = seed;
            _rng = new Random(seed);
            _initialized = true;
        }

        public static int Seed
        {
            get
            {
                EnsureInit();
                return _seed;
            }
        }

        /// <summary>
        /// Optional: reset back to the original seed.
        /// </summary>
        public static void Reset()
        {
            EnsureInit();
            _rng = new Random(_seed);
        }

        private static void EnsureInit()
        {
            if (!_initialized || _rng == null)
                throw new InvalidOperationException("RandomService.Init(seed) must be called before use.");
        }

        public static int NextInt() { EnsureInit(); return _rng!.Next(); }

        public static int NextInt(int maxExclusive) { EnsureInit(); return _rng!.Next(maxExclusive); }

        public static int NextInt(int minInclusive, int maxExclusive) { EnsureInit(); return _rng!.Next(minInclusive, maxExclusive); }

        public static double NextDouble() { EnsureInit(); return _rng!.NextDouble(); } // [0, 1)

        public static float NextFloat() { EnsureInit(); return (float)_rng!.NextDouble(); } // [0, 1)

        public static float NextFloat(float minInclusive, float maxInclusive)
        {
            EnsureInit();
            if (maxInclusive < minInclusive) throw new ArgumentException("maxInclusive must be >= minInclusive.");
            return minInclusive + (maxInclusive - minInclusive) * (float)_rng!.NextDouble();
        }

        public static bool NextBool() { EnsureInit(); return (_rng!.Next() & 1) == 0; }

        public static void Shuffle<T>(IList<T> list)
        {
            EnsureInit();
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _rng!.Next(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        public static T Pick<T>(IList<T> list)
        {
            EnsureInit();
            if (list == null || list.Count == 0) throw new ArgumentException("List is null or empty.");
            return list[_rng!.Next(list.Count)];
        }
    }
}
