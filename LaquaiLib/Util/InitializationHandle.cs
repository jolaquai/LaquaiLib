namespace LaquaiLib.Interfaces;

/// <summary>
/// Supports the <see cref="IInitializable"/> interface. While an instance of this <see langword="struct"/> is held and not disposed, the <see cref="IInitializable"/> instance is considered to be initializing.
/// </summary>
public class InitializationHandle : IDisposable
{
    private readonly IInitializable component;
    void IDisposable.Dispose()
    {
        if (component is null)
        {
            throw new InvalidOperationException($"{nameof(InitializationHandle)}s with a null component may not be operated on.");
        }
        component.EndInit();
    }
    /// <summary>
    /// Constructs a  new <see cref="InitializationHandle"/> for the given <see cref="IInitializable"/> component. While the instance remains undisposed, the component should be considered to be initializing.
    /// </summary>
    /// <param name="component">The <see cref="IInitializable"/> component to initialize.</param>
    public InitializationHandle(IInitializable component)
    {
        this.component = component;
        this.component.BeginInit();
    }
}
