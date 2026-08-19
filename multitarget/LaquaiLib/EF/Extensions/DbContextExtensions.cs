using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace LaquaiLib.EF.Extensions;

/// <summary>
/// Provides extensions for <see cref="DbContext"/>.
/// </summary>
public static class DbContextExtensions
{
    extension(DbContext context)
    {
        /// <summary>
        /// Enumerates all <see langword="object"/>s which have pending operations in the specified <see cref="DbContext"/> which a <see cref="DbContext.SaveChanges()"/> call would affect.
        /// </summary>
        /// <returns>The pending <see langword="object"/>s.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<object> EnumeratePendingObjects() => EnumeratePendingEntries(context).Select(static e => e.Entity);
        /// <summary>
        /// Enumerates all <typeparamref name="TUnderlying"/> instances which have pending operations in the specified <see cref="DbContext"/> which a <see cref="DbContext.SaveChanges()"/> call would affect.
        /// </summary>
        /// <returns>The pending <typeparamref name="TUnderlying"/> instances.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TUnderlying> EnumeratePendingObjects<TUnderlying>() where TUnderlying : class => EnumeratePendingEntries<TUnderlying>(context).Select(static e => e.Entity);
        /// <summary>
        /// Enumerates all <see cref="EntityEntry"/>s which have pending operations in the specified <see cref="DbContext"/> which a <see cref="DbContext.SaveChanges()"/> call would affect.
        /// </summary>
        /// <returns>The pending <see cref="EntityEntry"/>s.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<EntityEntry> EnumeratePendingEntries() => context.ChangeTracker.Entries().Where(static e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);
        /// <summary>
        /// Enumerates all <see cref="EntityEntry{TEntity}"/> of <typeparamref name="TUnderlying"/> which have pending operations in the specified <see cref="DbContext"/> which a <see cref="DbContext.SaveChanges()"/> call would affect.
        /// </summary>
        /// <returns>The pending <see cref="EntityEntry{TEntity}"/> of <typeparamref name="TUnderlying"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<EntityEntry<TUnderlying>> EnumeratePendingEntries<TUnderlying>()
            where TUnderlying : class => context.ChangeTracker.Entries<TUnderlying>().Where(static e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);
    }
}
