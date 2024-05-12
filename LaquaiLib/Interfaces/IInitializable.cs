using System.ComponentModel;

namespace LaquaiLib.Interfaces;

/// <summary>
/// Extends the <see cref="ISupportInitialize"/> using an <see cref="InitializationHandle"/>.
/// </summary>
public interface IInitializable : ISupportInitialize
{
    /// <summary>
    /// Gets or sets whether this instance is currently initializing.
    /// </summary>
    bool IsInitializing
    {
        get; protected set;
    }
    void ISupportInitialize.BeginInit() => IsInitializing = true;
    /// <summary>
    /// Transitions this instance to the initializing state and creates an <see cref="InitializationHandle"/> that represents this initialization.
    /// </summary>
    /// <returns>The created <see cref="InitializationHandle"/>.</returns>
    new InitializationHandle BeginInit() => new InitializationHandle(this);
    void ISupportInitialize.EndInit() => IsInitializing = false;
}
