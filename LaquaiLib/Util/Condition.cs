namespace LaquaiLib.Util;

/// <summary>
/// Represents a <see cref="bool"/> value that can be chained with other <see cref="Condition"/>s.
/// </summary>
public readonly struct Condition
{
    /// <summary>
    /// Returns a cached <see cref="Condition"/> representing <c>true</c>.
    /// </summary>
    public static Condition True { get; } = new Condition(true);
    /// <summary>
    /// Returns a cached <see cref="Condition"/> representing <c>false</c>.
    /// </summary>
    public static Condition False { get; } = new Condition(false);

    /// <summary>
    /// The actual <see cref="bool"/> value this <see cref="Condition"/> represents.
    /// </summary>
    public readonly bool Value { get; }
    
    /// <summary>
    /// Instantiates a new <see cref="Condition"/>. Its default state is representative of <c>false</c>.
    /// </summary>
    public Condition() : this(false)
    {
    }

    /// <summary>
    /// Instantiates a new <see cref="Condition"/> representing the given <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The <see cref="bool"/> value this <see cref="Condition"/> should represent.</param>
    public Condition(bool value)
    {
        Value = value;
    }

    /// <summary>
    /// Converts a <see cref="Condition"/> to a <see cref="bool"/>.
    /// </summary>
    /// <param name="condition">The <see cref="Condition"/> to convert.</param>
    public static implicit operator bool(Condition condition) => condition.Value;
    /// <summary>
    /// Converts a <see cref="bool"/> to a <see cref="Condition"/>.
    /// </summary>
    /// <param name="value">The <see cref="bool"/> to convert.</param>
    public static implicit operator Condition(bool value) => value ? True : False;

    /// <summary>
    /// Chains the current <see cref="Condition"/> with another <see cref="Condition"/> using logical AND..
    /// </summary>
    /// <param name="condition">The <see cref="Condition"/> to chain with.</param>
    /// <returns>A new <see cref="Condition"/> representing the result of the logical AND.</returns>
    public Condition And(Condition condition) => Value && condition;
    /// <summary>
    /// Chains the current <see cref="Condition"/> with other <see cref="Condition"/>s using logical AND.
    /// </summary>
    /// <param name="conditions">The <see cref="Condition"/>s to chain with.</param>
    /// <returns>A new <see cref="Condition"/> representing the result of the logical AND.</returns>
    public Condition And(params Condition[] conditions) => this && conditions.All(b => b);
    /// <summary>
    /// Chains the current <see cref="Condition"/> with another <see cref="Condition"/> using logical OR.
    /// </summary>
    /// <param name="condition">The <see cref="Condition"/> to chain with.</param>
    /// <returns>A new <see cref="Condition"/> representing the result of the logical OR.</returns>
    public Condition Or(Condition condition) => Value || condition;
    /// <summary>
    /// Chains the current <see cref="Condition"/> with other <see cref="Condition"/>s using logical OR.
    /// </summary>
    /// <param name="conditions">The <see cref="Condition"/>s to chain with.</param>
    /// <returns>A new <see cref="Condition"/> representing the result of the logical OR.</returns>
    public Condition Or(params Condition[] conditions) => this || conditions.Any(b => b);
    /// <summary>
    /// Inverts the current <see cref="Condition"/> using logical NOT.
    /// </summary>
    /// <returns>A new <see cref="Condition"/> representing the result of the logical NOT.</returns>
    public Condition Not() => !Value;

    /// <summary>
    /// Chains the current <see cref="Condition"/> with other <see cref="Condition"/>s using logical XOR.
    /// </summary>
    /// <param name="conditions">The <see cref="Condition"/>s to chain with.</param>
    /// <returns>A new <see cref="Condition"/> representing the result of the logical XOR.</returns>
    public Condition Xor(params Condition[] conditions) => (this && False.Are(conditions)) || (Not() && conditions.Count(b => b) == 1);

    /// <summary>
    /// Checks whether the current <see cref="Condition"/> is equal to the given <paramref name="condition"/>.
    /// </summary>
    /// <param name="condition">The <see cref="Condition"/> to check for equality.</param>
    /// <returns>A new <see cref="Condition"/> representing the result of the equality check.</returns>
    public Condition Is(Condition condition) => Value == condition;
    /// <summary>
    /// Checks whether the current <see cref="Condition"/> is equal to all of the given <paramref name="conditions"/>.
    /// </summary>
    /// <param name="conditions">The <see cref="Condition"/>s to check for equality.</param>
    /// <returns>A new <see cref="Condition"/> representing the result of the equality check.</returns>
    public Condition Are(params Condition[] conditions)
    {
        var thisValue = this;
        return conditions.All(b => b == thisValue);
    }
    /// <summary>
    /// Checks whether the current <see cref="Condition"/> is equal to any of the given <paramref name="conditions"/>.
    /// </summary>
    /// <param name="conditions">The <see cref="Condition"/>s to check for equality.</param>
    /// <returns>A new <see cref="Condition"/> representing the result of the equality check.</returns>
    public Condition Any(params Condition[] conditions)
    {
        var thisValue = this;
        return conditions.Any(b => b == thisValue);
    }

    /// <summary>
    /// Checks whether the current <see cref="Condition"/> is not equal to the given <paramref name="condition"/>.
    /// </summary>
    /// <param name="condition">The <see cref="Condition"/> to check for inequality.</param>
    /// <returns>A new <see cref="Condition"/> representing the result of the inequality check.</returns>
    public Condition IsNot(Condition condition) => Value != condition;
    /// <summary>
    /// Checks whether the current <see cref="Condition"/> is not equal to all of the given <paramref name="conditions"/>.
    /// </summary>
    /// <param name="conditions">The <see cref="Condition"/>s to check for inequality.</param>
    /// <returns>A new <see cref="Condition"/> representing the result of the inequality check.</returns>
    public Condition AreNot(params Condition[] conditions)
    {
        var thisValue = this;
        return conditions.All(b => b != thisValue);
    }
    /// <summary>
    /// Checks whether the current <see cref="Condition"/> is not equal to any of the given <paramref name="conditions"/>.
    /// </summary>
    /// <param name="conditions">The <see cref="Condition"/>s to check for inequality.</param>
    /// <returns>A new <see cref="Condition"/> representing the result of the inequality check.</returns>
    public Condition AnyNot(params Condition[] conditions)
    {
        var thisValue = this;
        return conditions.Any(b => b != thisValue);
    }
}
