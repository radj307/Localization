using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Testing
{
#if DEBUG
    public static class DebugHelpers
    {
        #region Fields
        public static readonly string[] Words = { // random words from chatgpt:
            "apple", "banana", "cherry", "date", "elder", "fig", "grape", "honey", "ivy", "jam",
            "kite", "lemon", "melon", "nutmeg", "olive", "peach", "quince", "raisin", "spice", "thyme",
            "umbra", "vanilla", "water", "xanthic", "yellow", "zest", "almond", "breeze", "cocoa", "daisy",
            "echo", "fudge", "glow", "hazel", "iris", "jazz", "karma", "lily", "mango", "nova",
            "opal", "piano", "quasar", "raven", "silk", "truffle", "ultra", "velvet", "whisk", "xerox",
            "yoga", "zebra", "azure", "blitz", "cider", "dynamo", "ember", "flint", "glimpse", "humble",
            "ignite", "jubilee", "kiwi", "lush", "mirth", "noble", "oasis", "pounce", "quiver", "riddle",
            "serene", "twirl", "uplift", "vortex", "waltz", "xylophone", "yearn", "zephyr", "ambush", "bewitch",
            "cascade", "dazzle", "effervesce", "fable", "glisten", "harmony", "infinite", "jubilant", "kaleidoscope", "luminous",
            "mesmerize", "nirvana", "oceanic", "panorama", "quintessence", "radiant", "serendipity", "tranquil", "utopia", "vivid",
            "whimsical", "xanadu", "yonder", "zenith", "adorn", "bliss", "courage", "diligent", "eloquent", "felicity",
            "gracious", "harmony", "innocent", "jubilant", "keen", "lively", "magnificent", "noble", "optimistic", "peaceful",
            "quaint", "resilient", "serene", "triumphant", "uplifting", "vibrant", "wholesome", "xenial", "youthful", "zealous"
        };
        #endregion Fields

        #region Next
        /// <summary>
        /// Inclusive version of the <see cref="Random.Next"/> method.
        /// </summary>
        /// <param name="min">The minimum value that can be returned.</param>
        /// <param name="max">The maximum value that can be returned.</param>
        /// <returns>A random value from <paramref name="min"/> up to and including <paramref name="max"/>.</returns>
        public static int Next(int min, int max)
            => Random.Shared.Next(min, max + 1);
        /// <summary>
        /// Inclusive version of the <see cref="Random.Next"/> method.
        /// </summary>
        /// <param name="max">The maximum value that can be returned.</param>
        /// <returns>A random value from 0 up to and including <paramref name="max"/>.</returns>
        public static int Next(int max)
            => Random.Shared.Next(max + 1);
        #endregion Next

        #region NextBool
        public static bool NextBool()
            => Next(0, 1) == 0;
        #endregion NextBool

        #region NextWord
        public static string NextWord()
            => Words[Next(0, Words.Length - 1)];
        #endregion NextWord

        #region NextWords
        public static string[] NextWords(int min, int max)
        {
            int len = Next(min, max);
            string[] arr = new string[len];

            for (int i = 0; i < len; ++i)
            {
                arr[i] = NextWord();
            }

            return arr;
        }
        public static string[] NextWords(int max)
            => NextWords(0, max);
        #endregion NextWords
    }
    /// <summary>
    /// Helper class for comparing the approximate speeds of code snippets.
    /// </summary>
    public class DebugProfiler
    {
        #region Fields
        private readonly Stopwatch _stopwatch = new();
        #endregion Fields

        #region Methods

        #region Profile
        /// <summary>
        /// Profiles the specified <paramref name="action"/> and returns the elapsed time.
        /// </summary>
        /// <param name="action">A delegate containing the code to profile.</param>
        /// <param name="preAction">An action to perform prior to the <paramref name="action"/>, or <see langword="null"/>.</param>
        /// <param name="postAction">An action to perform after the <paramref name="action"/>, or <see langword="null"/>.</param>
        /// <returns>A <see cref="TimeSpan"/> containing the elapsed time.</returns>
        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public TimeSpan Profile(Action action, Action? preAction = null, Action? postAction = null)
        {
            _stopwatch.Reset();

            preAction?.Invoke();

            _stopwatch.Start();
            action.Invoke();
            _stopwatch.Stop();

            postAction?.Invoke();

            return _stopwatch.Elapsed;
        }
        /// <summary>
        /// Profiles the specified <paramref name="action"/> and returns the average elapsed time.
        /// </summary>
        /// <param name="count">The number of times to profile the <paramref name="action"/>.</param>
        /// <param name="action">A delegate containing the code to profile.</param>
        /// <param name="preAction">An action to perform prior to the <paramref name="action"/>, or <see langword="null"/>.</param>
        /// <param name="postAction">An action to perform after the <paramref name="action"/>, or <see langword="null"/>.</param>
        /// <returns>The average amount of elapsed time, as a <see cref="TimeSpan"/> instance.</returns>
        public TimeSpan Profile(int count, Action action, Action? preAction = null, Action? postAction = null)
        {
            return new TimeSpan((long)Math.Round(ProfileAll(count, action, preAction, postAction).Select(ts => ts.Ticks).Average(), 0));
        }
        /// <summary>
        /// Profiles the specified <paramref name="action"/> and returns the average elapsed time.
        /// </summary>
        /// <param name="count">The number of times to profile the <paramref name="action"/>.</param>
        /// <param name="action">A delegate containing the code to profile.</param>
        /// <param name="preAction">An action to perform prior to the <paramref name="action"/>, or <see langword="null"/>. The <see cref="int"/> parameter is the invocation counter.</param>
        /// <param name="postAction">An action to perform after the <paramref name="action"/>, or <see langword="null"/>. The <see cref="int"/> parameter is the invocation counter.</param>
        /// <returns>The average amount of elapsed time, as a <see cref="TimeSpan"/> instance.</returns>
        public TimeSpan Profile(int count, Action action, Action<int>? preAction, Action<int>? postAction)
        {
            return new TimeSpan((long)Math.Round(ProfileAll(count, action, preAction, postAction).Select(ts => ts.Ticks).Average(), 0));
        }
        #endregion Profile

        #region ProfileAll
        /// <summary>
        /// Profiles the specified <paramref name="action"/> and returns the elapsed times.
        /// </summary>
        /// <param name="count">The number of times to profile the <paramref name="action"/>.</param>
        /// <param name="action">A delegate containing the code to profile.</param>
        /// <param name="preAction">An action to perform prior to the <paramref name="action"/>, or <see langword="null"/>.</param>
        /// <param name="postAction">An action to perform after the <paramref name="action"/>, or <see langword="null"/>.</param>
        /// <returns>An array of <see cref="TimeSpan"/> instances containing the elapsed time of each run.</returns>
        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public TimeSpan[] ProfileAll(int count, Action action, Action? preAction = null, Action? postAction = null)
        {
            var t = new TimeSpan[count];

            for (int i = 0; i < count; ++i)
            {
                t[i] = Profile(action, preAction, postAction);
            }

            return t;
        }
        /// <summary>
        /// Profiles the specified <paramref name="action"/> and returns the elapsed times.
        /// </summary>
        /// <param name="count">The number of times to profile the <paramref name="action"/>.</param>
        /// <param name="action">A delegate containing the code to profile.</param>
        /// <param name="preAction">An action to perform prior to the <paramref name="action"/>, or <see langword="null"/>. The <see cref="int"/> parameter is the invocation counter.</param>
        /// <param name="postAction">An action to perform after the <paramref name="action"/>, or <see langword="null"/>. The <see cref="int"/> parameter is the invocation counter.</param>
        /// <returns>An array of <see cref="TimeSpan"/> instances containing the elapsed time of each run.</returns>
        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public TimeSpan[] ProfileAll(int count, Action action, Action<int>? preAction, Action<int>? postAction)
        {
            var t = new TimeSpan[count];

            for (int i = 0; i < count; ++i)
            {
                preAction?.Invoke(i);

                t[i] = Profile(action);

                postAction?.Invoke(i);
            }

            return t;
        }
        #endregion ProfileAll

        #region ProfileTicks
        /// <summary>
        /// Profiles the specified <paramref name="action"/> and returns the average elapsed time in ticks.
        /// </summary>
        /// <param name="count">The number of times to profile the <paramref name="action"/>.</param>
        /// <param name="action">A delegate containing the code to profile.</param>
        /// <param name="preAction">An action to perform prior to the <paramref name="action"/>, or <see langword="null"/>.</param>
        /// <param name="postAction">An action to perform after the <paramref name="action"/>, or <see langword="null"/>.</param>
        /// <returns>The average elapsed time, measured in ticks.</returns>
        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public double ProfileTicks(int count, Action action, Action? preAction = null, Action? postAction = null)
        {
            return ProfileAll(count, action, preAction, postAction).Select(ts => ts.Ticks).Average();
        }
        /// <summary>
        /// Profiles the specified <paramref name="action"/> and returns the average elapsed time in ticks.
        /// </summary>
        /// <param name="count">The number of times to profile the <paramref name="action"/>.</param>
        /// <param name="action">A delegate containing the code to profile.</param>
        /// <param name="preAction">An action to perform prior to the <paramref name="action"/>, or <see langword="null"/>. The <see cref="int"/> parameter is the invocation counter.</param>
        /// <param name="postAction">An action to perform after the <paramref name="action"/>, or <see langword="null"/>. The <see cref="int"/> parameter is the invocation counter.</param>
        /// <returns>The average elapsed time, measured in ticks.</returns>
        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public double ProfileTicks(int count, Action action, Action<int>? preAction, Action<int>? postAction)
        {
            return ProfileAll(count, action, preAction, postAction).Select(ts => ts.Ticks).Average();
        }
        #endregion ProfileTicks

        #region ProfileMicroseconds
        /// <summary>
        /// Profiles the specified <paramref name="action"/> and returns the average elapsed time in microseconds.
        /// </summary>
        /// <param name="count">The number of times to profile the <paramref name="action"/>.</param>
        /// <param name="action">A delegate containing the code to profile.</param>
        /// <param name="preAction">An action to perform prior to the <paramref name="action"/>, or <see langword="null"/>.</param>
        /// <param name="postAction">An action to perform after the <paramref name="action"/>, or <see langword="null"/>.</param>
        /// <returns>The average elapsed time, measured in microseconds.</returns>
        public double ProfileMicroseconds(int count, Action action, Action? preAction = null, Action? postAction = null)
        {
            double avgTicks = ProfileTicks(count, action, preAction, postAction);
            return avgTicks / Stopwatch.Frequency * 1000000;
        }
        /// <summary>
        /// Profiles the specified <paramref name="action"/> and returns the average elapsed time in microseconds.
        /// </summary>
        /// <param name="count">The number of times to profile the <paramref name="action"/>.</param>
        /// <param name="action">A delegate containing the code to profile.</param>
        /// <param name="preAction">An action to perform prior to the <paramref name="action"/>, or <see langword="null"/>. The <see cref="int"/> parameter is the invocation counter.</param>
        /// <param name="postAction">An action to perform after the <paramref name="action"/>, or <see langword="null"/>. The <see cref="int"/> parameter is the invocation counter.</param>
        /// <returns>The average elapsed time, measured in microseconds.</returns>
        public double ProfileMicroseconds(int count, Action action, Action<int>? preAction, Action<int>? postAction)
        {
            double avgTicks = ProfileTicks(count, action, preAction, postAction);
            return avgTicks / Stopwatch.Frequency * 1000000;
        }
        #endregion ProfileMicroseconds

        #region IndexOfFastestValue
        /// <summary>
        /// Finds the smallest value &amp; its index in the specified <paramref name="values"/> array.
        /// </summary>
        /// <typeparam name="T">The comparable type of the specified <paramref name="values"/>.</typeparam>
        /// <param name="values">Any number of values.</param>
        /// <returns>The fastest value and its index in <paramref name="values"/>.</returns>
        public static (T Value, int Index) Smallest<T>(params T[] values) where T : IComparable<T>
        {
            T smallestValue = default!;
            int smallestIndex = -1;

            for (int i = 0, i_max = values.Length; i < i_max; ++i)
            {
                T value = values[i];
                if (smallestIndex == -1 || value.CompareTo(smallestValue) < 0)
                {
                    smallestValue = value;
                    smallestIndex = i;
                }
            }

            return (smallestValue, smallestIndex);
        }
        /// <summary>
        /// Gets the index of the smallest value in the specified <paramref name="values"/> array.
        /// </summary>
        /// <typeparam name="T">The comparable type of the specified <paramref name="values"/>.</typeparam>
        /// <param name="values">Any number of values.</param>
        /// <returns>The index of the fastest value in <paramref name="values"/>.</returns>
        public static int IndexOfSmallest<T>(params T[] values) where T : IComparable<T>
            => Smallest(values).Index;
        /// <inheritdoc cref="Smallest{T}(T[])"/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        public (T Value, int Index) Fastest<T>(params T[] values) where T : IComparable<T> => Smallest(values);
        /// <inheritdoc cref="IndexOfSmallest{T}(T[])"/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        public int IndexOfFastest<T>(params T[] values) where T : IComparable<T> => IndexOfSmallest(values);
        #endregion IndexOfFastestValue

        #endregion Methods
    }
    public class DebugProfiler2
    {
        public DebugProfiler2(int count) => Count = count;
        public DebugProfiler2() { }

        private readonly Stopwatch _stopwatch = new();

        public int Count { get; set; } = 100000;

        [MethodImpl(MethodImplOptions.NoOptimization)]
        public TimeSpan ProfileOnce(Action code, Action? setup = null, Action? teardown = null)
        {
            _stopwatch.Reset();

            setup?.Invoke();

            _stopwatch.Start();
            code.Invoke();
            _stopwatch.Stop();

            teardown?.Invoke();

            return _stopwatch.Elapsed;
        }
        public double ProfileOnce<TUnit>(Action code, Action? setup = null, Action? teardown = null) where TUnit : ITimeUnit, new()
            => ProfileOnce(code, setup, teardown).Ticks / Stopwatch.Frequency * new TUnit().UnitTicksPerSecond;
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public TimeSpan Profile(Action code, Action<int>? setup = null, Action<int>? teardown = null)
        {
            var times = new long[Count];
            for (int i = 0; i < Count; ++i)
            {
                setup?.Invoke(i);

                times[i] = ProfileOnce(code).Ticks;

                teardown?.Invoke(i);
            }
            return new TimeSpan((long)Math.Round(times.Average()));
        }
        public double Profile<TUnit>(Action code, Action<int>? setup = null, Action<int>? teardown = null) where TUnit : ITimeUnit, new()
            => Profile(code, setup, teardown).Ticks / Stopwatch.Frequency * new TUnit().UnitTicksPerSecond;

        public DebugProfiler2 Profile(Action code, Action<int> setup, Action<int> teardown, out TimeSpan elapsed)
        {
            elapsed = Profile(code, setup, teardown);
            return this;
        }
        public DebugProfiler2 Profile(out TimeSpan elapsed, Action code, Action<int> setup, Action<int> teardown)
        {
            elapsed = Profile(code, setup, teardown);
            return this;
        }
        public DebugProfiler2 Profile<TUnit>(Action code, Action<int> setup, Action<int> teardown, out double elapsed) where TUnit : ITimeUnit, new()
        {
            elapsed = Profile<TUnit>(code, setup, teardown);
            return this;
        }
        public DebugProfiler2 Profile<TUnit>(out double elapsed, Action code, Action<int> setup, Action<int> teardown) where TUnit : ITimeUnit, new()
        {
            elapsed = Profile<TUnit>(code, setup, teardown);
            return this;
        }
        public DebugProfiler2 Profile(Action code, Action<int> setup, out TimeSpan elapsed)
        {
            elapsed = Profile(code, setup);
            return this;
        }
        public DebugProfiler2 Profile(out TimeSpan elapsed, Action code, Action<int> setup)
        {
            elapsed = Profile(code, setup);
            return this;
        }
        public DebugProfiler2 Profile<TUnit>(Action code, Action<int> setup, out double elapsed) where TUnit : ITimeUnit, new()
        {
            elapsed = Profile<TUnit>(code, setup);
            return this;
        }
        public DebugProfiler2 Profile<TUnit>(out double elapsed, Action code, Action<int> setup) where TUnit : ITimeUnit, new()
        {
            elapsed = Profile<TUnit>(code, setup);
            return this;
        }
        public DebugProfiler2 Profile(Action code, out TimeSpan elapsed)
        {
            elapsed = Profile(code);
            return this;
        }
        public DebugProfiler2 Profile(out TimeSpan elapsed, Action code)
        {
            elapsed = Profile(code);
            return this;
        }
        public DebugProfiler2 Profile<TUnit>(Action code, out double elapsed) where TUnit : ITimeUnit, new()
        {
            elapsed = Profile<TUnit>(code);
            return this;
        }
        public DebugProfiler2 Profile<TUnit>(out double elapsed, Action code) where TUnit : ITimeUnit, new()
        {
            elapsed = Profile<TUnit>(code);
            return this;
        }

        public static DebugProfiler2 WithCount(int runCount) => new(runCount);

        public interface ITimeUnit
        {
            long UnitTicksPerSecond { get; }
        }
    }
    public readonly struct Milliseconds : DebugProfiler2.ITimeUnit { public long UnitTicksPerSecond => TimeSpan.TicksPerMillisecond; }
    public readonly struct Microseconds : DebugProfiler2.ITimeUnit { public long UnitTicksPerSecond => 1000000; }
    public readonly struct Nanoseconds : DebugProfiler2.ITimeUnit { public long UnitTicksPerSecond => 1000000000; }
#endif
}
