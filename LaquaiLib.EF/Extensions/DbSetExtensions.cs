using Microsoft.EntityFrameworkCore;

namespace LaquaiLib.EF.Extensions;

/// <summary>
/// Provides Extension Methods for the <see cref="DbSet{TEntity}"/> Type.
/// </summary>
public static class DbSetExtensions
{
    /// <summary>
    /// Gets an entity of type <typeparamref name="TEntity"/> if the <see cref="DbSet{TEntity}"/> contains one that fulfills the conditions of the <paramref name="selector"/>. If no entity is found, a new entity is created and added to the set using the <paramref name="factory"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="set">The <see cref="DbSet{TEntity}"/> to search in.</param>
    /// <param name="selector">A <see cref="Func{T, TResult}"/> that defines the conditions of the entity to search for. If multiple entities fulfill the conditions, the first match is returned, so it is advisable to incorporate <typeparamref name="TEntity"/>'s primary key(s) in the conditions.</param>
    /// <param name="factory">A <see cref="Func{TResult}"/> that asynchronously creates a new entity if no entity is found.</param>
    /// <returns>The found or created entity.</returns>
    public static async Task<TEntity> GetOrAdd<TEntity>(this DbSet<TEntity> set, Func<TEntity, bool> selector, Func<Task<TEntity>> factory)
        where TEntity : class
    {
        if (set.FirstOrDefault(selector) is TEntity existing)
        {
            return existing;
        }
        var newEntity = await factory().ConfigureAwait(false);
        set.Add(newEntity);
        return newEntity;
    }
}
