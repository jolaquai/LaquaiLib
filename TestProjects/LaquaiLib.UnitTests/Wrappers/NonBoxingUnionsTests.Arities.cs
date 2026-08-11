using LaquaiLib.Wrappers;

namespace LaquaiLib.UnitTests.Wrappers;

public class NonBoxingUnionsAritiesTests
{
    private static readonly Guid GuidValue = new Guid("0f8fad5b-d9cb-469f-a165-70867728950e");
    private static readonly DateTime DateTimeValue = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc);

    private static void AssertOnlyFlag(int expected, params bool[] flags)
    {
        for (var i = 0; i < flags.Length; i++)
        {
            if (i + 1 == expected)
            {
                Assert.True(flags[i]);
            }
            else
            {
                Assert.False(flags[i]);
            }
        }
    }

    [Fact]
    public void OneOfWith4CasesTracksEachCase()
    {
        var u1 = new OneOf<int, string, double, char>(1);
        AssertOnlyFlag(1, u1.IsT1, u1.IsT2, u1.IsT3, u1.IsT4);
        Assert.Equal(1, u1.AsT1());
        Assert.Equal((object)1, u1.Value);
        Assert.True(u1.HasValue);
        Assert.Equal((1).ToString(), u1.ToString());
        Assert.True(u1.TryGetValue(out int g1));
        Assert.Equal(1, g1);

        var u2 = new OneOf<int, string, double, char>("two");
        AssertOnlyFlag(2, u2.IsT1, u2.IsT2, u2.IsT3, u2.IsT4);
        Assert.Equal("two", u2.AsT2());
        Assert.Equal((object)"two", u2.Value);
        Assert.True(u2.HasValue);
        Assert.Equal(("two").ToString(), u2.ToString());
        Assert.True(u2.TryGetValue(out string g2));
        Assert.Equal("two", g2);

        var u3 = new OneOf<int, string, double, char>(3d);
        AssertOnlyFlag(3, u3.IsT1, u3.IsT2, u3.IsT3, u3.IsT4);
        Assert.Equal(3d, u3.AsT3());
        Assert.Equal((object)3d, u3.Value);
        Assert.True(u3.HasValue);
        Assert.Equal((3d).ToString(), u3.ToString());
        Assert.True(u3.TryGetValue(out double g3));
        Assert.Equal(3d, g3);

        var u4 = new OneOf<int, string, double, char>('4');
        AssertOnlyFlag(4, u4.IsT1, u4.IsT2, u4.IsT3, u4.IsT4);
        Assert.Equal('4', u4.AsT4());
        Assert.Equal((object)'4', u4.Value);
        Assert.True(u4.HasValue);
        Assert.Equal(('4').ToString(), u4.ToString());
        Assert.True(u4.TryGetValue(out char g4));
        Assert.Equal('4', g4);
    }

    [Fact]
    public void OneOfWith4CasesDefaultIsEmpty()
    {
        var u = default(OneOf<int, string, double, char>);
        AssertOnlyFlag(0, u.IsT1, u.IsT2, u.IsT3, u.IsT4);
        Assert.False(u.HasValue);
        Assert.Null(u.Value);
        Assert.Equal(LaquaiLibUnion.NoValue, u.ToString());
        Assert.Equal(0, u.GetHashCode());
        Assert.True(u.Equals(default(OneOf<int, string, double, char>)));
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT1(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT2(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT3(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT4(); });
        Assert.False(u.TryGetValue(out int e1));
        Assert.False(u.TryGetValue(out string e2));
        Assert.False(u.TryGetValue(out double e3));
        Assert.False(u.TryGetValue(out char e4));
    }

    [Fact]
    public void OneOfWith5CasesTracksEachCase()
    {
        var u1 = new OneOf<int, string, double, char, bool>(1);
        AssertOnlyFlag(1, u1.IsT1, u1.IsT2, u1.IsT3, u1.IsT4, u1.IsT5);
        Assert.Equal(1, u1.AsT1());
        Assert.Equal((object)1, u1.Value);
        Assert.True(u1.HasValue);
        Assert.Equal((1).ToString(), u1.ToString());
        Assert.True(u1.TryGetValue(out int g1));
        Assert.Equal(1, g1);

        var u2 = new OneOf<int, string, double, char, bool>("two");
        AssertOnlyFlag(2, u2.IsT1, u2.IsT2, u2.IsT3, u2.IsT4, u2.IsT5);
        Assert.Equal("two", u2.AsT2());
        Assert.Equal((object)"two", u2.Value);
        Assert.True(u2.HasValue);
        Assert.Equal(("two").ToString(), u2.ToString());
        Assert.True(u2.TryGetValue(out string g2));
        Assert.Equal("two", g2);

        var u3 = new OneOf<int, string, double, char, bool>(3d);
        AssertOnlyFlag(3, u3.IsT1, u3.IsT2, u3.IsT3, u3.IsT4, u3.IsT5);
        Assert.Equal(3d, u3.AsT3());
        Assert.Equal((object)3d, u3.Value);
        Assert.True(u3.HasValue);
        Assert.Equal((3d).ToString(), u3.ToString());
        Assert.True(u3.TryGetValue(out double g3));
        Assert.Equal(3d, g3);

        var u4 = new OneOf<int, string, double, char, bool>('4');
        AssertOnlyFlag(4, u4.IsT1, u4.IsT2, u4.IsT3, u4.IsT4, u4.IsT5);
        Assert.Equal('4', u4.AsT4());
        Assert.Equal((object)'4', u4.Value);
        Assert.True(u4.HasValue);
        Assert.Equal(('4').ToString(), u4.ToString());
        Assert.True(u4.TryGetValue(out char g4));
        Assert.Equal('4', g4);

        var u5 = new OneOf<int, string, double, char, bool>(true);
        AssertOnlyFlag(5, u5.IsT1, u5.IsT2, u5.IsT3, u5.IsT4, u5.IsT5);
        Assert.True(u5.AsT5());
        Assert.Equal((object)true, u5.Value);
        Assert.True(u5.HasValue);
        Assert.Equal((true).ToString(), u5.ToString());
        Assert.True(u5.TryGetValue(out bool g5));
        Assert.True(g5);
    }

    [Fact]
    public void OneOfWith5CasesDefaultIsEmpty()
    {
        var u = default(OneOf<int, string, double, char, bool>);
        AssertOnlyFlag(0, u.IsT1, u.IsT2, u.IsT3, u.IsT4, u.IsT5);
        Assert.False(u.HasValue);
        Assert.Null(u.Value);
        Assert.Equal(LaquaiLibUnion.NoValue, u.ToString());
        Assert.Equal(0, u.GetHashCode());
        Assert.True(u.Equals(default(OneOf<int, string, double, char, bool>)));
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT1(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT2(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT3(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT4(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT5(); });
        Assert.False(u.TryGetValue(out int e1));
        Assert.False(u.TryGetValue(out string e2));
        Assert.False(u.TryGetValue(out double e3));
        Assert.False(u.TryGetValue(out char e4));
        Assert.False(u.TryGetValue(out bool e5));
    }

    [Fact]
    public void OneOfWith6CasesTracksEachCase()
    {
        var u1 = new OneOf<int, string, double, char, bool, long>(1);
        AssertOnlyFlag(1, u1.IsT1, u1.IsT2, u1.IsT3, u1.IsT4, u1.IsT5, u1.IsT6);
        Assert.Equal(1, u1.AsT1());
        Assert.Equal((object)1, u1.Value);
        Assert.True(u1.HasValue);
        Assert.Equal((1).ToString(), u1.ToString());
        Assert.True(u1.TryGetValue(out int g1));
        Assert.Equal(1, g1);

        var u2 = new OneOf<int, string, double, char, bool, long>("two");
        AssertOnlyFlag(2, u2.IsT1, u2.IsT2, u2.IsT3, u2.IsT4, u2.IsT5, u2.IsT6);
        Assert.Equal("two", u2.AsT2());
        Assert.Equal((object)"two", u2.Value);
        Assert.True(u2.HasValue);
        Assert.Equal(("two").ToString(), u2.ToString());
        Assert.True(u2.TryGetValue(out string g2));
        Assert.Equal("two", g2);

        var u3 = new OneOf<int, string, double, char, bool, long>(3d);
        AssertOnlyFlag(3, u3.IsT1, u3.IsT2, u3.IsT3, u3.IsT4, u3.IsT5, u3.IsT6);
        Assert.Equal(3d, u3.AsT3());
        Assert.Equal((object)3d, u3.Value);
        Assert.True(u3.HasValue);
        Assert.Equal((3d).ToString(), u3.ToString());
        Assert.True(u3.TryGetValue(out double g3));
        Assert.Equal(3d, g3);

        var u4 = new OneOf<int, string, double, char, bool, long>('4');
        AssertOnlyFlag(4, u4.IsT1, u4.IsT2, u4.IsT3, u4.IsT4, u4.IsT5, u4.IsT6);
        Assert.Equal('4', u4.AsT4());
        Assert.Equal((object)'4', u4.Value);
        Assert.True(u4.HasValue);
        Assert.Equal(('4').ToString(), u4.ToString());
        Assert.True(u4.TryGetValue(out char g4));
        Assert.Equal('4', g4);

        var u5 = new OneOf<int, string, double, char, bool, long>(true);
        AssertOnlyFlag(5, u5.IsT1, u5.IsT2, u5.IsT3, u5.IsT4, u5.IsT5, u5.IsT6);
        Assert.True(u5.AsT5());
        Assert.Equal((object)true, u5.Value);
        Assert.True(u5.HasValue);
        Assert.Equal((true).ToString(), u5.ToString());
        Assert.True(u5.TryGetValue(out bool g5));
        Assert.True(g5);

        var u6 = new OneOf<int, string, double, char, bool, long>(6L);
        AssertOnlyFlag(6, u6.IsT1, u6.IsT2, u6.IsT3, u6.IsT4, u6.IsT5, u6.IsT6);
        Assert.Equal(6L, u6.AsT6());
        Assert.Equal((object)6L, u6.Value);
        Assert.True(u6.HasValue);
        Assert.Equal((6L).ToString(), u6.ToString());
        Assert.True(u6.TryGetValue(out long g6));
        Assert.Equal(6L, g6);
    }

    [Fact]
    public void OneOfWith6CasesDefaultIsEmpty()
    {
        var u = default(OneOf<int, string, double, char, bool, long>);
        AssertOnlyFlag(0, u.IsT1, u.IsT2, u.IsT3, u.IsT4, u.IsT5, u.IsT6);
        Assert.False(u.HasValue);
        Assert.Null(u.Value);
        Assert.Equal(LaquaiLibUnion.NoValue, u.ToString());
        Assert.Equal(0, u.GetHashCode());
        Assert.True(u.Equals(default(OneOf<int, string, double, char, bool, long>)));
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT1(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT2(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT3(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT4(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT5(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT6(); });
        Assert.False(u.TryGetValue(out int e1));
        Assert.False(u.TryGetValue(out string e2));
        Assert.False(u.TryGetValue(out double e3));
        Assert.False(u.TryGetValue(out char e4));
        Assert.False(u.TryGetValue(out bool e5));
        Assert.False(u.TryGetValue(out long e6));
    }

    [Fact]
    public void OneOfWith7CasesTracksEachCase()
    {
        var u1 = new OneOf<int, string, double, char, bool, long, byte>(1);
        AssertOnlyFlag(1, u1.IsT1, u1.IsT2, u1.IsT3, u1.IsT4, u1.IsT5, u1.IsT6, u1.IsT7);
        Assert.Equal(1, u1.AsT1());
        Assert.Equal((object)1, u1.Value);
        Assert.True(u1.HasValue);
        Assert.Equal((1).ToString(), u1.ToString());
        Assert.True(u1.TryGetValue(out int g1));
        Assert.Equal(1, g1);

        var u2 = new OneOf<int, string, double, char, bool, long, byte>("two");
        AssertOnlyFlag(2, u2.IsT1, u2.IsT2, u2.IsT3, u2.IsT4, u2.IsT5, u2.IsT6, u2.IsT7);
        Assert.Equal("two", u2.AsT2());
        Assert.Equal((object)"two", u2.Value);
        Assert.True(u2.HasValue);
        Assert.Equal(("two").ToString(), u2.ToString());
        Assert.True(u2.TryGetValue(out string g2));
        Assert.Equal("two", g2);

        var u3 = new OneOf<int, string, double, char, bool, long, byte>(3d);
        AssertOnlyFlag(3, u3.IsT1, u3.IsT2, u3.IsT3, u3.IsT4, u3.IsT5, u3.IsT6, u3.IsT7);
        Assert.Equal(3d, u3.AsT3());
        Assert.Equal((object)3d, u3.Value);
        Assert.True(u3.HasValue);
        Assert.Equal((3d).ToString(), u3.ToString());
        Assert.True(u3.TryGetValue(out double g3));
        Assert.Equal(3d, g3);

        var u4 = new OneOf<int, string, double, char, bool, long, byte>('4');
        AssertOnlyFlag(4, u4.IsT1, u4.IsT2, u4.IsT3, u4.IsT4, u4.IsT5, u4.IsT6, u4.IsT7);
        Assert.Equal('4', u4.AsT4());
        Assert.Equal((object)'4', u4.Value);
        Assert.True(u4.HasValue);
        Assert.Equal(('4').ToString(), u4.ToString());
        Assert.True(u4.TryGetValue(out char g4));
        Assert.Equal('4', g4);

        var u5 = new OneOf<int, string, double, char, bool, long, byte>(true);
        AssertOnlyFlag(5, u5.IsT1, u5.IsT2, u5.IsT3, u5.IsT4, u5.IsT5, u5.IsT6, u5.IsT7);
        Assert.True(u5.AsT5());
        Assert.Equal((object)true, u5.Value);
        Assert.True(u5.HasValue);
        Assert.Equal((true).ToString(), u5.ToString());
        Assert.True(u5.TryGetValue(out bool g5));
        Assert.True(g5);

        var u6 = new OneOf<int, string, double, char, bool, long, byte>(6L);
        AssertOnlyFlag(6, u6.IsT1, u6.IsT2, u6.IsT3, u6.IsT4, u6.IsT5, u6.IsT6, u6.IsT7);
        Assert.Equal(6L, u6.AsT6());
        Assert.Equal((object)6L, u6.Value);
        Assert.True(u6.HasValue);
        Assert.Equal((6L).ToString(), u6.ToString());
        Assert.True(u6.TryGetValue(out long g6));
        Assert.Equal(6L, g6);

        var u7 = new OneOf<int, string, double, char, bool, long, byte>((byte)7);
        AssertOnlyFlag(7, u7.IsT1, u7.IsT2, u7.IsT3, u7.IsT4, u7.IsT5, u7.IsT6, u7.IsT7);
        Assert.Equal((byte)7, u7.AsT7());
        Assert.Equal((object)(byte)7, u7.Value);
        Assert.True(u7.HasValue);
        Assert.Equal(((byte)7).ToString(), u7.ToString());
        Assert.True(u7.TryGetValue(out byte g7));
        Assert.Equal((byte)7, g7);
    }

    [Fact]
    public void OneOfWith7CasesDefaultIsEmpty()
    {
        var u = default(OneOf<int, string, double, char, bool, long, byte>);
        AssertOnlyFlag(0, u.IsT1, u.IsT2, u.IsT3, u.IsT4, u.IsT5, u.IsT6, u.IsT7);
        Assert.False(u.HasValue);
        Assert.Null(u.Value);
        Assert.Equal(LaquaiLibUnion.NoValue, u.ToString());
        Assert.Equal(0, u.GetHashCode());
        Assert.True(u.Equals(default(OneOf<int, string, double, char, bool, long, byte>)));
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT1(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT2(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT3(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT4(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT5(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT6(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT7(); });
        Assert.False(u.TryGetValue(out int e1));
        Assert.False(u.TryGetValue(out string e2));
        Assert.False(u.TryGetValue(out double e3));
        Assert.False(u.TryGetValue(out char e4));
        Assert.False(u.TryGetValue(out bool e5));
        Assert.False(u.TryGetValue(out long e6));
        Assert.False(u.TryGetValue(out byte e7));
    }

    [Fact]
    public void OneOfWith8CasesTracksEachCase()
    {
        var u1 = new OneOf<int, string, double, char, bool, long, byte, short>(1);
        AssertOnlyFlag(1, u1.IsT1, u1.IsT2, u1.IsT3, u1.IsT4, u1.IsT5, u1.IsT6, u1.IsT7, u1.IsT8);
        Assert.Equal(1, u1.AsT1());
        Assert.Equal((object)1, u1.Value);
        Assert.True(u1.HasValue);
        Assert.Equal((1).ToString(), u1.ToString());
        Assert.True(u1.TryGetValue(out int g1));
        Assert.Equal(1, g1);

        var u2 = new OneOf<int, string, double, char, bool, long, byte, short>("two");
        AssertOnlyFlag(2, u2.IsT1, u2.IsT2, u2.IsT3, u2.IsT4, u2.IsT5, u2.IsT6, u2.IsT7, u2.IsT8);
        Assert.Equal("two", u2.AsT2());
        Assert.Equal((object)"two", u2.Value);
        Assert.True(u2.HasValue);
        Assert.Equal(("two").ToString(), u2.ToString());
        Assert.True(u2.TryGetValue(out string g2));
        Assert.Equal("two", g2);

        var u3 = new OneOf<int, string, double, char, bool, long, byte, short>(3d);
        AssertOnlyFlag(3, u3.IsT1, u3.IsT2, u3.IsT3, u3.IsT4, u3.IsT5, u3.IsT6, u3.IsT7, u3.IsT8);
        Assert.Equal(3d, u3.AsT3());
        Assert.Equal((object)3d, u3.Value);
        Assert.True(u3.HasValue);
        Assert.Equal((3d).ToString(), u3.ToString());
        Assert.True(u3.TryGetValue(out double g3));
        Assert.Equal(3d, g3);

        var u4 = new OneOf<int, string, double, char, bool, long, byte, short>('4');
        AssertOnlyFlag(4, u4.IsT1, u4.IsT2, u4.IsT3, u4.IsT4, u4.IsT5, u4.IsT6, u4.IsT7, u4.IsT8);
        Assert.Equal('4', u4.AsT4());
        Assert.Equal((object)'4', u4.Value);
        Assert.True(u4.HasValue);
        Assert.Equal(('4').ToString(), u4.ToString());
        Assert.True(u4.TryGetValue(out char g4));
        Assert.Equal('4', g4);

        var u5 = new OneOf<int, string, double, char, bool, long, byte, short>(true);
        AssertOnlyFlag(5, u5.IsT1, u5.IsT2, u5.IsT3, u5.IsT4, u5.IsT5, u5.IsT6, u5.IsT7, u5.IsT8);
        Assert.True(u5.AsT5());
        Assert.Equal((object)true, u5.Value);
        Assert.True(u5.HasValue);
        Assert.Equal((true).ToString(), u5.ToString());
        Assert.True(u5.TryGetValue(out bool g5));
        Assert.True(g5);

        var u6 = new OneOf<int, string, double, char, bool, long, byte, short>(6L);
        AssertOnlyFlag(6, u6.IsT1, u6.IsT2, u6.IsT3, u6.IsT4, u6.IsT5, u6.IsT6, u6.IsT7, u6.IsT8);
        Assert.Equal(6L, u6.AsT6());
        Assert.Equal((object)6L, u6.Value);
        Assert.True(u6.HasValue);
        Assert.Equal((6L).ToString(), u6.ToString());
        Assert.True(u6.TryGetValue(out long g6));
        Assert.Equal(6L, g6);

        var u7 = new OneOf<int, string, double, char, bool, long, byte, short>((byte)7);
        AssertOnlyFlag(7, u7.IsT1, u7.IsT2, u7.IsT3, u7.IsT4, u7.IsT5, u7.IsT6, u7.IsT7, u7.IsT8);
        Assert.Equal((byte)7, u7.AsT7());
        Assert.Equal((object)(byte)7, u7.Value);
        Assert.True(u7.HasValue);
        Assert.Equal(((byte)7).ToString(), u7.ToString());
        Assert.True(u7.TryGetValue(out byte g7));
        Assert.Equal((byte)7, g7);

        var u8 = new OneOf<int, string, double, char, bool, long, byte, short>((short)8);
        AssertOnlyFlag(8, u8.IsT1, u8.IsT2, u8.IsT3, u8.IsT4, u8.IsT5, u8.IsT6, u8.IsT7, u8.IsT8);
        Assert.Equal((short)8, u8.AsT8());
        Assert.Equal((object)(short)8, u8.Value);
        Assert.True(u8.HasValue);
        Assert.Equal(((short)8).ToString(), u8.ToString());
        Assert.True(u8.TryGetValue(out short g8));
        Assert.Equal((short)8, g8);
    }

    [Fact]
    public void OneOfWith8CasesDefaultIsEmpty()
    {
        var u = default(OneOf<int, string, double, char, bool, long, byte, short>);
        AssertOnlyFlag(0, u.IsT1, u.IsT2, u.IsT3, u.IsT4, u.IsT5, u.IsT6, u.IsT7, u.IsT8);
        Assert.False(u.HasValue);
        Assert.Null(u.Value);
        Assert.Equal(LaquaiLibUnion.NoValue, u.ToString());
        Assert.Equal(0, u.GetHashCode());
        Assert.True(u.Equals(default(OneOf<int, string, double, char, bool, long, byte, short>)));
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT1(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT2(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT3(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT4(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT5(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT6(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT7(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT8(); });
        Assert.False(u.TryGetValue(out int e1));
        Assert.False(u.TryGetValue(out string e2));
        Assert.False(u.TryGetValue(out double e3));
        Assert.False(u.TryGetValue(out char e4));
        Assert.False(u.TryGetValue(out bool e5));
        Assert.False(u.TryGetValue(out long e6));
        Assert.False(u.TryGetValue(out byte e7));
        Assert.False(u.TryGetValue(out short e8));
    }

    [Fact]
    public void OneOfWith9CasesTracksEachCase()
    {
        var u1 = new OneOf<int, string, double, char, bool, long, byte, short, float>(1);
        AssertOnlyFlag(1, u1.IsT1, u1.IsT2, u1.IsT3, u1.IsT4, u1.IsT5, u1.IsT6, u1.IsT7, u1.IsT8, u1.IsT9);
        Assert.Equal(1, u1.AsT1());
        Assert.Equal((object)1, u1.Value);
        Assert.True(u1.HasValue);
        Assert.Equal((1).ToString(), u1.ToString());
        Assert.True(u1.TryGetValue(out int g1));
        Assert.Equal(1, g1);

        var u2 = new OneOf<int, string, double, char, bool, long, byte, short, float>("two");
        AssertOnlyFlag(2, u2.IsT1, u2.IsT2, u2.IsT3, u2.IsT4, u2.IsT5, u2.IsT6, u2.IsT7, u2.IsT8, u2.IsT9);
        Assert.Equal("two", u2.AsT2());
        Assert.Equal((object)"two", u2.Value);
        Assert.True(u2.HasValue);
        Assert.Equal(("two").ToString(), u2.ToString());
        Assert.True(u2.TryGetValue(out string g2));
        Assert.Equal("two", g2);

        var u3 = new OneOf<int, string, double, char, bool, long, byte, short, float>(3d);
        AssertOnlyFlag(3, u3.IsT1, u3.IsT2, u3.IsT3, u3.IsT4, u3.IsT5, u3.IsT6, u3.IsT7, u3.IsT8, u3.IsT9);
        Assert.Equal(3d, u3.AsT3());
        Assert.Equal((object)3d, u3.Value);
        Assert.True(u3.HasValue);
        Assert.Equal((3d).ToString(), u3.ToString());
        Assert.True(u3.TryGetValue(out double g3));
        Assert.Equal(3d, g3);

        var u4 = new OneOf<int, string, double, char, bool, long, byte, short, float>('4');
        AssertOnlyFlag(4, u4.IsT1, u4.IsT2, u4.IsT3, u4.IsT4, u4.IsT5, u4.IsT6, u4.IsT7, u4.IsT8, u4.IsT9);
        Assert.Equal('4', u4.AsT4());
        Assert.Equal((object)'4', u4.Value);
        Assert.True(u4.HasValue);
        Assert.Equal(('4').ToString(), u4.ToString());
        Assert.True(u4.TryGetValue(out char g4));
        Assert.Equal('4', g4);

        var u5 = new OneOf<int, string, double, char, bool, long, byte, short, float>(true);
        AssertOnlyFlag(5, u5.IsT1, u5.IsT2, u5.IsT3, u5.IsT4, u5.IsT5, u5.IsT6, u5.IsT7, u5.IsT8, u5.IsT9);
        Assert.True(u5.AsT5());
        Assert.Equal((object)true, u5.Value);
        Assert.True(u5.HasValue);
        Assert.Equal((true).ToString(), u5.ToString());
        Assert.True(u5.TryGetValue(out bool g5));
        Assert.True(g5);

        var u6 = new OneOf<int, string, double, char, bool, long, byte, short, float>(6L);
        AssertOnlyFlag(6, u6.IsT1, u6.IsT2, u6.IsT3, u6.IsT4, u6.IsT5, u6.IsT6, u6.IsT7, u6.IsT8, u6.IsT9);
        Assert.Equal(6L, u6.AsT6());
        Assert.Equal((object)6L, u6.Value);
        Assert.True(u6.HasValue);
        Assert.Equal((6L).ToString(), u6.ToString());
        Assert.True(u6.TryGetValue(out long g6));
        Assert.Equal(6L, g6);

        var u7 = new OneOf<int, string, double, char, bool, long, byte, short, float>((byte)7);
        AssertOnlyFlag(7, u7.IsT1, u7.IsT2, u7.IsT3, u7.IsT4, u7.IsT5, u7.IsT6, u7.IsT7, u7.IsT8, u7.IsT9);
        Assert.Equal((byte)7, u7.AsT7());
        Assert.Equal((object)(byte)7, u7.Value);
        Assert.True(u7.HasValue);
        Assert.Equal(((byte)7).ToString(), u7.ToString());
        Assert.True(u7.TryGetValue(out byte g7));
        Assert.Equal((byte)7, g7);

        var u8 = new OneOf<int, string, double, char, bool, long, byte, short, float>((short)8);
        AssertOnlyFlag(8, u8.IsT1, u8.IsT2, u8.IsT3, u8.IsT4, u8.IsT5, u8.IsT6, u8.IsT7, u8.IsT8, u8.IsT9);
        Assert.Equal((short)8, u8.AsT8());
        Assert.Equal((object)(short)8, u8.Value);
        Assert.True(u8.HasValue);
        Assert.Equal(((short)8).ToString(), u8.ToString());
        Assert.True(u8.TryGetValue(out short g8));
        Assert.Equal((short)8, g8);

        var u9 = new OneOf<int, string, double, char, bool, long, byte, short, float>(9f);
        AssertOnlyFlag(9, u9.IsT1, u9.IsT2, u9.IsT3, u9.IsT4, u9.IsT5, u9.IsT6, u9.IsT7, u9.IsT8, u9.IsT9);
        Assert.Equal(9f, u9.AsT9());
        Assert.Equal((object)9f, u9.Value);
        Assert.True(u9.HasValue);
        Assert.Equal((9f).ToString(), u9.ToString());
        Assert.True(u9.TryGetValue(out float g9));
        Assert.Equal(9f, g9);
    }

    [Fact]
    public void OneOfWith9CasesDefaultIsEmpty()
    {
        var u = default(OneOf<int, string, double, char, bool, long, byte, short, float>);
        AssertOnlyFlag(0, u.IsT1, u.IsT2, u.IsT3, u.IsT4, u.IsT5, u.IsT6, u.IsT7, u.IsT8, u.IsT9);
        Assert.False(u.HasValue);
        Assert.Null(u.Value);
        Assert.Equal(LaquaiLibUnion.NoValue, u.ToString());
        Assert.Equal(0, u.GetHashCode());
        Assert.True(u.Equals(default(OneOf<int, string, double, char, bool, long, byte, short, float>)));
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT1(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT2(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT3(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT4(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT5(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT6(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT7(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT8(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT9(); });
        Assert.False(u.TryGetValue(out int e1));
        Assert.False(u.TryGetValue(out string e2));
        Assert.False(u.TryGetValue(out double e3));
        Assert.False(u.TryGetValue(out char e4));
        Assert.False(u.TryGetValue(out bool e5));
        Assert.False(u.TryGetValue(out long e6));
        Assert.False(u.TryGetValue(out byte e7));
        Assert.False(u.TryGetValue(out short e8));
        Assert.False(u.TryGetValue(out float e9));
    }

    [Fact]
    public void OneOfWith10CasesTracksEachCase()
    {
        var u1 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal>(1);
        AssertOnlyFlag(1, u1.IsT1, u1.IsT2, u1.IsT3, u1.IsT4, u1.IsT5, u1.IsT6, u1.IsT7, u1.IsT8, u1.IsT9, u1.IsT10);
        Assert.Equal(1, u1.AsT1());
        Assert.Equal((object)1, u1.Value);
        Assert.True(u1.HasValue);
        Assert.Equal((1).ToString(), u1.ToString());
        Assert.True(u1.TryGetValue(out int g1));
        Assert.Equal(1, g1);

        var u2 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal>("two");
        AssertOnlyFlag(2, u2.IsT1, u2.IsT2, u2.IsT3, u2.IsT4, u2.IsT5, u2.IsT6, u2.IsT7, u2.IsT8, u2.IsT9, u2.IsT10);
        Assert.Equal("two", u2.AsT2());
        Assert.Equal((object)"two", u2.Value);
        Assert.True(u2.HasValue);
        Assert.Equal(("two").ToString(), u2.ToString());
        Assert.True(u2.TryGetValue(out string g2));
        Assert.Equal("two", g2);

        var u3 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal>(3d);
        AssertOnlyFlag(3, u3.IsT1, u3.IsT2, u3.IsT3, u3.IsT4, u3.IsT5, u3.IsT6, u3.IsT7, u3.IsT8, u3.IsT9, u3.IsT10);
        Assert.Equal(3d, u3.AsT3());
        Assert.Equal((object)3d, u3.Value);
        Assert.True(u3.HasValue);
        Assert.Equal((3d).ToString(), u3.ToString());
        Assert.True(u3.TryGetValue(out double g3));
        Assert.Equal(3d, g3);

        var u4 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal>('4');
        AssertOnlyFlag(4, u4.IsT1, u4.IsT2, u4.IsT3, u4.IsT4, u4.IsT5, u4.IsT6, u4.IsT7, u4.IsT8, u4.IsT9, u4.IsT10);
        Assert.Equal('4', u4.AsT4());
        Assert.Equal((object)'4', u4.Value);
        Assert.True(u4.HasValue);
        Assert.Equal(('4').ToString(), u4.ToString());
        Assert.True(u4.TryGetValue(out char g4));
        Assert.Equal('4', g4);

        var u5 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal>(true);
        AssertOnlyFlag(5, u5.IsT1, u5.IsT2, u5.IsT3, u5.IsT4, u5.IsT5, u5.IsT6, u5.IsT7, u5.IsT8, u5.IsT9, u5.IsT10);
        Assert.True(u5.AsT5());
        Assert.Equal((object)true, u5.Value);
        Assert.True(u5.HasValue);
        Assert.Equal((true).ToString(), u5.ToString());
        Assert.True(u5.TryGetValue(out bool g5));
        Assert.True(g5);

        var u6 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal>(6L);
        AssertOnlyFlag(6, u6.IsT1, u6.IsT2, u6.IsT3, u6.IsT4, u6.IsT5, u6.IsT6, u6.IsT7, u6.IsT8, u6.IsT9, u6.IsT10);
        Assert.Equal(6L, u6.AsT6());
        Assert.Equal((object)6L, u6.Value);
        Assert.True(u6.HasValue);
        Assert.Equal((6L).ToString(), u6.ToString());
        Assert.True(u6.TryGetValue(out long g6));
        Assert.Equal(6L, g6);

        var u7 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal>((byte)7);
        AssertOnlyFlag(7, u7.IsT1, u7.IsT2, u7.IsT3, u7.IsT4, u7.IsT5, u7.IsT6, u7.IsT7, u7.IsT8, u7.IsT9, u7.IsT10);
        Assert.Equal((byte)7, u7.AsT7());
        Assert.Equal((object)(byte)7, u7.Value);
        Assert.True(u7.HasValue);
        Assert.Equal(((byte)7).ToString(), u7.ToString());
        Assert.True(u7.TryGetValue(out byte g7));
        Assert.Equal((byte)7, g7);

        var u8 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal>((short)8);
        AssertOnlyFlag(8, u8.IsT1, u8.IsT2, u8.IsT3, u8.IsT4, u8.IsT5, u8.IsT6, u8.IsT7, u8.IsT8, u8.IsT9, u8.IsT10);
        Assert.Equal((short)8, u8.AsT8());
        Assert.Equal((object)(short)8, u8.Value);
        Assert.True(u8.HasValue);
        Assert.Equal(((short)8).ToString(), u8.ToString());
        Assert.True(u8.TryGetValue(out short g8));
        Assert.Equal((short)8, g8);

        var u9 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal>(9f);
        AssertOnlyFlag(9, u9.IsT1, u9.IsT2, u9.IsT3, u9.IsT4, u9.IsT5, u9.IsT6, u9.IsT7, u9.IsT8, u9.IsT9, u9.IsT10);
        Assert.Equal(9f, u9.AsT9());
        Assert.Equal((object)9f, u9.Value);
        Assert.True(u9.HasValue);
        Assert.Equal((9f).ToString(), u9.ToString());
        Assert.True(u9.TryGetValue(out float g9));
        Assert.Equal(9f, g9);

        var u10 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal>(10m);
        AssertOnlyFlag(10, u10.IsT1, u10.IsT2, u10.IsT3, u10.IsT4, u10.IsT5, u10.IsT6, u10.IsT7, u10.IsT8, u10.IsT9, u10.IsT10);
        Assert.Equal(10m, u10.AsT10());
        Assert.Equal((object)10m, u10.Value);
        Assert.True(u10.HasValue);
        Assert.Equal((10m).ToString(), u10.ToString());
        Assert.True(u10.TryGetValue(out decimal g10));
        Assert.Equal(10m, g10);
    }

    [Fact]
    public void OneOfWith10CasesDefaultIsEmpty()
    {
        var u = default(OneOf<int, string, double, char, bool, long, byte, short, float, decimal>);
        AssertOnlyFlag(0, u.IsT1, u.IsT2, u.IsT3, u.IsT4, u.IsT5, u.IsT6, u.IsT7, u.IsT8, u.IsT9, u.IsT10);
        Assert.False(u.HasValue);
        Assert.Null(u.Value);
        Assert.Equal(LaquaiLibUnion.NoValue, u.ToString());
        Assert.Equal(0, u.GetHashCode());
        Assert.True(u.Equals(default(OneOf<int, string, double, char, bool, long, byte, short, float, decimal>)));
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT1(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT2(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT3(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT4(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT5(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT6(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT7(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT8(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT9(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT10(); });
        Assert.False(u.TryGetValue(out int e1));
        Assert.False(u.TryGetValue(out string e2));
        Assert.False(u.TryGetValue(out double e3));
        Assert.False(u.TryGetValue(out char e4));
        Assert.False(u.TryGetValue(out bool e5));
        Assert.False(u.TryGetValue(out long e6));
        Assert.False(u.TryGetValue(out byte e7));
        Assert.False(u.TryGetValue(out short e8));
        Assert.False(u.TryGetValue(out float e9));
        Assert.False(u.TryGetValue(out decimal e10));
    }

    [Fact]
    public void OneOfWith11CasesTracksEachCase()
    {
        var u1 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint>(1);
        AssertOnlyFlag(1, u1.IsT1, u1.IsT2, u1.IsT3, u1.IsT4, u1.IsT5, u1.IsT6, u1.IsT7, u1.IsT8, u1.IsT9, u1.IsT10, u1.IsT11);
        Assert.Equal(1, u1.AsT1());
        Assert.Equal((object)1, u1.Value);
        Assert.True(u1.HasValue);
        Assert.Equal((1).ToString(), u1.ToString());
        Assert.True(u1.TryGetValue(out int g1));
        Assert.Equal(1, g1);

        var u2 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint>("two");
        AssertOnlyFlag(2, u2.IsT1, u2.IsT2, u2.IsT3, u2.IsT4, u2.IsT5, u2.IsT6, u2.IsT7, u2.IsT8, u2.IsT9, u2.IsT10, u2.IsT11);
        Assert.Equal("two", u2.AsT2());
        Assert.Equal((object)"two", u2.Value);
        Assert.True(u2.HasValue);
        Assert.Equal(("two").ToString(), u2.ToString());
        Assert.True(u2.TryGetValue(out string g2));
        Assert.Equal("two", g2);

        var u3 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint>(3d);
        AssertOnlyFlag(3, u3.IsT1, u3.IsT2, u3.IsT3, u3.IsT4, u3.IsT5, u3.IsT6, u3.IsT7, u3.IsT8, u3.IsT9, u3.IsT10, u3.IsT11);
        Assert.Equal(3d, u3.AsT3());
        Assert.Equal((object)3d, u3.Value);
        Assert.True(u3.HasValue);
        Assert.Equal((3d).ToString(), u3.ToString());
        Assert.True(u3.TryGetValue(out double g3));
        Assert.Equal(3d, g3);

        var u4 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint>('4');
        AssertOnlyFlag(4, u4.IsT1, u4.IsT2, u4.IsT3, u4.IsT4, u4.IsT5, u4.IsT6, u4.IsT7, u4.IsT8, u4.IsT9, u4.IsT10, u4.IsT11);
        Assert.Equal('4', u4.AsT4());
        Assert.Equal((object)'4', u4.Value);
        Assert.True(u4.HasValue);
        Assert.Equal(('4').ToString(), u4.ToString());
        Assert.True(u4.TryGetValue(out char g4));
        Assert.Equal('4', g4);

        var u5 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint>(true);
        AssertOnlyFlag(5, u5.IsT1, u5.IsT2, u5.IsT3, u5.IsT4, u5.IsT5, u5.IsT6, u5.IsT7, u5.IsT8, u5.IsT9, u5.IsT10, u5.IsT11);
        Assert.True(u5.AsT5());
        Assert.Equal((object)true, u5.Value);
        Assert.True(u5.HasValue);
        Assert.Equal((true).ToString(), u5.ToString());
        Assert.True(u5.TryGetValue(out bool g5));
        Assert.True(g5);

        var u6 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint>(6L);
        AssertOnlyFlag(6, u6.IsT1, u6.IsT2, u6.IsT3, u6.IsT4, u6.IsT5, u6.IsT6, u6.IsT7, u6.IsT8, u6.IsT9, u6.IsT10, u6.IsT11);
        Assert.Equal(6L, u6.AsT6());
        Assert.Equal((object)6L, u6.Value);
        Assert.True(u6.HasValue);
        Assert.Equal((6L).ToString(), u6.ToString());
        Assert.True(u6.TryGetValue(out long g6));
        Assert.Equal(6L, g6);

        var u7 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint>((byte)7);
        AssertOnlyFlag(7, u7.IsT1, u7.IsT2, u7.IsT3, u7.IsT4, u7.IsT5, u7.IsT6, u7.IsT7, u7.IsT8, u7.IsT9, u7.IsT10, u7.IsT11);
        Assert.Equal((byte)7, u7.AsT7());
        Assert.Equal((object)(byte)7, u7.Value);
        Assert.True(u7.HasValue);
        Assert.Equal(((byte)7).ToString(), u7.ToString());
        Assert.True(u7.TryGetValue(out byte g7));
        Assert.Equal((byte)7, g7);

        var u8 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint>((short)8);
        AssertOnlyFlag(8, u8.IsT1, u8.IsT2, u8.IsT3, u8.IsT4, u8.IsT5, u8.IsT6, u8.IsT7, u8.IsT8, u8.IsT9, u8.IsT10, u8.IsT11);
        Assert.Equal((short)8, u8.AsT8());
        Assert.Equal((object)(short)8, u8.Value);
        Assert.True(u8.HasValue);
        Assert.Equal(((short)8).ToString(), u8.ToString());
        Assert.True(u8.TryGetValue(out short g8));
        Assert.Equal((short)8, g8);

        var u9 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint>(9f);
        AssertOnlyFlag(9, u9.IsT1, u9.IsT2, u9.IsT3, u9.IsT4, u9.IsT5, u9.IsT6, u9.IsT7, u9.IsT8, u9.IsT9, u9.IsT10, u9.IsT11);
        Assert.Equal(9f, u9.AsT9());
        Assert.Equal((object)9f, u9.Value);
        Assert.True(u9.HasValue);
        Assert.Equal((9f).ToString(), u9.ToString());
        Assert.True(u9.TryGetValue(out float g9));
        Assert.Equal(9f, g9);

        var u10 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint>(10m);
        AssertOnlyFlag(10, u10.IsT1, u10.IsT2, u10.IsT3, u10.IsT4, u10.IsT5, u10.IsT6, u10.IsT7, u10.IsT8, u10.IsT9, u10.IsT10, u10.IsT11);
        Assert.Equal(10m, u10.AsT10());
        Assert.Equal((object)10m, u10.Value);
        Assert.True(u10.HasValue);
        Assert.Equal((10m).ToString(), u10.ToString());
        Assert.True(u10.TryGetValue(out decimal g10));
        Assert.Equal(10m, g10);

        var u11 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint>(11u);
        AssertOnlyFlag(11, u11.IsT1, u11.IsT2, u11.IsT3, u11.IsT4, u11.IsT5, u11.IsT6, u11.IsT7, u11.IsT8, u11.IsT9, u11.IsT10, u11.IsT11);
        Assert.Equal(11u, u11.AsT11());
        Assert.Equal((object)11u, u11.Value);
        Assert.True(u11.HasValue);
        Assert.Equal((11u).ToString(), u11.ToString());
        Assert.True(u11.TryGetValue(out uint g11));
        Assert.Equal(11u, g11);
    }

    [Fact]
    public void OneOfWith11CasesDefaultIsEmpty()
    {
        var u = default(OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint>);
        AssertOnlyFlag(0, u.IsT1, u.IsT2, u.IsT3, u.IsT4, u.IsT5, u.IsT6, u.IsT7, u.IsT8, u.IsT9, u.IsT10, u.IsT11);
        Assert.False(u.HasValue);
        Assert.Null(u.Value);
        Assert.Equal(LaquaiLibUnion.NoValue, u.ToString());
        Assert.Equal(0, u.GetHashCode());
        Assert.True(u.Equals(default(OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint>)));
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT1(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT2(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT3(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT4(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT5(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT6(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT7(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT8(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT9(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT10(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT11(); });
        Assert.False(u.TryGetValue(out int e1));
        Assert.False(u.TryGetValue(out string e2));
        Assert.False(u.TryGetValue(out double e3));
        Assert.False(u.TryGetValue(out char e4));
        Assert.False(u.TryGetValue(out bool e5));
        Assert.False(u.TryGetValue(out long e6));
        Assert.False(u.TryGetValue(out byte e7));
        Assert.False(u.TryGetValue(out short e8));
        Assert.False(u.TryGetValue(out float e9));
        Assert.False(u.TryGetValue(out decimal e10));
        Assert.False(u.TryGetValue(out uint e11));
    }

    [Fact]
    public void OneOfWith12CasesTracksEachCase()
    {
        var u1 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong>(1);
        AssertOnlyFlag(1, u1.IsT1, u1.IsT2, u1.IsT3, u1.IsT4, u1.IsT5, u1.IsT6, u1.IsT7, u1.IsT8, u1.IsT9, u1.IsT10, u1.IsT11, u1.IsT12);
        Assert.Equal(1, u1.AsT1());
        Assert.Equal((object)1, u1.Value);
        Assert.True(u1.HasValue);
        Assert.Equal((1).ToString(), u1.ToString());
        Assert.True(u1.TryGetValue(out int g1));
        Assert.Equal(1, g1);

        var u2 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong>("two");
        AssertOnlyFlag(2, u2.IsT1, u2.IsT2, u2.IsT3, u2.IsT4, u2.IsT5, u2.IsT6, u2.IsT7, u2.IsT8, u2.IsT9, u2.IsT10, u2.IsT11, u2.IsT12);
        Assert.Equal("two", u2.AsT2());
        Assert.Equal((object)"two", u2.Value);
        Assert.True(u2.HasValue);
        Assert.Equal(("two").ToString(), u2.ToString());
        Assert.True(u2.TryGetValue(out string g2));
        Assert.Equal("two", g2);

        var u3 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong>(3d);
        AssertOnlyFlag(3, u3.IsT1, u3.IsT2, u3.IsT3, u3.IsT4, u3.IsT5, u3.IsT6, u3.IsT7, u3.IsT8, u3.IsT9, u3.IsT10, u3.IsT11, u3.IsT12);
        Assert.Equal(3d, u3.AsT3());
        Assert.Equal((object)3d, u3.Value);
        Assert.True(u3.HasValue);
        Assert.Equal((3d).ToString(), u3.ToString());
        Assert.True(u3.TryGetValue(out double g3));
        Assert.Equal(3d, g3);

        var u4 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong>('4');
        AssertOnlyFlag(4, u4.IsT1, u4.IsT2, u4.IsT3, u4.IsT4, u4.IsT5, u4.IsT6, u4.IsT7, u4.IsT8, u4.IsT9, u4.IsT10, u4.IsT11, u4.IsT12);
        Assert.Equal('4', u4.AsT4());
        Assert.Equal((object)'4', u4.Value);
        Assert.True(u4.HasValue);
        Assert.Equal(('4').ToString(), u4.ToString());
        Assert.True(u4.TryGetValue(out char g4));
        Assert.Equal('4', g4);

        var u5 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong>(true);
        AssertOnlyFlag(5, u5.IsT1, u5.IsT2, u5.IsT3, u5.IsT4, u5.IsT5, u5.IsT6, u5.IsT7, u5.IsT8, u5.IsT9, u5.IsT10, u5.IsT11, u5.IsT12);
        Assert.True(u5.AsT5());
        Assert.Equal((object)true, u5.Value);
        Assert.True(u5.HasValue);
        Assert.Equal((true).ToString(), u5.ToString());
        Assert.True(u5.TryGetValue(out bool g5));
        Assert.True(g5);

        var u6 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong>(6L);
        AssertOnlyFlag(6, u6.IsT1, u6.IsT2, u6.IsT3, u6.IsT4, u6.IsT5, u6.IsT6, u6.IsT7, u6.IsT8, u6.IsT9, u6.IsT10, u6.IsT11, u6.IsT12);
        Assert.Equal(6L, u6.AsT6());
        Assert.Equal((object)6L, u6.Value);
        Assert.True(u6.HasValue);
        Assert.Equal((6L).ToString(), u6.ToString());
        Assert.True(u6.TryGetValue(out long g6));
        Assert.Equal(6L, g6);

        var u7 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong>((byte)7);
        AssertOnlyFlag(7, u7.IsT1, u7.IsT2, u7.IsT3, u7.IsT4, u7.IsT5, u7.IsT6, u7.IsT7, u7.IsT8, u7.IsT9, u7.IsT10, u7.IsT11, u7.IsT12);
        Assert.Equal((byte)7, u7.AsT7());
        Assert.Equal((object)(byte)7, u7.Value);
        Assert.True(u7.HasValue);
        Assert.Equal(((byte)7).ToString(), u7.ToString());
        Assert.True(u7.TryGetValue(out byte g7));
        Assert.Equal((byte)7, g7);

        var u8 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong>((short)8);
        AssertOnlyFlag(8, u8.IsT1, u8.IsT2, u8.IsT3, u8.IsT4, u8.IsT5, u8.IsT6, u8.IsT7, u8.IsT8, u8.IsT9, u8.IsT10, u8.IsT11, u8.IsT12);
        Assert.Equal((short)8, u8.AsT8());
        Assert.Equal((object)(short)8, u8.Value);
        Assert.True(u8.HasValue);
        Assert.Equal(((short)8).ToString(), u8.ToString());
        Assert.True(u8.TryGetValue(out short g8));
        Assert.Equal((short)8, g8);

        var u9 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong>(9f);
        AssertOnlyFlag(9, u9.IsT1, u9.IsT2, u9.IsT3, u9.IsT4, u9.IsT5, u9.IsT6, u9.IsT7, u9.IsT8, u9.IsT9, u9.IsT10, u9.IsT11, u9.IsT12);
        Assert.Equal(9f, u9.AsT9());
        Assert.Equal((object)9f, u9.Value);
        Assert.True(u9.HasValue);
        Assert.Equal((9f).ToString(), u9.ToString());
        Assert.True(u9.TryGetValue(out float g9));
        Assert.Equal(9f, g9);

        var u10 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong>(10m);
        AssertOnlyFlag(10, u10.IsT1, u10.IsT2, u10.IsT3, u10.IsT4, u10.IsT5, u10.IsT6, u10.IsT7, u10.IsT8, u10.IsT9, u10.IsT10, u10.IsT11, u10.IsT12);
        Assert.Equal(10m, u10.AsT10());
        Assert.Equal((object)10m, u10.Value);
        Assert.True(u10.HasValue);
        Assert.Equal((10m).ToString(), u10.ToString());
        Assert.True(u10.TryGetValue(out decimal g10));
        Assert.Equal(10m, g10);

        var u11 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong>(11u);
        AssertOnlyFlag(11, u11.IsT1, u11.IsT2, u11.IsT3, u11.IsT4, u11.IsT5, u11.IsT6, u11.IsT7, u11.IsT8, u11.IsT9, u11.IsT10, u11.IsT11, u11.IsT12);
        Assert.Equal(11u, u11.AsT11());
        Assert.Equal((object)11u, u11.Value);
        Assert.True(u11.HasValue);
        Assert.Equal((11u).ToString(), u11.ToString());
        Assert.True(u11.TryGetValue(out uint g11));
        Assert.Equal(11u, g11);

        var u12 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong>(12ul);
        AssertOnlyFlag(12, u12.IsT1, u12.IsT2, u12.IsT3, u12.IsT4, u12.IsT5, u12.IsT6, u12.IsT7, u12.IsT8, u12.IsT9, u12.IsT10, u12.IsT11, u12.IsT12);
        Assert.Equal(12ul, u12.AsT12());
        Assert.Equal((object)12ul, u12.Value);
        Assert.True(u12.HasValue);
        Assert.Equal((12ul).ToString(), u12.ToString());
        Assert.True(u12.TryGetValue(out ulong g12));
        Assert.Equal(12ul, g12);
    }

    [Fact]
    public void OneOfWith12CasesDefaultIsEmpty()
    {
        var u = default(OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong>);
        AssertOnlyFlag(0, u.IsT1, u.IsT2, u.IsT3, u.IsT4, u.IsT5, u.IsT6, u.IsT7, u.IsT8, u.IsT9, u.IsT10, u.IsT11, u.IsT12);
        Assert.False(u.HasValue);
        Assert.Null(u.Value);
        Assert.Equal(LaquaiLibUnion.NoValue, u.ToString());
        Assert.Equal(0, u.GetHashCode());
        Assert.True(u.Equals(default(OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong>)));
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT1(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT2(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT3(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT4(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT5(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT6(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT7(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT8(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT9(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT10(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT11(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT12(); });
        Assert.False(u.TryGetValue(out int e1));
        Assert.False(u.TryGetValue(out string e2));
        Assert.False(u.TryGetValue(out double e3));
        Assert.False(u.TryGetValue(out char e4));
        Assert.False(u.TryGetValue(out bool e5));
        Assert.False(u.TryGetValue(out long e6));
        Assert.False(u.TryGetValue(out byte e7));
        Assert.False(u.TryGetValue(out short e8));
        Assert.False(u.TryGetValue(out float e9));
        Assert.False(u.TryGetValue(out decimal e10));
        Assert.False(u.TryGetValue(out uint e11));
        Assert.False(u.TryGetValue(out ulong e12));
    }

    [Fact]
    public void OneOfWith13CasesTracksEachCase()
    {
        var u1 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte>(1);
        AssertOnlyFlag(1, u1.IsT1, u1.IsT2, u1.IsT3, u1.IsT4, u1.IsT5, u1.IsT6, u1.IsT7, u1.IsT8, u1.IsT9, u1.IsT10, u1.IsT11, u1.IsT12, u1.IsT13);
        Assert.Equal(1, u1.AsT1());
        Assert.Equal((object)1, u1.Value);
        Assert.True(u1.HasValue);
        Assert.Equal((1).ToString(), u1.ToString());
        Assert.True(u1.TryGetValue(out int g1));
        Assert.Equal(1, g1);

        var u2 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte>("two");
        AssertOnlyFlag(2, u2.IsT1, u2.IsT2, u2.IsT3, u2.IsT4, u2.IsT5, u2.IsT6, u2.IsT7, u2.IsT8, u2.IsT9, u2.IsT10, u2.IsT11, u2.IsT12, u2.IsT13);
        Assert.Equal("two", u2.AsT2());
        Assert.Equal((object)"two", u2.Value);
        Assert.True(u2.HasValue);
        Assert.Equal(("two").ToString(), u2.ToString());
        Assert.True(u2.TryGetValue(out string g2));
        Assert.Equal("two", g2);

        var u3 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte>(3d);
        AssertOnlyFlag(3, u3.IsT1, u3.IsT2, u3.IsT3, u3.IsT4, u3.IsT5, u3.IsT6, u3.IsT7, u3.IsT8, u3.IsT9, u3.IsT10, u3.IsT11, u3.IsT12, u3.IsT13);
        Assert.Equal(3d, u3.AsT3());
        Assert.Equal((object)3d, u3.Value);
        Assert.True(u3.HasValue);
        Assert.Equal((3d).ToString(), u3.ToString());
        Assert.True(u3.TryGetValue(out double g3));
        Assert.Equal(3d, g3);

        var u4 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte>('4');
        AssertOnlyFlag(4, u4.IsT1, u4.IsT2, u4.IsT3, u4.IsT4, u4.IsT5, u4.IsT6, u4.IsT7, u4.IsT8, u4.IsT9, u4.IsT10, u4.IsT11, u4.IsT12, u4.IsT13);
        Assert.Equal('4', u4.AsT4());
        Assert.Equal((object)'4', u4.Value);
        Assert.True(u4.HasValue);
        Assert.Equal(('4').ToString(), u4.ToString());
        Assert.True(u4.TryGetValue(out char g4));
        Assert.Equal('4', g4);

        var u5 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte>(true);
        AssertOnlyFlag(5, u5.IsT1, u5.IsT2, u5.IsT3, u5.IsT4, u5.IsT5, u5.IsT6, u5.IsT7, u5.IsT8, u5.IsT9, u5.IsT10, u5.IsT11, u5.IsT12, u5.IsT13);
        Assert.True(u5.AsT5());
        Assert.Equal((object)true, u5.Value);
        Assert.True(u5.HasValue);
        Assert.Equal((true).ToString(), u5.ToString());
        Assert.True(u5.TryGetValue(out bool g5));
        Assert.True(g5);

        var u6 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte>(6L);
        AssertOnlyFlag(6, u6.IsT1, u6.IsT2, u6.IsT3, u6.IsT4, u6.IsT5, u6.IsT6, u6.IsT7, u6.IsT8, u6.IsT9, u6.IsT10, u6.IsT11, u6.IsT12, u6.IsT13);
        Assert.Equal(6L, u6.AsT6());
        Assert.Equal((object)6L, u6.Value);
        Assert.True(u6.HasValue);
        Assert.Equal((6L).ToString(), u6.ToString());
        Assert.True(u6.TryGetValue(out long g6));
        Assert.Equal(6L, g6);

        var u7 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte>((byte)7);
        AssertOnlyFlag(7, u7.IsT1, u7.IsT2, u7.IsT3, u7.IsT4, u7.IsT5, u7.IsT6, u7.IsT7, u7.IsT8, u7.IsT9, u7.IsT10, u7.IsT11, u7.IsT12, u7.IsT13);
        Assert.Equal((byte)7, u7.AsT7());
        Assert.Equal((object)(byte)7, u7.Value);
        Assert.True(u7.HasValue);
        Assert.Equal(((byte)7).ToString(), u7.ToString());
        Assert.True(u7.TryGetValue(out byte g7));
        Assert.Equal((byte)7, g7);

        var u8 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte>((short)8);
        AssertOnlyFlag(8, u8.IsT1, u8.IsT2, u8.IsT3, u8.IsT4, u8.IsT5, u8.IsT6, u8.IsT7, u8.IsT8, u8.IsT9, u8.IsT10, u8.IsT11, u8.IsT12, u8.IsT13);
        Assert.Equal((short)8, u8.AsT8());
        Assert.Equal((object)(short)8, u8.Value);
        Assert.True(u8.HasValue);
        Assert.Equal(((short)8).ToString(), u8.ToString());
        Assert.True(u8.TryGetValue(out short g8));
        Assert.Equal((short)8, g8);

        var u9 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte>(9f);
        AssertOnlyFlag(9, u9.IsT1, u9.IsT2, u9.IsT3, u9.IsT4, u9.IsT5, u9.IsT6, u9.IsT7, u9.IsT8, u9.IsT9, u9.IsT10, u9.IsT11, u9.IsT12, u9.IsT13);
        Assert.Equal(9f, u9.AsT9());
        Assert.Equal((object)9f, u9.Value);
        Assert.True(u9.HasValue);
        Assert.Equal((9f).ToString(), u9.ToString());
        Assert.True(u9.TryGetValue(out float g9));
        Assert.Equal(9f, g9);

        var u10 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte>(10m);
        AssertOnlyFlag(10, u10.IsT1, u10.IsT2, u10.IsT3, u10.IsT4, u10.IsT5, u10.IsT6, u10.IsT7, u10.IsT8, u10.IsT9, u10.IsT10, u10.IsT11, u10.IsT12, u10.IsT13);
        Assert.Equal(10m, u10.AsT10());
        Assert.Equal((object)10m, u10.Value);
        Assert.True(u10.HasValue);
        Assert.Equal((10m).ToString(), u10.ToString());
        Assert.True(u10.TryGetValue(out decimal g10));
        Assert.Equal(10m, g10);

        var u11 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte>(11u);
        AssertOnlyFlag(11, u11.IsT1, u11.IsT2, u11.IsT3, u11.IsT4, u11.IsT5, u11.IsT6, u11.IsT7, u11.IsT8, u11.IsT9, u11.IsT10, u11.IsT11, u11.IsT12, u11.IsT13);
        Assert.Equal(11u, u11.AsT11());
        Assert.Equal((object)11u, u11.Value);
        Assert.True(u11.HasValue);
        Assert.Equal((11u).ToString(), u11.ToString());
        Assert.True(u11.TryGetValue(out uint g11));
        Assert.Equal(11u, g11);

        var u12 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte>(12ul);
        AssertOnlyFlag(12, u12.IsT1, u12.IsT2, u12.IsT3, u12.IsT4, u12.IsT5, u12.IsT6, u12.IsT7, u12.IsT8, u12.IsT9, u12.IsT10, u12.IsT11, u12.IsT12, u12.IsT13);
        Assert.Equal(12ul, u12.AsT12());
        Assert.Equal((object)12ul, u12.Value);
        Assert.True(u12.HasValue);
        Assert.Equal((12ul).ToString(), u12.ToString());
        Assert.True(u12.TryGetValue(out ulong g12));
        Assert.Equal(12ul, g12);

        var u13 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte>((sbyte)13);
        AssertOnlyFlag(13, u13.IsT1, u13.IsT2, u13.IsT3, u13.IsT4, u13.IsT5, u13.IsT6, u13.IsT7, u13.IsT8, u13.IsT9, u13.IsT10, u13.IsT11, u13.IsT12, u13.IsT13);
        Assert.Equal((sbyte)13, u13.AsT13());
        Assert.Equal((object)(sbyte)13, u13.Value);
        Assert.True(u13.HasValue);
        Assert.Equal(((sbyte)13).ToString(), u13.ToString());
        Assert.True(u13.TryGetValue(out sbyte g13));
        Assert.Equal((sbyte)13, g13);
    }

    [Fact]
    public void OneOfWith13CasesDefaultIsEmpty()
    {
        var u = default(OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte>);
        AssertOnlyFlag(0, u.IsT1, u.IsT2, u.IsT3, u.IsT4, u.IsT5, u.IsT6, u.IsT7, u.IsT8, u.IsT9, u.IsT10, u.IsT11, u.IsT12, u.IsT13);
        Assert.False(u.HasValue);
        Assert.Null(u.Value);
        Assert.Equal(LaquaiLibUnion.NoValue, u.ToString());
        Assert.Equal(0, u.GetHashCode());
        Assert.True(u.Equals(default(OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte>)));
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT1(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT2(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT3(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT4(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT5(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT6(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT7(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT8(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT9(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT10(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT11(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT12(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT13(); });
        Assert.False(u.TryGetValue(out int e1));
        Assert.False(u.TryGetValue(out string e2));
        Assert.False(u.TryGetValue(out double e3));
        Assert.False(u.TryGetValue(out char e4));
        Assert.False(u.TryGetValue(out bool e5));
        Assert.False(u.TryGetValue(out long e6));
        Assert.False(u.TryGetValue(out byte e7));
        Assert.False(u.TryGetValue(out short e8));
        Assert.False(u.TryGetValue(out float e9));
        Assert.False(u.TryGetValue(out decimal e10));
        Assert.False(u.TryGetValue(out uint e11));
        Assert.False(u.TryGetValue(out ulong e12));
        Assert.False(u.TryGetValue(out sbyte e13));
    }

    [Fact]
    public void OneOfWith14CasesTracksEachCase()
    {
        var u1 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort>(1);
        AssertOnlyFlag(1, u1.IsT1, u1.IsT2, u1.IsT3, u1.IsT4, u1.IsT5, u1.IsT6, u1.IsT7, u1.IsT8, u1.IsT9, u1.IsT10, u1.IsT11, u1.IsT12, u1.IsT13, u1.IsT14);
        Assert.Equal(1, u1.AsT1());
        Assert.Equal((object)1, u1.Value);
        Assert.True(u1.HasValue);
        Assert.Equal((1).ToString(), u1.ToString());
        Assert.True(u1.TryGetValue(out int g1));
        Assert.Equal(1, g1);

        var u2 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort>("two");
        AssertOnlyFlag(2, u2.IsT1, u2.IsT2, u2.IsT3, u2.IsT4, u2.IsT5, u2.IsT6, u2.IsT7, u2.IsT8, u2.IsT9, u2.IsT10, u2.IsT11, u2.IsT12, u2.IsT13, u2.IsT14);
        Assert.Equal("two", u2.AsT2());
        Assert.Equal((object)"two", u2.Value);
        Assert.True(u2.HasValue);
        Assert.Equal(("two").ToString(), u2.ToString());
        Assert.True(u2.TryGetValue(out string g2));
        Assert.Equal("two", g2);

        var u3 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort>(3d);
        AssertOnlyFlag(3, u3.IsT1, u3.IsT2, u3.IsT3, u3.IsT4, u3.IsT5, u3.IsT6, u3.IsT7, u3.IsT8, u3.IsT9, u3.IsT10, u3.IsT11, u3.IsT12, u3.IsT13, u3.IsT14);
        Assert.Equal(3d, u3.AsT3());
        Assert.Equal((object)3d, u3.Value);
        Assert.True(u3.HasValue);
        Assert.Equal((3d).ToString(), u3.ToString());
        Assert.True(u3.TryGetValue(out double g3));
        Assert.Equal(3d, g3);

        var u4 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort>('4');
        AssertOnlyFlag(4, u4.IsT1, u4.IsT2, u4.IsT3, u4.IsT4, u4.IsT5, u4.IsT6, u4.IsT7, u4.IsT8, u4.IsT9, u4.IsT10, u4.IsT11, u4.IsT12, u4.IsT13, u4.IsT14);
        Assert.Equal('4', u4.AsT4());
        Assert.Equal((object)'4', u4.Value);
        Assert.True(u4.HasValue);
        Assert.Equal(('4').ToString(), u4.ToString());
        Assert.True(u4.TryGetValue(out char g4));
        Assert.Equal('4', g4);

        var u5 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort>(true);
        AssertOnlyFlag(5, u5.IsT1, u5.IsT2, u5.IsT3, u5.IsT4, u5.IsT5, u5.IsT6, u5.IsT7, u5.IsT8, u5.IsT9, u5.IsT10, u5.IsT11, u5.IsT12, u5.IsT13, u5.IsT14);
        Assert.True(u5.AsT5());
        Assert.Equal((object)true, u5.Value);
        Assert.True(u5.HasValue);
        Assert.Equal((true).ToString(), u5.ToString());
        Assert.True(u5.TryGetValue(out bool g5));
        Assert.True(g5);

        var u6 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort>(6L);
        AssertOnlyFlag(6, u6.IsT1, u6.IsT2, u6.IsT3, u6.IsT4, u6.IsT5, u6.IsT6, u6.IsT7, u6.IsT8, u6.IsT9, u6.IsT10, u6.IsT11, u6.IsT12, u6.IsT13, u6.IsT14);
        Assert.Equal(6L, u6.AsT6());
        Assert.Equal((object)6L, u6.Value);
        Assert.True(u6.HasValue);
        Assert.Equal((6L).ToString(), u6.ToString());
        Assert.True(u6.TryGetValue(out long g6));
        Assert.Equal(6L, g6);

        var u7 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort>((byte)7);
        AssertOnlyFlag(7, u7.IsT1, u7.IsT2, u7.IsT3, u7.IsT4, u7.IsT5, u7.IsT6, u7.IsT7, u7.IsT8, u7.IsT9, u7.IsT10, u7.IsT11, u7.IsT12, u7.IsT13, u7.IsT14);
        Assert.Equal((byte)7, u7.AsT7());
        Assert.Equal((object)(byte)7, u7.Value);
        Assert.True(u7.HasValue);
        Assert.Equal(((byte)7).ToString(), u7.ToString());
        Assert.True(u7.TryGetValue(out byte g7));
        Assert.Equal((byte)7, g7);

        var u8 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort>((short)8);
        AssertOnlyFlag(8, u8.IsT1, u8.IsT2, u8.IsT3, u8.IsT4, u8.IsT5, u8.IsT6, u8.IsT7, u8.IsT8, u8.IsT9, u8.IsT10, u8.IsT11, u8.IsT12, u8.IsT13, u8.IsT14);
        Assert.Equal((short)8, u8.AsT8());
        Assert.Equal((object)(short)8, u8.Value);
        Assert.True(u8.HasValue);
        Assert.Equal(((short)8).ToString(), u8.ToString());
        Assert.True(u8.TryGetValue(out short g8));
        Assert.Equal((short)8, g8);

        var u9 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort>(9f);
        AssertOnlyFlag(9, u9.IsT1, u9.IsT2, u9.IsT3, u9.IsT4, u9.IsT5, u9.IsT6, u9.IsT7, u9.IsT8, u9.IsT9, u9.IsT10, u9.IsT11, u9.IsT12, u9.IsT13, u9.IsT14);
        Assert.Equal(9f, u9.AsT9());
        Assert.Equal((object)9f, u9.Value);
        Assert.True(u9.HasValue);
        Assert.Equal((9f).ToString(), u9.ToString());
        Assert.True(u9.TryGetValue(out float g9));
        Assert.Equal(9f, g9);

        var u10 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort>(10m);
        AssertOnlyFlag(10, u10.IsT1, u10.IsT2, u10.IsT3, u10.IsT4, u10.IsT5, u10.IsT6, u10.IsT7, u10.IsT8, u10.IsT9, u10.IsT10, u10.IsT11, u10.IsT12, u10.IsT13, u10.IsT14);
        Assert.Equal(10m, u10.AsT10());
        Assert.Equal((object)10m, u10.Value);
        Assert.True(u10.HasValue);
        Assert.Equal((10m).ToString(), u10.ToString());
        Assert.True(u10.TryGetValue(out decimal g10));
        Assert.Equal(10m, g10);

        var u11 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort>(11u);
        AssertOnlyFlag(11, u11.IsT1, u11.IsT2, u11.IsT3, u11.IsT4, u11.IsT5, u11.IsT6, u11.IsT7, u11.IsT8, u11.IsT9, u11.IsT10, u11.IsT11, u11.IsT12, u11.IsT13, u11.IsT14);
        Assert.Equal(11u, u11.AsT11());
        Assert.Equal((object)11u, u11.Value);
        Assert.True(u11.HasValue);
        Assert.Equal((11u).ToString(), u11.ToString());
        Assert.True(u11.TryGetValue(out uint g11));
        Assert.Equal(11u, g11);

        var u12 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort>(12ul);
        AssertOnlyFlag(12, u12.IsT1, u12.IsT2, u12.IsT3, u12.IsT4, u12.IsT5, u12.IsT6, u12.IsT7, u12.IsT8, u12.IsT9, u12.IsT10, u12.IsT11, u12.IsT12, u12.IsT13, u12.IsT14);
        Assert.Equal(12ul, u12.AsT12());
        Assert.Equal((object)12ul, u12.Value);
        Assert.True(u12.HasValue);
        Assert.Equal((12ul).ToString(), u12.ToString());
        Assert.True(u12.TryGetValue(out ulong g12));
        Assert.Equal(12ul, g12);

        var u13 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort>((sbyte)13);
        AssertOnlyFlag(13, u13.IsT1, u13.IsT2, u13.IsT3, u13.IsT4, u13.IsT5, u13.IsT6, u13.IsT7, u13.IsT8, u13.IsT9, u13.IsT10, u13.IsT11, u13.IsT12, u13.IsT13, u13.IsT14);
        Assert.Equal((sbyte)13, u13.AsT13());
        Assert.Equal((object)(sbyte)13, u13.Value);
        Assert.True(u13.HasValue);
        Assert.Equal(((sbyte)13).ToString(), u13.ToString());
        Assert.True(u13.TryGetValue(out sbyte g13));
        Assert.Equal((sbyte)13, g13);

        var u14 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort>((ushort)14);
        AssertOnlyFlag(14, u14.IsT1, u14.IsT2, u14.IsT3, u14.IsT4, u14.IsT5, u14.IsT6, u14.IsT7, u14.IsT8, u14.IsT9, u14.IsT10, u14.IsT11, u14.IsT12, u14.IsT13, u14.IsT14);
        Assert.Equal((ushort)14, u14.AsT14());
        Assert.Equal((object)(ushort)14, u14.Value);
        Assert.True(u14.HasValue);
        Assert.Equal(((ushort)14).ToString(), u14.ToString());
        Assert.True(u14.TryGetValue(out ushort g14));
        Assert.Equal((ushort)14, g14);
    }

    [Fact]
    public void OneOfWith14CasesDefaultIsEmpty()
    {
        var u = default(OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort>);
        AssertOnlyFlag(0, u.IsT1, u.IsT2, u.IsT3, u.IsT4, u.IsT5, u.IsT6, u.IsT7, u.IsT8, u.IsT9, u.IsT10, u.IsT11, u.IsT12, u.IsT13, u.IsT14);
        Assert.False(u.HasValue);
        Assert.Null(u.Value);
        Assert.Equal(LaquaiLibUnion.NoValue, u.ToString());
        Assert.Equal(0, u.GetHashCode());
        Assert.True(u.Equals(default(OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort>)));
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT1(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT2(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT3(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT4(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT5(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT6(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT7(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT8(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT9(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT10(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT11(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT12(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT13(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT14(); });
        Assert.False(u.TryGetValue(out int e1));
        Assert.False(u.TryGetValue(out string e2));
        Assert.False(u.TryGetValue(out double e3));
        Assert.False(u.TryGetValue(out char e4));
        Assert.False(u.TryGetValue(out bool e5));
        Assert.False(u.TryGetValue(out long e6));
        Assert.False(u.TryGetValue(out byte e7));
        Assert.False(u.TryGetValue(out short e8));
        Assert.False(u.TryGetValue(out float e9));
        Assert.False(u.TryGetValue(out decimal e10));
        Assert.False(u.TryGetValue(out uint e11));
        Assert.False(u.TryGetValue(out ulong e12));
        Assert.False(u.TryGetValue(out sbyte e13));
        Assert.False(u.TryGetValue(out ushort e14));
    }

    [Fact]
    public void OneOfWith15CasesTracksEachCase()
    {
        var u1 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid>(1);
        AssertOnlyFlag(1, u1.IsT1, u1.IsT2, u1.IsT3, u1.IsT4, u1.IsT5, u1.IsT6, u1.IsT7, u1.IsT8, u1.IsT9, u1.IsT10, u1.IsT11, u1.IsT12, u1.IsT13, u1.IsT14, u1.IsT15);
        Assert.Equal(1, u1.AsT1());
        Assert.Equal((object)1, u1.Value);
        Assert.True(u1.HasValue);
        Assert.Equal((1).ToString(), u1.ToString());
        Assert.True(u1.TryGetValue(out int g1));
        Assert.Equal(1, g1);

        var u2 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid>("two");
        AssertOnlyFlag(2, u2.IsT1, u2.IsT2, u2.IsT3, u2.IsT4, u2.IsT5, u2.IsT6, u2.IsT7, u2.IsT8, u2.IsT9, u2.IsT10, u2.IsT11, u2.IsT12, u2.IsT13, u2.IsT14, u2.IsT15);
        Assert.Equal("two", u2.AsT2());
        Assert.Equal((object)"two", u2.Value);
        Assert.True(u2.HasValue);
        Assert.Equal(("two").ToString(), u2.ToString());
        Assert.True(u2.TryGetValue(out string g2));
        Assert.Equal("two", g2);

        var u3 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid>(3d);
        AssertOnlyFlag(3, u3.IsT1, u3.IsT2, u3.IsT3, u3.IsT4, u3.IsT5, u3.IsT6, u3.IsT7, u3.IsT8, u3.IsT9, u3.IsT10, u3.IsT11, u3.IsT12, u3.IsT13, u3.IsT14, u3.IsT15);
        Assert.Equal(3d, u3.AsT3());
        Assert.Equal((object)3d, u3.Value);
        Assert.True(u3.HasValue);
        Assert.Equal((3d).ToString(), u3.ToString());
        Assert.True(u3.TryGetValue(out double g3));
        Assert.Equal(3d, g3);

        var u4 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid>('4');
        AssertOnlyFlag(4, u4.IsT1, u4.IsT2, u4.IsT3, u4.IsT4, u4.IsT5, u4.IsT6, u4.IsT7, u4.IsT8, u4.IsT9, u4.IsT10, u4.IsT11, u4.IsT12, u4.IsT13, u4.IsT14, u4.IsT15);
        Assert.Equal('4', u4.AsT4());
        Assert.Equal((object)'4', u4.Value);
        Assert.True(u4.HasValue);
        Assert.Equal(('4').ToString(), u4.ToString());
        Assert.True(u4.TryGetValue(out char g4));
        Assert.Equal('4', g4);

        var u5 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid>(true);
        AssertOnlyFlag(5, u5.IsT1, u5.IsT2, u5.IsT3, u5.IsT4, u5.IsT5, u5.IsT6, u5.IsT7, u5.IsT8, u5.IsT9, u5.IsT10, u5.IsT11, u5.IsT12, u5.IsT13, u5.IsT14, u5.IsT15);
        Assert.True(u5.AsT5());
        Assert.Equal((object)true, u5.Value);
        Assert.True(u5.HasValue);
        Assert.Equal((true).ToString(), u5.ToString());
        Assert.True(u5.TryGetValue(out bool g5));
        Assert.True(g5);

        var u6 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid>(6L);
        AssertOnlyFlag(6, u6.IsT1, u6.IsT2, u6.IsT3, u6.IsT4, u6.IsT5, u6.IsT6, u6.IsT7, u6.IsT8, u6.IsT9, u6.IsT10, u6.IsT11, u6.IsT12, u6.IsT13, u6.IsT14, u6.IsT15);
        Assert.Equal(6L, u6.AsT6());
        Assert.Equal((object)6L, u6.Value);
        Assert.True(u6.HasValue);
        Assert.Equal((6L).ToString(), u6.ToString());
        Assert.True(u6.TryGetValue(out long g6));
        Assert.Equal(6L, g6);

        var u7 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid>((byte)7);
        AssertOnlyFlag(7, u7.IsT1, u7.IsT2, u7.IsT3, u7.IsT4, u7.IsT5, u7.IsT6, u7.IsT7, u7.IsT8, u7.IsT9, u7.IsT10, u7.IsT11, u7.IsT12, u7.IsT13, u7.IsT14, u7.IsT15);
        Assert.Equal((byte)7, u7.AsT7());
        Assert.Equal((object)(byte)7, u7.Value);
        Assert.True(u7.HasValue);
        Assert.Equal(((byte)7).ToString(), u7.ToString());
        Assert.True(u7.TryGetValue(out byte g7));
        Assert.Equal((byte)7, g7);

        var u8 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid>((short)8);
        AssertOnlyFlag(8, u8.IsT1, u8.IsT2, u8.IsT3, u8.IsT4, u8.IsT5, u8.IsT6, u8.IsT7, u8.IsT8, u8.IsT9, u8.IsT10, u8.IsT11, u8.IsT12, u8.IsT13, u8.IsT14, u8.IsT15);
        Assert.Equal((short)8, u8.AsT8());
        Assert.Equal((object)(short)8, u8.Value);
        Assert.True(u8.HasValue);
        Assert.Equal(((short)8).ToString(), u8.ToString());
        Assert.True(u8.TryGetValue(out short g8));
        Assert.Equal((short)8, g8);

        var u9 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid>(9f);
        AssertOnlyFlag(9, u9.IsT1, u9.IsT2, u9.IsT3, u9.IsT4, u9.IsT5, u9.IsT6, u9.IsT7, u9.IsT8, u9.IsT9, u9.IsT10, u9.IsT11, u9.IsT12, u9.IsT13, u9.IsT14, u9.IsT15);
        Assert.Equal(9f, u9.AsT9());
        Assert.Equal((object)9f, u9.Value);
        Assert.True(u9.HasValue);
        Assert.Equal((9f).ToString(), u9.ToString());
        Assert.True(u9.TryGetValue(out float g9));
        Assert.Equal(9f, g9);

        var u10 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid>(10m);
        AssertOnlyFlag(10, u10.IsT1, u10.IsT2, u10.IsT3, u10.IsT4, u10.IsT5, u10.IsT6, u10.IsT7, u10.IsT8, u10.IsT9, u10.IsT10, u10.IsT11, u10.IsT12, u10.IsT13, u10.IsT14, u10.IsT15);
        Assert.Equal(10m, u10.AsT10());
        Assert.Equal((object)10m, u10.Value);
        Assert.True(u10.HasValue);
        Assert.Equal((10m).ToString(), u10.ToString());
        Assert.True(u10.TryGetValue(out decimal g10));
        Assert.Equal(10m, g10);

        var u11 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid>(11u);
        AssertOnlyFlag(11, u11.IsT1, u11.IsT2, u11.IsT3, u11.IsT4, u11.IsT5, u11.IsT6, u11.IsT7, u11.IsT8, u11.IsT9, u11.IsT10, u11.IsT11, u11.IsT12, u11.IsT13, u11.IsT14, u11.IsT15);
        Assert.Equal(11u, u11.AsT11());
        Assert.Equal((object)11u, u11.Value);
        Assert.True(u11.HasValue);
        Assert.Equal((11u).ToString(), u11.ToString());
        Assert.True(u11.TryGetValue(out uint g11));
        Assert.Equal(11u, g11);

        var u12 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid>(12ul);
        AssertOnlyFlag(12, u12.IsT1, u12.IsT2, u12.IsT3, u12.IsT4, u12.IsT5, u12.IsT6, u12.IsT7, u12.IsT8, u12.IsT9, u12.IsT10, u12.IsT11, u12.IsT12, u12.IsT13, u12.IsT14, u12.IsT15);
        Assert.Equal(12ul, u12.AsT12());
        Assert.Equal((object)12ul, u12.Value);
        Assert.True(u12.HasValue);
        Assert.Equal((12ul).ToString(), u12.ToString());
        Assert.True(u12.TryGetValue(out ulong g12));
        Assert.Equal(12ul, g12);

        var u13 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid>((sbyte)13);
        AssertOnlyFlag(13, u13.IsT1, u13.IsT2, u13.IsT3, u13.IsT4, u13.IsT5, u13.IsT6, u13.IsT7, u13.IsT8, u13.IsT9, u13.IsT10, u13.IsT11, u13.IsT12, u13.IsT13, u13.IsT14, u13.IsT15);
        Assert.Equal((sbyte)13, u13.AsT13());
        Assert.Equal((object)(sbyte)13, u13.Value);
        Assert.True(u13.HasValue);
        Assert.Equal(((sbyte)13).ToString(), u13.ToString());
        Assert.True(u13.TryGetValue(out sbyte g13));
        Assert.Equal((sbyte)13, g13);

        var u14 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid>((ushort)14);
        AssertOnlyFlag(14, u14.IsT1, u14.IsT2, u14.IsT3, u14.IsT4, u14.IsT5, u14.IsT6, u14.IsT7, u14.IsT8, u14.IsT9, u14.IsT10, u14.IsT11, u14.IsT12, u14.IsT13, u14.IsT14, u14.IsT15);
        Assert.Equal((ushort)14, u14.AsT14());
        Assert.Equal((object)(ushort)14, u14.Value);
        Assert.True(u14.HasValue);
        Assert.Equal(((ushort)14).ToString(), u14.ToString());
        Assert.True(u14.TryGetValue(out ushort g14));
        Assert.Equal((ushort)14, g14);

        var u15 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid>(GuidValue);
        AssertOnlyFlag(15, u15.IsT1, u15.IsT2, u15.IsT3, u15.IsT4, u15.IsT5, u15.IsT6, u15.IsT7, u15.IsT8, u15.IsT9, u15.IsT10, u15.IsT11, u15.IsT12, u15.IsT13, u15.IsT14, u15.IsT15);
        Assert.Equal(GuidValue, u15.AsT15());
        Assert.Equal((object)GuidValue, u15.Value);
        Assert.True(u15.HasValue);
        Assert.Equal(GuidValue.ToString(), u15.ToString());
        Assert.True(u15.TryGetValue(out Guid g15));
        Assert.Equal(GuidValue, g15);
    }

    [Fact]
    public void OneOfWith15CasesDefaultIsEmpty()
    {
        var u = default(OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid>);
        AssertOnlyFlag(0, u.IsT1, u.IsT2, u.IsT3, u.IsT4, u.IsT5, u.IsT6, u.IsT7, u.IsT8, u.IsT9, u.IsT10, u.IsT11, u.IsT12, u.IsT13, u.IsT14, u.IsT15);
        Assert.False(u.HasValue);
        Assert.Null(u.Value);
        Assert.Equal(LaquaiLibUnion.NoValue, u.ToString());
        Assert.Equal(0, u.GetHashCode());
        Assert.True(u.Equals(default(OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid>)));
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT1(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT2(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT3(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT4(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT5(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT6(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT7(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT8(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT9(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT10(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT11(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT12(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT13(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT14(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT15(); });
        Assert.False(u.TryGetValue(out int e1));
        Assert.False(u.TryGetValue(out string e2));
        Assert.False(u.TryGetValue(out double e3));
        Assert.False(u.TryGetValue(out char e4));
        Assert.False(u.TryGetValue(out bool e5));
        Assert.False(u.TryGetValue(out long e6));
        Assert.False(u.TryGetValue(out byte e7));
        Assert.False(u.TryGetValue(out short e8));
        Assert.False(u.TryGetValue(out float e9));
        Assert.False(u.TryGetValue(out decimal e10));
        Assert.False(u.TryGetValue(out uint e11));
        Assert.False(u.TryGetValue(out ulong e12));
        Assert.False(u.TryGetValue(out sbyte e13));
        Assert.False(u.TryGetValue(out ushort e14));
        Assert.False(u.TryGetValue(out Guid e15));
    }

    [Fact]
    public void OneOfWith16CasesTracksEachCase()
    {
        var u1 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid, DateTime>(1);
        AssertOnlyFlag(1, u1.IsT1, u1.IsT2, u1.IsT3, u1.IsT4, u1.IsT5, u1.IsT6, u1.IsT7, u1.IsT8, u1.IsT9, u1.IsT10, u1.IsT11, u1.IsT12, u1.IsT13, u1.IsT14, u1.IsT15, u1.IsT16);
        Assert.Equal(1, u1.AsT1());
        Assert.Equal((object)1, u1.Value);
        Assert.True(u1.HasValue);
        Assert.Equal((1).ToString(), u1.ToString());
        Assert.True(u1.TryGetValue(out int g1));
        Assert.Equal(1, g1);

        var u2 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid, DateTime>("two");
        AssertOnlyFlag(2, u2.IsT1, u2.IsT2, u2.IsT3, u2.IsT4, u2.IsT5, u2.IsT6, u2.IsT7, u2.IsT8, u2.IsT9, u2.IsT10, u2.IsT11, u2.IsT12, u2.IsT13, u2.IsT14, u2.IsT15, u2.IsT16);
        Assert.Equal("two", u2.AsT2());
        Assert.Equal((object)"two", u2.Value);
        Assert.True(u2.HasValue);
        Assert.Equal(("two").ToString(), u2.ToString());
        Assert.True(u2.TryGetValue(out string g2));
        Assert.Equal("two", g2);

        var u3 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid, DateTime>(3d);
        AssertOnlyFlag(3, u3.IsT1, u3.IsT2, u3.IsT3, u3.IsT4, u3.IsT5, u3.IsT6, u3.IsT7, u3.IsT8, u3.IsT9, u3.IsT10, u3.IsT11, u3.IsT12, u3.IsT13, u3.IsT14, u3.IsT15, u3.IsT16);
        Assert.Equal(3d, u3.AsT3());
        Assert.Equal((object)3d, u3.Value);
        Assert.True(u3.HasValue);
        Assert.Equal((3d).ToString(), u3.ToString());
        Assert.True(u3.TryGetValue(out double g3));
        Assert.Equal(3d, g3);

        var u4 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid, DateTime>('4');
        AssertOnlyFlag(4, u4.IsT1, u4.IsT2, u4.IsT3, u4.IsT4, u4.IsT5, u4.IsT6, u4.IsT7, u4.IsT8, u4.IsT9, u4.IsT10, u4.IsT11, u4.IsT12, u4.IsT13, u4.IsT14, u4.IsT15, u4.IsT16);
        Assert.Equal('4', u4.AsT4());
        Assert.Equal((object)'4', u4.Value);
        Assert.True(u4.HasValue);
        Assert.Equal(('4').ToString(), u4.ToString());
        Assert.True(u4.TryGetValue(out char g4));
        Assert.Equal('4', g4);

        var u5 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid, DateTime>(true);
        AssertOnlyFlag(5, u5.IsT1, u5.IsT2, u5.IsT3, u5.IsT4, u5.IsT5, u5.IsT6, u5.IsT7, u5.IsT8, u5.IsT9, u5.IsT10, u5.IsT11, u5.IsT12, u5.IsT13, u5.IsT14, u5.IsT15, u5.IsT16);
        Assert.True(u5.AsT5());
        Assert.Equal((object)true, u5.Value);
        Assert.True(u5.HasValue);
        Assert.Equal((true).ToString(), u5.ToString());
        Assert.True(u5.TryGetValue(out bool g5));
        Assert.True(g5);

        var u6 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid, DateTime>(6L);
        AssertOnlyFlag(6, u6.IsT1, u6.IsT2, u6.IsT3, u6.IsT4, u6.IsT5, u6.IsT6, u6.IsT7, u6.IsT8, u6.IsT9, u6.IsT10, u6.IsT11, u6.IsT12, u6.IsT13, u6.IsT14, u6.IsT15, u6.IsT16);
        Assert.Equal(6L, u6.AsT6());
        Assert.Equal((object)6L, u6.Value);
        Assert.True(u6.HasValue);
        Assert.Equal((6L).ToString(), u6.ToString());
        Assert.True(u6.TryGetValue(out long g6));
        Assert.Equal(6L, g6);

        var u7 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid, DateTime>((byte)7);
        AssertOnlyFlag(7, u7.IsT1, u7.IsT2, u7.IsT3, u7.IsT4, u7.IsT5, u7.IsT6, u7.IsT7, u7.IsT8, u7.IsT9, u7.IsT10, u7.IsT11, u7.IsT12, u7.IsT13, u7.IsT14, u7.IsT15, u7.IsT16);
        Assert.Equal((byte)7, u7.AsT7());
        Assert.Equal((object)(byte)7, u7.Value);
        Assert.True(u7.HasValue);
        Assert.Equal(((byte)7).ToString(), u7.ToString());
        Assert.True(u7.TryGetValue(out byte g7));
        Assert.Equal((byte)7, g7);

        var u8 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid, DateTime>((short)8);
        AssertOnlyFlag(8, u8.IsT1, u8.IsT2, u8.IsT3, u8.IsT4, u8.IsT5, u8.IsT6, u8.IsT7, u8.IsT8, u8.IsT9, u8.IsT10, u8.IsT11, u8.IsT12, u8.IsT13, u8.IsT14, u8.IsT15, u8.IsT16);
        Assert.Equal((short)8, u8.AsT8());
        Assert.Equal((object)(short)8, u8.Value);
        Assert.True(u8.HasValue);
        Assert.Equal(((short)8).ToString(), u8.ToString());
        Assert.True(u8.TryGetValue(out short g8));
        Assert.Equal((short)8, g8);

        var u9 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid, DateTime>(9f);
        AssertOnlyFlag(9, u9.IsT1, u9.IsT2, u9.IsT3, u9.IsT4, u9.IsT5, u9.IsT6, u9.IsT7, u9.IsT8, u9.IsT9, u9.IsT10, u9.IsT11, u9.IsT12, u9.IsT13, u9.IsT14, u9.IsT15, u9.IsT16);
        Assert.Equal(9f, u9.AsT9());
        Assert.Equal((object)9f, u9.Value);
        Assert.True(u9.HasValue);
        Assert.Equal((9f).ToString(), u9.ToString());
        Assert.True(u9.TryGetValue(out float g9));
        Assert.Equal(9f, g9);

        var u10 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid, DateTime>(10m);
        AssertOnlyFlag(10, u10.IsT1, u10.IsT2, u10.IsT3, u10.IsT4, u10.IsT5, u10.IsT6, u10.IsT7, u10.IsT8, u10.IsT9, u10.IsT10, u10.IsT11, u10.IsT12, u10.IsT13, u10.IsT14, u10.IsT15, u10.IsT16);
        Assert.Equal(10m, u10.AsT10());
        Assert.Equal((object)10m, u10.Value);
        Assert.True(u10.HasValue);
        Assert.Equal((10m).ToString(), u10.ToString());
        Assert.True(u10.TryGetValue(out decimal g10));
        Assert.Equal(10m, g10);

        var u11 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid, DateTime>(11u);
        AssertOnlyFlag(11, u11.IsT1, u11.IsT2, u11.IsT3, u11.IsT4, u11.IsT5, u11.IsT6, u11.IsT7, u11.IsT8, u11.IsT9, u11.IsT10, u11.IsT11, u11.IsT12, u11.IsT13, u11.IsT14, u11.IsT15, u11.IsT16);
        Assert.Equal(11u, u11.AsT11());
        Assert.Equal((object)11u, u11.Value);
        Assert.True(u11.HasValue);
        Assert.Equal((11u).ToString(), u11.ToString());
        Assert.True(u11.TryGetValue(out uint g11));
        Assert.Equal(11u, g11);

        var u12 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid, DateTime>(12ul);
        AssertOnlyFlag(12, u12.IsT1, u12.IsT2, u12.IsT3, u12.IsT4, u12.IsT5, u12.IsT6, u12.IsT7, u12.IsT8, u12.IsT9, u12.IsT10, u12.IsT11, u12.IsT12, u12.IsT13, u12.IsT14, u12.IsT15, u12.IsT16);
        Assert.Equal(12ul, u12.AsT12());
        Assert.Equal((object)12ul, u12.Value);
        Assert.True(u12.HasValue);
        Assert.Equal((12ul).ToString(), u12.ToString());
        Assert.True(u12.TryGetValue(out ulong g12));
        Assert.Equal(12ul, g12);

        var u13 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid, DateTime>((sbyte)13);
        AssertOnlyFlag(13, u13.IsT1, u13.IsT2, u13.IsT3, u13.IsT4, u13.IsT5, u13.IsT6, u13.IsT7, u13.IsT8, u13.IsT9, u13.IsT10, u13.IsT11, u13.IsT12, u13.IsT13, u13.IsT14, u13.IsT15, u13.IsT16);
        Assert.Equal((sbyte)13, u13.AsT13());
        Assert.Equal((object)(sbyte)13, u13.Value);
        Assert.True(u13.HasValue);
        Assert.Equal(((sbyte)13).ToString(), u13.ToString());
        Assert.True(u13.TryGetValue(out sbyte g13));
        Assert.Equal((sbyte)13, g13);

        var u14 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid, DateTime>((ushort)14);
        AssertOnlyFlag(14, u14.IsT1, u14.IsT2, u14.IsT3, u14.IsT4, u14.IsT5, u14.IsT6, u14.IsT7, u14.IsT8, u14.IsT9, u14.IsT10, u14.IsT11, u14.IsT12, u14.IsT13, u14.IsT14, u14.IsT15, u14.IsT16);
        Assert.Equal((ushort)14, u14.AsT14());
        Assert.Equal((object)(ushort)14, u14.Value);
        Assert.True(u14.HasValue);
        Assert.Equal(((ushort)14).ToString(), u14.ToString());
        Assert.True(u14.TryGetValue(out ushort g14));
        Assert.Equal((ushort)14, g14);

        var u15 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid, DateTime>(GuidValue);
        AssertOnlyFlag(15, u15.IsT1, u15.IsT2, u15.IsT3, u15.IsT4, u15.IsT5, u15.IsT6, u15.IsT7, u15.IsT8, u15.IsT9, u15.IsT10, u15.IsT11, u15.IsT12, u15.IsT13, u15.IsT14, u15.IsT15, u15.IsT16);
        Assert.Equal(GuidValue, u15.AsT15());
        Assert.Equal((object)GuidValue, u15.Value);
        Assert.True(u15.HasValue);
        Assert.Equal(GuidValue.ToString(), u15.ToString());
        Assert.True(u15.TryGetValue(out Guid g15));
        Assert.Equal(GuidValue, g15);

        var u16 = new OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid, DateTime>(DateTimeValue);
        AssertOnlyFlag(16, u16.IsT1, u16.IsT2, u16.IsT3, u16.IsT4, u16.IsT5, u16.IsT6, u16.IsT7, u16.IsT8, u16.IsT9, u16.IsT10, u16.IsT11, u16.IsT12, u16.IsT13, u16.IsT14, u16.IsT15, u16.IsT16);
        Assert.Equal(DateTimeValue, u16.AsT16());
        Assert.Equal((object)DateTimeValue, u16.Value);
        Assert.True(u16.HasValue);
        Assert.Equal(DateTimeValue.ToString(), u16.ToString());
        Assert.True(u16.TryGetValue(out DateTime g16));
        Assert.Equal(DateTimeValue, g16);
    }

    [Fact]
    public void OneOfWith16CasesDefaultIsEmpty()
    {
        var u = default(OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid, DateTime>);
        AssertOnlyFlag(0, u.IsT1, u.IsT2, u.IsT3, u.IsT4, u.IsT5, u.IsT6, u.IsT7, u.IsT8, u.IsT9, u.IsT10, u.IsT11, u.IsT12, u.IsT13, u.IsT14, u.IsT15, u.IsT16);
        Assert.False(u.HasValue);
        Assert.Null(u.Value);
        Assert.Equal(LaquaiLibUnion.NoValue, u.ToString());
        Assert.Equal(0, u.GetHashCode());
        Assert.True(u.Equals(default(OneOf<int, string, double, char, bool, long, byte, short, float, decimal, uint, ulong, sbyte, ushort, Guid, DateTime>)));
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT1(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT2(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT3(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT4(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT5(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT6(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT7(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT8(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT9(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT10(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT11(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT12(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT13(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT14(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT15(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT16(); });
        Assert.False(u.TryGetValue(out int e1));
        Assert.False(u.TryGetValue(out string e2));
        Assert.False(u.TryGetValue(out double e3));
        Assert.False(u.TryGetValue(out char e4));
        Assert.False(u.TryGetValue(out bool e5));
        Assert.False(u.TryGetValue(out long e6));
        Assert.False(u.TryGetValue(out byte e7));
        Assert.False(u.TryGetValue(out short e8));
        Assert.False(u.TryGetValue(out float e9));
        Assert.False(u.TryGetValue(out decimal e10));
        Assert.False(u.TryGetValue(out uint e11));
        Assert.False(u.TryGetValue(out ulong e12));
        Assert.False(u.TryGetValue(out sbyte e13));
        Assert.False(u.TryGetValue(out ushort e14));
        Assert.False(u.TryGetValue(out Guid e15));
        Assert.False(u.TryGetValue(out DateTime e16));
    }
}
