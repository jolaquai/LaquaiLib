namespace LaquaiLib.EF.Extensions;

/// <summary>
/// Provides Extension Methods for <see cref="DbContext"/>.
/// </summary>
public static class DbContextExtensions
{
    extension(DbContext context)
    {
        /// <summary>
        /// Enumerates all <see langword="object"/>s which have pending operations in the specified <see cref="DbContext"/> which a <see cref="DbContext.SaveChanges()"/> call would affect.
        /// </summary>
        /// <param name="context">The <see cref="DbContext"/> to get the pending <see langword="object"/>s from.</param>
        /// <returns>The pending <see langword="object"/>s.</returns>
        public IEnumerable<object> EnumeratePendingObjects() => EnumeratePendingEntries(context).Select(static e => e.Entity);
        /// <summary>
        /// Enumerates all <typeparamref name="TUnderlying"/> instances which have pending operations in the specified <see cref="DbContext"/> which a <see cref="DbContext.SaveChanges()"/> call would affect.
        /// </summary>
        /// <param name="context">The <see cref="DbContext"/> to get the pending <typeparamref name="TUnderlying"/> instances from.</param>
        /// <returns>The pending <typeparamref name="TUnderlying"/> instances.</returns>
        public IEnumerable<TUnderlying> EnumeratePendingObjects<TUnderlying>() where TUnderlying : class => EnumeratePendingEntries<TUnderlying>(context).Select(static e => e.Entity);
        /// <summary>
        /// Enumerates all <see cref="EntityEntry"/>s which have pending operations in the specified <see cref="DbContext"/> which a <see cref="DbContext.SaveChanges()"/> call would affect.
        /// </summary>
        /// <param name="context">The <see cref="DbContext"/> to get the pending <see cref="EntityEntry"/>s from.</param>
        /// <returns>The pending <see cref="EntityEntry"/>s.</returns>
        public IEnumerable<EntityEntry> EnumeratePendingEntries() => context.ChangeTracker.Entries().Where(static e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);
        /// <summary>
        /// Enumerates all <see cref="EntityEntry{TEntity}"/> of <typeparamref name="TUnderlying"/> which have pending operations in the specified <see cref="DbContext"/> which a <see cref="DbContext.SaveChanges()"/> call would affect.
        /// </summary>
        /// <param name="context">The <see cref="DbContext"/> to get the pending <see cref="EntityEntry"/>s from.</param>
        /// <returns>The pending <see cref="EntityEntry{TEntity}"/> of <typeparamref name="TUnderlying"/>.</returns>
        public IEnumerable<EntityEntry<TUnderlying>> EnumeratePendingEntries<TUnderlying>()
            where TUnderlying : class => context.ChangeTracker.Entries<TUnderlying>().Where(static e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);
    }
}
