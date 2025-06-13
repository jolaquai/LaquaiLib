namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="DateTime"/>, <see cref="DateTimeOffset"/>, <see cref="DateOnly"/> and <see cref="TimeOnly"/> types.
/// </summary>
public static class TimingExtensions
{
    extension(DateTime dateTime)
    {
        /// <summary>
        /// Advances the given <see cref="DateTime"/> to the next weekday.
        /// </summary>
        /// <param name="startAt">The <see cref="DateTime"/> to start at.</param>
        /// <param name="timeComponent">The time of day to set the <see cref="DateTime"/> to. Defaults to <see langword="null"/>, which leaves the time component unchanged.</param>
        /// <param name="includeSaturdays">Whether to include Saturdays as weekdays. Defaults to <see langword="false"/>.</param>
        /// <returns>The next weekday after the given <see cref="DateTime"/>.</returns>
        public DateTime NextWeekday(TimeOnly? timeComponent = null, bool includeSaturdays = false)
        {
            if (timeComponent is not null)
            {
                dateTime = new DateTime(DateOnly.FromDateTime(dateTime), timeComponent.Value);
            }

            var nextWeekday = dateTime.AddDays(1);
            while (nextWeekday <= dateTime || (nextWeekday.DayOfWeek is DayOfWeek.Saturday && !includeSaturdays) || nextWeekday.DayOfWeek is DayOfWeek.Sunday)
            {
                nextWeekday = nextWeekday.AddDays(1);
            }
            return nextWeekday;
        }
        /// <summary>
        /// Advances the given <see cref="DateTime"/> to the next weekday.
        /// </summary>
        /// <param name="startAt">The <see cref="DateTime"/> to start at.</param>
        /// <param name="timeComponent">Another <see cref="DateTime"/> instance the time component of which to set the resulting <see cref="DateTime"/> to. Defaults to <see langword="null"/>, which leaves the time component unchanged.</param>
        /// <param name="includeSaturdays">Whether to include Saturdays as weekdays. Defaults to <see langword="false"/>.</param>
        /// <returns>The next weekday after the given <see cref="DateTime"/>.</returns>
        public DateTime NextWeekday(DateTime? timeComponent = null, bool includeSaturdays = false)
            => NextWeekday(dateTime, timeComponent is not null ? TimeOnly.FromDateTime(timeComponent.Value) : null, includeSaturdays);

        /// <summary>
        /// Returns an awaiter that can be used to await a <see cref="Task"/> that completes when the specified <see cref="DateTime"/> is reached.
        /// </summary>
        /// <param name="dateTime">The <see cref="DateTime"/> to wait for.</param>
        /// <returns>A <see cref="TaskAwaiter"/> instance is used to await the <see cref="Task"/>.</returns>
        public TaskAwaiter GetAwaiter()
        {
            var now = DateTime.Now;
            if (dateTime > now)
            {
                return Task.Delay(dateTime - now).GetAwaiter();
            }
            return Task.CompletedTask.GetAwaiter();
        }
    }

    extension(DateTimeOffset dateTimeOffset)
    {
        /// <summary>
        /// Returns an awaiter that can be used to await a <see cref="Task"/> that completes when the specified <see cref="DateTimeOffset"/> is reached.
        /// </summary>
        /// <param name="dateTimeOffset">The <see cref="DateTimeOffset"/> to wait for.</param>
        /// <returns>A <see cref="TaskAwaiter"/> instance is used to await the <see cref="Task"/>.</returns>
        public TaskAwaiter GetAwaiter()
        {
            var now = DateTimeOffset.Now;
            if (dateTimeOffset > now)
            {
                return Task.Delay(dateTimeOffset - now).GetAwaiter();
            }
            return Task.CompletedTask.GetAwaiter();
        }
    }
    extension(DateOnly dateOnly)
    {
        /// <summary>
        /// Returns an awaiter that can be used to await a <see cref="Task"/> that completes when the specified <see cref="DateOnly"/> at midnight is reached.
        /// </summary>
        /// <param name="dateOnly">The <see cref="DateOnly"/> to wait for.</param>
        /// <returns>A <see cref="TaskAwaiter"/> instance is used to await the <see cref="Task"/>.</returns>
        public TaskAwaiter GetAwaiter() => GetAwaiter(dateOnly.ToDateTime(TimeOnly.MinValue));
    }
    extension(TimeOnly timeOnly)
    {
        /// <summary>
        /// Returns an awaiter that can be used to await a <see cref="Task"/> that completes when the specified <see cref="TimeOnly"/> on the current day is reached.
        /// </summary>
        /// <param name="timeOnly">The <see cref="TimeOnly"/> to wait for.</param>
        /// <returns>A <see cref="TaskAwaiter"/> instance is used to await the <see cref="Task"/>.</returns>
        public TaskAwaiter GetAwaiter() => GetAwaiter(DateTime.Today.AddTicks(timeOnly.Ticks));
    }
    extension(TimeSpan timeSpan)
    {
        /// <summary>
        /// Returns an awaiter that can be used to await a <see cref="Task"/> that completes when the specified <see cref="TimeSpan"/>, starting from now, has passed.
        /// </summary>
        /// <param name="timeSpan">The <see cref="TimeSpan"/> to wait for.</param>
        /// <returns>A <see cref="TaskAwaiter"/> instance is used to await the <see cref="Task"/>.</returns>
        /// <remarks>
        /// This is essentially just a shorthand for creating a <see cref="Task.Delay(TimeSpan)"/> call.
        /// </remarks>
        public TaskAwaiter GetAwaiter() => Task.Delay(timeSpan).GetAwaiter();
    }
}
