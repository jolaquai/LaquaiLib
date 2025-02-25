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
    /// <remarks>
    /// This method does not persist changes to the database.
    /// </remarks>
    public static async Task<TEntity> GetOrAdd<TEntity>(this DbSet<TEntity> set, Func<TEntity, bool> selector, Func<Task<TEntity>> factory)
        where TEntity : class
    {
        if (set.FirstOrDefault(selector) is TEntity existing)
        {
            return existing;
        }
        var newEntity = await factory().ConfigureAwait(false);
        _ = set.Add(newEntity);
        return newEntity;
    }
    /// <summary>
    /// Adds the specified <paramref name="entity"/> object to the <see cref="DbSet{TEntity}"/> if no entity with the specified <paramref name="keys"/> is found.
    /// Otherwise, it updates the existing entity found by the specified <paramref name="keys"/> with the specified <paramref name="entity"/> object.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="set">The <see cref="DbSet{TEntity}"/> to add or update the entity in.</param>
    /// <param name="entity">The actual entity object to add or update.</param>
    /// <param name="keys">The primary key(s) of the entity to search for.</param>
    /// <returns>The existing entity from the database (which will not yet reflect the changes made to the <paramref name="entity"/> object) if an entity with the specified <paramref name="keys"/> is found, otherwise <paramref name="entity"/> object itself.</returns>
    /// <remarks>
    /// This method does not persist changes to the database.
    /// <para/>To prevent double-adding entities, <paramref name="entity"/> is attached to the <see cref="DbSet{TEntity}"/> before anything else is done.
    /// </remarks>
    public static TEntity AddOrUpdate<TEntity>(this DbSet<TEntity> set, TEntity entity, params object[] keys)
        where TEntity : class
    {
        _ = set.Attach(entity);
        var existing = set.Find(keys);
        if (existing is not null)
        {
            _ = set.Update(entity);
            return existing;
        }
        _ = set.Add(entity);
        return entity;
    }
}
