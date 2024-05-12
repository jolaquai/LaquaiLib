using LaquaiLib.Extensions;

using Timer = System.Threading.Timer;

namespace LaquaiLib.Util;

/// <summary>
/// Provides methods and events for working with drives.
/// </summary>
public static class Drives
{
    private static readonly Timer _timer = new Timer(ConditionalRaiseEvents, null, Timeout.Infinite, 10);
    private static List<DriveInfo> previousDriveList = [];
    private static List<DriveInfo> previousRemovableList = [];
    private static List<DriveInfo> previousReadyCdList = [];
    private static DateTime lastCdDriveChange = DateTime.MinValue;

    /// <summary>
    /// The <see cref="object"/> that is locked on when modifying collections in any of the "GetAll..." methods in <see cref="Drives"/>. <b>Callers should lock on this when accessing these collections as well, otherwise, exceptions may be thrown during enumeration.</b>
    /// </summary>
    public static object SyncRoot { get; } = new object();

    static Drives()
    {
        GetAllDrives(previousDriveList);
        GetAllCdDrives(previousReadyCdList);
        previousReadyCdList.KeepOnly(drive => drive.IsReady);
    }

    /// <summary>
    /// Replaces the contents of the given <paramref name="existing"/> <see cref="ICollection{T}"/> of <see cref="DriveInfo"/> with <see cref="DriveInfo"/> instances representing all drives on the system.
    /// </summary>
    /// <param name="existing">The <see cref="ICollection{T}"/> of <see cref="DriveInfo"/> to place the <see cref="DriveInfo"/>s into.</param>
    /// <remarks>For the entire duration of this method, a lock on <see cref="SyncRoot"/> is held.</remarks>
    public static void GetAllDrives(ICollection<DriveInfo> existing)
    {
        lock (SyncRoot)
        {
            existing.Clear();
            foreach (var drvInfo in DriveInfo.GetDrives())
            {
                existing.Add(drvInfo);
            }
        }
    }

    /// <summary>
    /// Replaces the contents of the given <paramref name="existing"/> <see cref="ICollection{T}"/> of <see cref="DriveInfo"/> with <see cref="DriveInfo"/> instances representing all CD-ROM drives on the system.
    /// </summary>
    /// <param name="existing">The <see cref="ICollection{T}"/> of <see cref="DriveInfo"/> to place the CD-ROM drives' <see cref="DriveInfo"/>s into.</param>
    /// <remarks>For the entire duration of this method, a lock on <see cref="SyncRoot"/> is held.</remarks>
    public static void GetAllCdDrives(ICollection<DriveInfo> existing)
    {
        lock (SyncRoot)
        {
            GetAllDrives(existing);
            existing.KeepOnly(drv => drv.DriveType == DriveType.CDRom);
        }
    }

    /// <summary>
    /// Replaces the contents of the given <paramref name="existing"/> <see cref="ICollection{T}"/> of <see cref="DriveInfo"/> with <see cref="DriveInfo"/> instances representing all removable drives on the system.
    /// </summary>
    /// <param name="existing">The <see cref="ICollection{T}"/> of <see cref="DriveInfo"/> to place the removable drives' <see cref="DriveInfo"/>s into.</param>
    /// <remarks>For the entire duration of this method, a lock on <see cref="SyncRoot"/> is held.</remarks>
    public static void GetAllRemovableDrives(ICollection<DriveInfo> existing)
    {
        lock (SyncRoot)
        {
            GetAllDrives(existing);
            existing.KeepOnly(drv => drv.DriveType == DriveType.Removable);
        }
    }

    private static event Action<DriveInfo>? driveConnected;
    private static event Action<DriveInfo>? driveDisconnected;
    private static event Action<DriveInfo>? removableDriveConnected;
    private static event Action<DriveInfo>? removableDriveDisconnected;
    private static event Action<DriveInfo>? discInserted;
    private static event Action<DriveInfo>? discEjected;

