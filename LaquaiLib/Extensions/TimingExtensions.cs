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
}
