using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace LaquaiLib.Windows.Extensions;

/// <summary>
/// Provides extensions for the <see cref="DependencyObject"/> type.
/// </summary>
public static class DependencyObjectExtensions
{
    extension(DependencyObject obj)
    {
        /// <summary>
        /// Enumerates all binding expressions in the specified <see cref="DependencyObject"/> and forces them to fetch new values.
        /// </summary>
        public void UpdateBindings()
        {
            if (obj is null)
                return;

            var locals = obj.GetLocalValueEnumerator();
            while (locals.MoveNext())
            {
                var dp = locals.Current.Property;
                BindingOperations.GetBindingExpression(obj, dp)?.UpdateTarget();
                BindingOperations.GetMultiBindingExpression(obj, dp)?.UpdateTarget();
            }
        }
        /// <summary>
        /// Enumerates all binding expressions in the specified <see cref="DependencyObject"/> and its child hierarchy and forces them to fetch new values.
        /// </summary>
        public void UpdateBindingsRecurse()
        {
            // First update the current object
            UpdateBindings(obj);

            // Then recurse into children
            var childrenCount = VisualTreeHelper.GetChildrenCount(obj);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                UpdateBindingsRecurse(child);
            }
        }
    }
}
