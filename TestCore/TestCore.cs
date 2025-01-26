using Microsoft.Extensions.DependencyInjection;

namespace TestCore;

public static class TestCore
{
    private static bool initialized;
    private static ServiceProvider provider;
    private static void Initialize()
    {
        var services = new ServiceCollection();

        _ = services.AddSingleton<HttpClient>();
        _ = services.AddSingleton(Random.Shared);

        provider = services.BuildServiceProvider(true);

        initialized = true;
    }

    public static Task<AsyncServiceScope> GetScope()
    {
        if (!initialized)
        {
            Initialize();
        }

        return Task.FromResult(provider.CreateAsyncScope());
    }
}
