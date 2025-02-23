using System.Runtime.CompilerServices;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="DateTime"/>, <see cref="DateTimeOffset"/>, <see cref="DateOnly"/> and <see cref="TimeOnly"/> types.
/// </summary>
public static class TimingExtensions
{
    /// <summary>
    /// Advances the given <see cref="DateTime"/> to the next weekday.
    /// </summary>
    /// <param name="startAt">The <see cref="DateTime"/> to start at.</param>
    /// <param name="timeComponent">The time of day to set the <see cref="DateTime"/> to. Defaults to <see langword="null"/>, which leaves the time component unchanged.</param>
    /// <param name="includeSaturdays">Whether to include Saturdays as weekdays. Defaults to <see langword="false"/>.</param>
    /// <returns>The next weekday after the given <see cref="DateTime"/>.</returns>
    public static DateTime NextWeekday(this DateTime startAt, TimeOnly? timeComponent = null, bool includeSaturdays = false)
    {
        if (timeComponent is not null)
        {
            startAt = new DateTime(DateOnly.FromDateTime(startAt), timeComponent.Value);
        }

        var nextWeekday = startAt.AddDays(1);
        while (nextWeekday <= startAt || (nextWeekday.DayOfWeek is DayOfWeek.Saturday && !includeSaturdays) || nextWeekday.DayOfWeek is DayOfWeek.Sunday)
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
    public static DateTime NextWeekday(this DateTime startAt, DateTime? timeComponent = null, bool includeSaturdays = false)
        => NextWeekday(startAt, timeComponent is not null ? TimeOnly.FromDateTime(timeComponent.Value) : null, includeSaturdays);

    /// <summary>
    /// Returns an awaiter that can be used to await a <see cref="Task"/> that completes when the specified <see cref="DateTime"/> is reached.
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTime"/> to wait for.</param>
    /// <returns>A <see cref="TaskAwaiter"/> instance is used to await the <see cref="Task"/>.</returns>
    public static TaskAwaiter GetAwaiter(this DateTime dateTime)
    {
        var now = DateTime.Now;
        if (dateTime > now)
        {
            return Task.Delay(dateTime - now).GetAwaiter();
        }
        return Task.CompletedTask.GetAwaiter();
    }
    /// <summary>
    /// Returns an awaiter that can be used to await a <see cref="Task"/> that completes when the specified <see cref="DateTimeOffset"/> is reached.
    /// </summary>
    /// <param name="dateTimeOffset">The <see cref="DateTimeOffset"/> to wait for.</param>
    /// <returns>A <see cref="TaskAwaiter"/> instance is used to await the <see cref="Task"/>.</returns>
    public static TaskAwaiter GetAwaiter(this DateTimeOffset dateTimeOffset)
    {
        var now = DateTimeOffset.Now;
        if (dateTimeOffset > now)
        {
            return Task.Delay(dateTimeOffset - now).GetAwaiter();
        }
        return Task.CompletedTask.GetAwaiter();
    }
    /// <summary>
    /// Returns an awaiter that can be used to await a <see cref="Task"/> that completes when the specified <see cref="DateOnly"/> at midnight is reached.
    /// </summary>
    /// <param name="dateOnly">The <see cref="DateOnly"/> to wait for.</param>
    /// <returns>A <see cref="TaskAwaiter"/> instance is used to await the <see cref="Task"/>.</returns>
    public static TaskAwaiter GetAwaiter(this DateOnly dateOnly) => GetAwaiter(dateOnly.ToDateTime(TimeOnly.MinValue));
    /// <summary>
    /// Returns an awaiter that can be used to await a <see cref="Task"/> that completes when the specified <see cref="TimeOnly"/> on the current day is reached.
    /// </summary>
    /// <param name="timeOnly">The <see cref="TimeOnly"/> to wait for.</param>
    /// <returns>A <see cref="TaskAwaiter"/> instance is used to await the <see cref="Task"/>.</returns>
    public static TaskAwaiter GetAwaiter(this TimeOnly timeOnly) => GetAwaiter(DateTime.Today.AddTicks(timeOnly.Ticks));
    /// <summary>
    /// Returns an awaiter that can be used to await a <see cref="Task"/> that completes when the specified <see cref="TimeSpan"/>, starting from now, has passed.
    /// </summary>
    /// <param name="timeSpan">The <see cref="TimeSpan"/> to wait for.</param>
    /// <returns>A <see cref="TaskAwaiter"/> instance is used to await the <see cref="Task"/>.</returns>
    /// <remarks>
    /// This is essentially just a shorthand for creating a <see cref="Task.Delay(TimeSpan)"/> call.
    /// </remarks>
    public static TaskAwaiter GetAwaiter(this TimeSpan timeSpan) => Task.Delay(timeSpan).GetAwaiter();
}
