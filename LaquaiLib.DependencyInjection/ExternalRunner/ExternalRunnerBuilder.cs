namespace LaquaiLib.DependencyInjection.ExternalRunner;

/// <summary>
/// Implements the builder pattern for <see cref="ExternalRunnerService"/>.
/// </summary>
/// <param name="services">The service collection to which the external runner service were added and which will receive calls to configuration methods.</param>
public class ExternalRunnerBuilder(IServiceCollection services)
{
    private readonly IServiceCollection _services = services;

    /// <summary>
    /// Registers a new external object to run.
    /// </summary>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public ExternalRunnerBuilder AddExternal(string path, params string[] args)
    {
        _services.Configure<ExternalRunnerOptions>(o =>
        {
            o.Externals.Add((path, args));
        });
        return this;
    }
    /// <summary>
    /// Configures the underlying runner manager to use Windows PowerShell (<c>powershell.exe</c>) to handle external objects with the extension <c>.ps1</c>.
    /// This cannot be used in conjunction with <see cref="UsePwsh"/>. Attempting to do so will throw an <see cref="InvalidOperationException"/>.
    /// </summary>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public ExternalRunnerBuilder UsePowershell()
    {
        _services.AddSingleton<IExternalRunner, PowerShellScriptRunner>();
        return this;
    }
    /// <summary>
    /// Configures the underlying runner manager to use Microsoft PowerShell / PowerShell Core (<c>pwsh.exe</c>) to handle external objects with the extension <c>.ps1</c>.
    /// This cannot be used in conjunction with <see cref="UsePowershell"/>. Attempting to do so will throw an <see cref="InvalidOperationException"/>.
    /// </summary>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public ExternalRunnerBuilder UsePwsh()
    {
        _services.AddSingleton<IExternalRunner, PwshScriptRunner>();
        return this;
    }
}
