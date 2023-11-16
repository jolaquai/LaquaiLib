using System.Reflection;

namespace LaquaiLib.Analyzers;

/// <summary>
/// Contains constants used throughout the project.
/// </summary>
internal static class Constants
{
    internal static class DiagnosticCategories
    {
        internal static string ArrayFindIndex { get; }

        static DiagnosticCategories()
        {
            foreach (var property in typeof(DiagnosticCategories).GetProperties(BindingFlags.NonPublic | BindingFlags.Static))
            {
                if (property.PropertyType == typeof(string))
                {
                    property.SetValue(null, $"{typeof(Constants).Namespace}.{property.Name}");
                }
            }
        }
    }
}
