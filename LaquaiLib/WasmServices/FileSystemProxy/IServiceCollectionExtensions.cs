using Microsoft.Extensions.DependencyInjection;

namespace LaquaiLib.WasmServices.FileSystemProxy;

/// <summary>
/// Provides extensions methods for types implementing <see cref="IServiceCollection"/>.
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds the <see cref="IFileSystemProxy"/> and its dependencies to the <see cref="IServiceCollection"/> as singletons.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the <see cref="IFileSystemProxy"/> and its dependencies to.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IServiceCollection AddFileSystemProxy(this IServiceCollection services)
    {
        return services.AddSingleton<IFile>(new File())
                       .AddSingleton<IDirectory>(new Directory())
                       .AddSingleton<IPath>(new Path())
                       .AddSingleton<IFileSystemProxy>(static sp => new FileSystemProxy(sp.GetRequiredService<IFile>(), sp.GetRequiredService<IDirectory>(), sp.GetRequiredService<IPath>()));
    }
}
