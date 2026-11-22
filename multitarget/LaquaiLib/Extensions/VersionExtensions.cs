namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extensions for <see cref="Version"/>.
/// </summary>
public static class VersionExtensions
{
    extension(Version version)
    {
        public int CanonicalCompareTo(Version other)
        {
            ArgumentNullException.ThrowIfNull(version);
            ArgumentNullException.ThrowIfNull(other);
            return version.GetComponents().CompareTo(other.GetComponents());
        }

        public Version WithZeroedUnknowns()
        {
            ArgumentNullException.ThrowIfNull(version);
            if (version.Build >= 0 && version.Revision >= 0)
                return version;
            var (major, minor, build, revision) = version.GetComponents();
            return new(major, minor, build, revision);
        }
        public static Version CreateZeroed(int major, int minor = 0, int patch = 0, int revision = 0) => new Version(major, Math.Max(minor, 0), Math.Max(patch, 0), Math.Max(revision, 0));

        public (int Major, int Minor, int Build, int Revision) GetComponents()
        {
            ArgumentNullException.ThrowIfNull(version);
            return (version.Major, version.Minor, Math.Max(version.Build, 0), Math.Max(version.Revision, 0));
        }
    }
}
