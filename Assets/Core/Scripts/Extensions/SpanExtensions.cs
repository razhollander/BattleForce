using System;
using System.Runtime.CompilerServices;

namespace Core.Scripts.Extensions
{
    public static partial class LinqExtensions
    {
        // ------------------------------------------------------------
        // ForEach
        // ------------------------------------------------------------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ForEach<T>(this ReadOnlySpan<T> span, Action<T> action)
        {
            // Note: Action<T> is a delegate; avoid capturing lambdas to keep it allocation-free.
            for (int i = 0; i < span.Length; i++)
                action(span[i]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ForEach<T>(this Span<T> span, ActionRef<T> action)
        {
            for (int i = 0; i < span.Length; i++)
                action(ref span[i]);
        }

        public delegate void ActionRef<T>(ref T value);

        // ------------------------------------------------------------
        // Any / All
        // ------------------------------------------------------------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Any<T>(this ReadOnlySpan<T> span, Func<T, bool> predicate)
        {
            for (int i = 0; i < span.Length; i++)
                if (predicate(span[i]))
                    return true;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Any<T>(this ReadOnlySpan<T> span)
            => span.Length != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool All<T>(this ReadOnlySpan<T> span, Func<T, bool> predicate)
        {
            for (int i = 0; i < span.Length; i++)
                if (!predicate(span[i]))
                    return false;
            return true;
        }

        // ------------------------------------------------------------
        // Count / CountWhere
        // ------------------------------------------------------------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountWhere<T>(this ReadOnlySpan<T> span, Func<T, bool> predicate)
        {
            int count = 0;
            for (int i = 0; i < span.Length; i++)
                if (predicate(span[i]))
                    count++;
            return count;
        }

        // ------------------------------------------------------------
        // Find / FindIndex / First / FirstOrDefault
        // ------------------------------------------------------------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FindIndex<T>(this ReadOnlySpan<T> span, Func<T, bool> predicate)
        {
            for (int i = 0; i < span.Length; i++)
                if (predicate(span[i]))
                    return i;
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFind<T>(this ReadOnlySpan<T> span, Func<T, bool> predicate, out T value)
        {
            for (int i = 0; i < span.Length; i++)
            {
                var v = span[i];
                if (predicate(v))
                {
                    value = v;
                    return true;
                }
            }

            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T First<T>(this ReadOnlySpan<T> span)
        {
            if (span.Length == 0)
                throw new InvalidOperationException("Span is empty.");
            return span[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T FirstOrDefault<T>(this ReadOnlySpan<T> span)
            => span.Length == 0 ? default : span[0];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T FirstOrDefault<T>(this ReadOnlySpan<T> span, Func<T, bool> predicate)
        {
            for (int i = 0; i < span.Length; i++)
            {
                var v = span[i];
                if (predicate(v))
                    return v;
            }
            return default;
        }

        // ------------------------------------------------------------
        // Min / Max (requires IComparable<T>)
        // ------------------------------------------------------------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Min<T>(this ReadOnlySpan<T> span) where T : IComparable<T>
        {
            if (span.Length == 0)
                throw new InvalidOperationException("Span is empty.");

            T min = span[0];
            for (int i = 1; i < span.Length; i++)
            {
                var v = span[i];
                if (v.CompareTo(min) < 0)
                    min = v;
            }
            return min;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Max<T>(this ReadOnlySpan<T> span) where T : IComparable<T>
        {
            if (span.Length == 0)
                throw new InvalidOperationException("Span is empty.");

            T max = span[0];
            for (int i = 1; i < span.Length; i++)
            {
                var v = span[i];
                if (v.CompareTo(max) > 0)
                    max = v;
            }
            return max;
        }

        // ------------------------------------------------------------
        // Sum / Average (specialized for common numeric types)
        // ------------------------------------------------------------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Sum(this ReadOnlySpan<int> span)
        {
            int sum = 0;
            for (int i = 0; i < span.Length; i++)
                sum += span[i];
            return sum;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sum(this ReadOnlySpan<float> span)
        {
            float sum = 0f;
            for (int i = 0; i < span.Length; i++)
                sum += span[i];
            return sum;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Sum(this ReadOnlySpan<double> span)
        {
            double sum = 0d;
            for (int i = 0; i < span.Length; i++)
                sum += span[i];
            return sum;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Average(this ReadOnlySpan<int> span)
        {
            if (span.Length == 0)
                throw new InvalidOperationException("Span is empty.");
            return (float)span.Sum() / span.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Average(this ReadOnlySpan<float> span)
        {
            if (span.Length == 0)
                throw new InvalidOperationException("Span is empty.");
            return span.Sum() / span.Length;
        }

        // ------------------------------------------------------------
        // Where / Select (copy to buffer)
        // ------------------------------------------------------------
        // Spans cannot return lazy enumerables. The fast pattern is:
        // - Caller provides a buffer Span<TOut>
        // - Method fills it and returns count written

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WhereTo<T>(this ReadOnlySpan<T> span, Span<T> destination, Func<T, bool> predicate)
        {
            int written = 0;
            for (int i = 0; i < span.Length; i++)
            {
                var v = span[i];
                if (predicate(v))
                {
                    if ((uint)written >= (uint)destination.Length)
                        return written;

                    destination[written++] = v;
                }
            }
            return written;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SelectTo<TIn, TOut>(this ReadOnlySpan<TIn> span, Span<TOut> destination, Func<TIn, TOut> selector)
        {
            int written = Math.Min(span.Length, destination.Length);
            for (int i = 0; i < written; i++)
                destination[i] = selector(span[i]);
            return written;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SelectWhereTo<TIn, TOut>(
            this ReadOnlySpan<TIn> span,
            Span<TOut> destination,
            Func<TIn, bool> predicate,
            Func<TIn, TOut> selector)
        {
            int written = 0;
            for (int i = 0; i < span.Length; i++)
            {
                var v = span[i];
                if (!predicate(v)) continue;

                if ((uint)written >= (uint)destination.Length)
                    return written;

                destination[written++] = selector(v);
            }
            return written;
        }

        // ------------------------------------------------------------
        // Contains / IndexOf (fast)
        // ------------------------------------------------------------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<T>(this ReadOnlySpan<T> span, T value) where T : IEquatable<T>
        {
            for (int i = 0; i < span.Length; i++)
                if (span[i].Equals(value))
                    return true;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this ReadOnlySpan<T> span, T value) where T : IEquatable<T>
        {
            for (int i = 0; i < span.Length; i++)
                if (span[i].Equals(value))
                    return i;
            return -1;
        }
    }
}
