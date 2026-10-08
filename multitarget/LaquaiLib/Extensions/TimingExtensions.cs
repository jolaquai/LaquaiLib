namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extensions for the <see cref="DateTime"/>, <see cref="DateTimeOffset"/>, <see cref="DateOnly"/> and <see cref="TimeOnly"/> types.
/// </summary>
public static class TimingExtensions
{
    extension(in DateTime dateTime)
    {
        /// <summary>
        /// Advances the given <see cref="DateTime"/> to the next weekday.
        /// </summary>
        /// <param name="timeComponent">The time of day to set the <see cref="DateTime"/> to. Defaults to <see langword="null"/>, which leaves the time component unchanged.</param>
        /// <param name="includeSaturdays">Whether to include Saturdays as weekdays. Defaults to <see langword="false"/>.</param>
        /// <returns>The next weekday after the given <see cref="DateTime"/>.</returns>
        public DateTime NextWeekday(TimeOnly? timeComponent = null, bool includeSaturdays = false)
        {
            var dt = dateTime;
            if (timeComponent is not null)
                dt = new DateTime(DateOnly.FromDateTime(dt), timeComponent.Value);

            var nextWeekday = dt.AddDays(1);
            while (nextWeekday <= dt || (nextWeekday.DayOfWeek is DayOfWeek.Saturday && !includeSaturdays) || nextWeekday.DayOfWeek is DayOfWeek.Sunday)
                nextWeekday = nextWeekday.AddDays(1);
            return nextWeekday;
        }
        /// <summary>
        /// Advances the given <see cref="DateTime"/> to the next weekday.
        /// </summary>
        /// <param name="timeComponent">Another <see cref="DateTime"/> instance the time component of which to set the resulting <see cref="DateTime"/> to. Defaults to <see langword="null"/>, which leaves the time component unchanged.</param>
        /// <param name="includeSaturdays">Whether to include Saturdays as weekdays. Defaults to <see langword="false"/>.</param>
        /// <returns>The next weekday after the given <see cref="DateTime"/>.</returns>
        public DateTime NextWeekday(DateTime? timeComponent = null, bool includeSaturdays = false)
            => NextWeekday(dateTime, timeComponent is not null ? TimeOnly.FromDateTime(timeComponent.Value) : null, includeSaturdays);

        /// <summary>
        /// Returns an awaiter that can be used to await a <see cref="Task"/> that completes when the specified <see cref="DateTime"/> is reached.
        /// </summary>
        /// <returns>A <see cref="TaskAwaiter"/> instance is used to await the <see cref="Task"/>.</returns>
        public TaskAwaiter GetAwaiter()
        {
            var now = DateTime.Now;
            if (dateTime > now)
                return Task.Delay(dateTime - now).GetAwaiter();
            return Task.CompletedTask.GetAwaiter();
        }
    }

    extension(in DateTimeOffset dateTimeOffset)
    {
        /// <summary>
        /// Returns an awaiter that can be used to await a <see cref="Task"/> that completes when the specified <see cref="DateTimeOffset"/> is reached.
        /// </summary>
        /// <returns>A <see cref="TaskAwaiter"/> instance is used to await the <see cref="Task"/>.</returns>
        public TaskAwaiter GetAwaiter()
        {
            var now = DateTimeOffset.Now;
            if (dateTimeOffset > now)
                return Task.Delay(dateTimeOffset - now).GetAwaiter();
            return Task.CompletedTask.GetAwaiter();
        }
    }
    extension(in DateOnly dateOnly)
    {
        /// <summary>
        /// Returns an awaiter that can be used to await a <see cref="Task"/> that completes when the specified <see cref="DateOnly"/> at midnight is reached.
        /// </summary>
        /// <returns>A <see cref="TaskAwaiter"/> instance is used to await the <see cref="Task"/>.</returns>
        public TaskAwaiter GetAwaiter() => GetAwaiter(dateOnly.ToDateTime(TimeOnly.MinValue));
    }
    extension(in TimeOnly timeOnly)
    {
        /// <summary>
        /// Returns an awaiter that can be used to await a <see cref="Task"/> that completes when the specified <see cref="TimeOnly"/> on the current day is reached.
        /// </summary>
        /// <returns>A <see cref="TaskAwaiter"/> instance is used to await the <see cref="Task"/>.</returns>
        public TaskAwaiter GetAwaiter() => GetAwaiter(DateTime.Today.AddTicks(timeOnly.Ticks));
    }
    extension(in TimeSpan timeSpan)
    {
        /// <summary>
        /// Returns an awaiter that can be used to await a <see cref="Task"/> that completes when the specified <see cref="TimeSpan"/>, starting from now, has passed.
        /// </summary>
        /// <returns>A <see cref="TaskAwaiter"/> instance is used to await the <see cref="Task"/>.</returns>
        /// <remarks>
        /// This is essentially just a shorthand for creating a <see cref="Task.Delay(TimeSpan)"/> call.
        /// </remarks>
        public TaskAwaiter GetAwaiter() => Task.Delay(timeSpan).GetAwaiter();
    }
}
