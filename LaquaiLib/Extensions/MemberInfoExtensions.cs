using System.Reflection;

using LaquaiLib.Util.DynamicExtensions.FullAccessDynamic;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="MemberInfo"/> Type.
/// </summary>
public static class MemberInfoExtensions
{
    /// <summary>
    /// Creates a <see cref="BindingFlags"/> enum value that, when used to reflect into the member represented by a <paramref name="memberInfo"/> instance, will yield that same instance.
    /// </summary>
    /// <returns>The created <see cref="BindingFlags"/> enum value.</returns>
    public static BindingFlags ConstructBindingFlags(this MemberInfo memberInfo)
    {
        var flags = BindingFlags.Default;

        // static?
        var wrapper = SilentFullAccessDynamicFactory.Create(memberInfo.GetType(), memberInfo);
        if ((bool)wrapper.IsStatic)
        {
            flags |= BindingFlags.Static;
        }
        else
        {
            flags |= BindingFlags.Instance;
        }
        if ((bool)wrapper.IsPublic)
        {
            flags |= BindingFlags.Public;
        }
        else
        {
            flags |= BindingFlags.NonPublic;
        }

        return flags;
    }
}
