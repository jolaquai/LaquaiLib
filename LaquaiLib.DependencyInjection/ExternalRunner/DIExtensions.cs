namespace LaquaiLib.DependencyInjection.ExternalRunner;

/// <summary>
/// Provides extension methods for dependency injection.
/// </summary>
public static class DIExtensions
{
    /// <summary>
    /// Adds the external runner service and most default <see cref="IExternalRunner"/> implementations to the service collection.
    /// </summary>
    /// <param name="services">The service collection to which the external runner service will be added.</param>
    /// <returns>A <see cref="ExternalRunnerBuilder"/> instance to configure the external runner service.</returns>
    public static ExternalRunnerBuilder AddExternalRunners(this IServiceCollection services)
    {
        _ = services.AddHostedService<ExternalRunnerService>();
        _ = services.AddSingleton<IExternalRunner, ExecutableRunner>();
        _ = services.AddSingleton<IExternalRunner, PythonScriptRunner>();
        _ = services.AddSingleton<IExternalRunner, BatchScriptRunner>();
        return new ExternalRunnerBuilder(services);
    }
}