    /// <summary>
    /// Occurs when a drive is connected to the system.
    /// </summary>
    public static event Action<DriveInfo>? DriveConnected
    {
        add
        {
            lock (SyncRoot)
            {
                driveConnected += value;
            }
        }
        remove
        {
            lock (SyncRoot)
            {
                driveConnected -= value;
            }
        }
    }
    /// <summary>
    /// Occurs when a drive is disconnected from the system.
    /// </summary>
    public static event Action<DriveInfo>? DriveDisconnected
    {
        add
        {
            lock (SyncRoot)
            {
                driveDisconnected += value;
            }
        }
        remove
        {
            lock (SyncRoot)
            {
                driveDisconnected -= value;
            }
        }
    }
    /// <summary>
    /// Occurs when a removable drive (such as a USB drive) is connected to the system.
    /// </summary>
    public static event Action<DriveInfo>? RemovableDriveConnected
    {
        add
        {
            lock (SyncRoot)
            {
                removableDriveConnected += value;
            }
        }
        remove
        {
            lock (SyncRoot)
            {
                removableDriveConnected -= value;
            }
        }
    }
    /// <summary>
    /// Occurs when a removable drive (such as a USB drive) is disconnected from the system.
    /// </summary>
    public static event Action<DriveInfo>? RemovableDriveDisconnected
    {
        add
        {
            lock (SyncRoot)
            {
                removableDriveDisconnected += value;
            }
        }
        remove
        {
            lock (SyncRoot)
            {
                removableDriveDisconnected -= value;
            }
        }
    }
    /// <summary>
    /// Occurs when a disc is inserted into a drive.
    /// </summary>
    public static event Action<DriveInfo>? DiscInserted
    {
        add
        {
            lock (SyncRoot)
            {
                discInserted += value;
            }
        }
        remove
        {
            lock (SyncRoot)
            {
                discInserted -= value;
            }
        }
    }
    /// <summary>
    /// Occurs when a disc is ejected from a drive.
    /// </summary>
    public static event Action<DriveInfo>? DiscEjected
    {
        add
        {
            lock (SyncRoot)
            {
                discEjected += value;
            }
        }
        remove
        {
            lock (SyncRoot)
            {
                discEjected -= value;
            }
        }
    }

    /// <summary>
    /// Removes all entries in the invocation lists of the events defined in <see cref="Drives"/>.
    /// </summary>
    public static void Clear()
    {
        driveDisconnected = null;
        driveConnected = null;
        removableDriveConnected = null;
        removableDriveDisconnected = null;
        discInserted = null;
        discEjected = null;
    }
    /// <summary>
    /// Starts raising the events defined in <see cref="Drives"/>.
    /// </summary>
    public static void Start()
    {
        _timer.Change(0, 10);
    }
    /// <summary>
    /// Stops raising the events defined in <see cref="Drives"/>.
    /// </summary>
    public static void Stop()
    {
        _timer.Change(Timeout.Infinite, 10);
    }

    /// <summary>
    /// Raises the events defined in <see cref="Drives"/> if their conditions are met.
    /// </summary>
    /// <param name="state">Unused / ignored unconditionally.</param>
    private static void ConditionalRaiseEvents(object? state)
    {
        #region Removable
        if (removableDriveConnected is not null
            || removableDriveDisconnected is not null)
        {
            var currentRemovableDriveList = new List<DriveInfo>();
            GetAllRemovableDrives(currentRemovableDriveList);
            var newRemovableDrives =
                currentRemovableDriveList.ExceptBy(previousRemovableList, drv => drv.Name);
            var removedRemovableDrives =
                previousRemovableList.ExceptBy(currentRemovableDriveList, drv => drv.Name);

            foreach (var drive in newRemovableDrives)
            {
                removableDriveConnected?.Invoke(drive);
            }
            foreach (var drive in removedRemovableDrives)
            {
                removableDriveDisconnected?.Invoke(drive);
            }

            previousRemovableList = currentRemovableDriveList;
        }
        #endregion
        #region CDRom
        // Special treatments for CDs because their status is... weird
        if (DateTime.Now - lastCdDriveChange > TimeSpan.FromMilliseconds(500)
            && (discInserted is not null || discEjected is not null))
        {
            var currentReadyCdList = new List<DriveInfo>();
            GetAllCdDrives(currentReadyCdList);
            currentReadyCdList.KeepOnly(cdDrv => cdDrv.IsReady);

            var insertedCds = currentReadyCdList.ExceptBy(previousReadyCdList, drv => drv.IsReady);
            var ejectedCds = previousReadyCdList.ExceptBy(currentReadyCdList, drv => drv.IsReady);

            foreach (var drive in insertedCds)
            {
                discInserted?.Invoke(drive);
            }

            foreach (var drive in ejectedCds)
            {
                discEjected?.Invoke(drive);
            }

            previousReadyCdList = currentReadyCdList;

            lastCdDriveChange = DateTime.Now;
        }
        #endregion
        #region Any type that is not handled otherwise
        if (driveConnected is not null
            || driveDisconnected is not null)
        {
            var currentDriveList = new List<DriveInfo>();
            GetAllDrives(currentDriveList);
            var newDrives =
                currentDriveList.ExceptBy(previousDriveList, drv => drv.Name)
                                .Where(drv => drv.DriveType is not (DriveType.Removable or DriveType.CDRom));
            var removedDrives =
                previousDriveList.ExceptBy(currentDriveList, drv => drv.Name)
                                 .Where(drv => drv.DriveType is not (DriveType.Removable or DriveType.CDRom));

            foreach (var drive in newDrives)
            {
                driveConnected?.Invoke(drive);
            }
            foreach (var drive in removedDrives)
            {
                driveDisconnected?.Invoke(drive);
            }

            previousDriveList = currentDriveList;
        }
        #endregion
    }
}
