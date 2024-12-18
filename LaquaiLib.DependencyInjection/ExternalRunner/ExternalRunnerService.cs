namespace LaquaiLib.DependencyInjection.ExternalRunner;

/// <summary>
/// Services <see cref="IExternalRunner"/> instances, running registered external objects and managing their lifetimes.
/// </summary>
public class ExternalRunnerService : BackgroundService
{
    private readonly ExternalRunnerOptions _options;
    private readonly IServiceScope _scope;
    private readonly IServiceProvider _serviceProvider;
    private readonly IExternalRunner[] _runners;

    private Task[] tasks;

    /// <summary>
    /// Initializes a new <see cref="ExternalRunnerService"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceProvider"/> to create a scope from.</param>
    /// <param name="options">An <see cref="IOptions{TOptions}"/> instance that holds the <see cref="ExternalRunnerOptions"/>.</param>
    public ExternalRunnerService(IServiceProvider services, IOptions<ExternalRunnerOptions> options)
    {
        _options = options.Value;
        _scope = services.CreateScope();
        _serviceProvider = _scope.ServiceProvider;
        _runners = _serviceProvider.GetServices<IExternalRunner>().ToArray();

        EnsureNoPowerShellConflicts();
        EnsureAllExternalsHandled();
        EnsureExternalsExist();
    }

    /// <inheritdoc/>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        tasks = new Task[_options.Externals.Count];
        foreach (var (i, (path, args)) in _options.Externals.Index())
        {
            if (Array.Find(_runners, r => r.CanHandle(path)) is IExternalRunner runner)
            {
                tasks[i] = runner.RunAsync(path, args, stoppingToken);
            }
        }
        return Task.WhenAll(tasks);
    }

    private void EnsureAllExternalsHandled()
    {
        var unhandled = _options.Externals
            .Select(t => t.Item1)
            .Where(path => !_runners.Any(runner => runner.CanHandle(path)));
        if (unhandled.Any())
        {
            throw new InvalidOperationException($"No runner found for the following external objects:{Environment.NewLine}{string.Join(Environment.NewLine, unhandled)}");
        }
    }
    private void EnsureExternalsExist()
    {
        var missing = _options.Externals
            .Select(t => t.Item1)
            .Where(path => !File.Exists(path));
        if (missing.Any())
        {
            throw new FileNotFoundException($"The following external objects do not exist:{Environment.NewLine}{string.Join(Environment.NewLine, missing)}");
        }
    }
    private void EnsureNoPowerShellConflicts()
    {
        if (Array.FindIndex(_runners, r => r is PowerShellScriptRunner) > -1 && Array.FindIndex(_runners, r => r is PwshScriptRunner) > -1)
        {
            throw new InvalidOperationException($"Cannot have both a {nameof(PowerShellScriptRunner)} and a {nameof(PwshScriptRunner)} registered.");
        }
    }

    private bool disposed;
    /// <inheritdoc/>
    public override void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;

        GC.SuppressFinalize(this);

        base.Dispose();
        _scope.Dispose();
    }
}
