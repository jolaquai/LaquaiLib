using LaquaiLib.IO;

namespace LaquaiLib.UnitTests.IO.FileSystemHelperTests;

public class PathStringTests
{
    #region IsBaseOf
    #endregion

    #region ChangeName
    [Fact]
    public void ChangeNameWithBasicPathReturnsNewName()
    {
        var path = @"C:\folder\oldname.txt";
        var newName = "newname";

        var result = FileSystemHelper.ChangeName(path, newName);

        Assert.Equal(@"C:\folder\newname.txt", result);
    }

    [Fact]
    public void ChangeNameWithNewNameHavingExtensionReplacesFullName()
    {
        var path = @"C:\folder\oldname.txt";
        var newName = "newname.doc";

        var result = FileSystemHelper.ChangeName(path, newName);

        Assert.Equal(@"C:\folder\newname.doc", result);
    }

    [Fact]
    public void ChangeNameWithPathWithoutExtensionReturnsNewName()
    {
        var path = @"C:\folder\oldname";
        var newName = "newname";

        var result = FileSystemHelper.ChangeName(path, newName);

        Assert.Equal(@"C:\folder\newname", result);
    }

    [Fact]
    public void ChangeNameWithPathWithoutDirectoryReturnsNewName()
    {
        var path = "oldname.txt";
        var newName = "newname";

        var result = FileSystemHelper.ChangeName(path, newName);

        Assert.Equal("newname.txt", result);
    }

    [Fact]
    public void ChangeNameWithUnixStylePathsReturnsNewName()
    {
        var path = "/usr/local/oldname.txt";
        var newName = "newname";

        var result = FileSystemHelper.ChangeName(path, newName);

        Assert.Equal("/usr/local/newname.txt", result);
    }

    [Fact]
    public void ChangeNameWithMultipleExtensionDotsHandlesCorrectly()
    {
        var path = @"C:\folder\file.name.with.dots.txt";
        var newName = "newname";

        var result = FileSystemHelper.ChangeName(path, newName);

        Assert.Equal(@"C:\folder\newname.txt", result);
    }

    [Fact]
    public void ChangeNameWithFileNameStartingWithDotHandlesCorrectly()
    {
        var path = @"C:\folder\.hiddenfile";
        var newName = "newname";

        var result = FileSystemHelper.ChangeName(path, newName);

        Assert.Equal(@"C:\folder\newname", result);
    }

    [Fact]
    public void ChangeNameWithEmptyNewNameThrowsArgumentException()
    {
        var path = @"C:\folder\oldname.txt";
        var newName = "";

        Assert.ThrowsAny<ArgumentException>(() => FileSystemHelper.ChangeName(path, newName));
    }

    [Fact]
    public void ChangeNameWithNullNewNameThrowsArgumentNullException()
    {
        var path = @"C:\folder\oldname.txt";
        string newName = null;

        Assert.ThrowsAny<ArgumentException>(() => FileSystemHelper.ChangeName(path, newName));
    }

    [Fact]
    public void ChangeNameWithNullPathThrowsArgumentNullException()
    {
        string path = null;
        var newName = "newname";

        Assert.ThrowsAny<ArgumentException>(() => FileSystemHelper.ChangeName(path, newName));
    }

    [Fact]
    public void ChangeNameWithWhiteSpacePathThrowsArgumentException()
    {
        var path = "   ";
        var newName = "newname";

        Assert.ThrowsAny<ArgumentException>(() => FileSystemHelper.ChangeName(path, newName));
    }

    [Fact]
    public void ChangeNameWithPathEndingWithDirectorySeparatorHandlesCorrectly()
    {
        var path = @"C:\folder\";
        var newName = "newname";

        var result = FileSystemHelper.ChangeName(path, newName);

        Assert.Equal(@"C:\newname", result);
    }

    [Fact]
    public void ChangeNameWithRelativePathHandlesCorrectly()
    {
        var path = @"..\folder\oldname.txt";
        var newName = "newname";

        var result = FileSystemHelper.ChangeName(path, newName);

        Assert.Equal(@"..\folder\newname.txt", result);
    }
    #endregion
}
