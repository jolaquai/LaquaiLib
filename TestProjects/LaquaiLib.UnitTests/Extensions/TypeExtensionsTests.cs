using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Extensions;

public static class TypeExtensionsTests
{
    public sealed class SealedType { }

    public abstract class Base { }
    public class DerivedPublic : Base { public DerivedPublic() { } }
    public abstract class DerivedAbstract : Base { }
    public class DerivedPrivateCtor : Base { private DerivedPrivateCtor() { } }

    public class ReflectionTarget
    {
        public int X;
        public string Y { get; set; } = "y";
        public static int SX = 7;
        public static string SY { get; set; } = "s";
        public int GetX() => X;
        public void VoidMethod() { }
        public static string StaticNoParam() => "ok";
        public static int StaticWithParam(int a) => a;
        public string InstanceWithParam(int a) => a.ToString();
    }

    public struct MyStruct { public int A; public int B; }

    public class Construction
    {
        private sealed class CtorTargets
        {
            public CtorTargets() { }
            public CtorTargets(int a, string b) { A = a; B = b; }
            public int A { get; }
            public string B { get; }
        }

        [Fact]
        public void NewCreatesInstanceWithDefaultCtor()
        {
            var obj = typeof(CtorTargets).New();
            Assert.NotNull(obj);
            Assert.IsType<CtorTargets>(obj);
        }

        [Fact]
        public void NewCreatesInstanceWithMatchingCtor()
        {
            var obj = (CtorTargets)typeof(CtorTargets).New(5, "x");
            Assert.Equal(5, obj.A);
            Assert.Equal("x", obj.B);
        }

        [Fact]
        public void NewReturnsNullWhenNoMatchingCtor()
        {
            var obj = typeof(CtorTargets).New(5);
            Assert.Null(obj);
        }

        [Fact]
        public void NewReturnsNullWhenNullArgumentPreventsMatch()
        {
            var obj = typeof(CtorTargets).New(5, null);
            Assert.Null(obj);
        }
    }

    public class Defaults
    {
        [Fact]
        public void GetDefaultForValueType()
        {
            var d = typeof(int).GetDefault();
            Assert.Equal(0, d);
        }

        [Fact]
        public void GetDefaultForStringReturnsEmpty()
        {
            var d = typeof(string).GetDefault();
            Assert.Equal(string.Empty, d);
        }

        [Fact]
        public void GetDefaultForReferenceTypeReturnsNull()
        {
            var d = typeof(object).GetDefault();
            Assert.Null(d);
        }
    }

    public class InstanceAndStaticValues
    {
        [Fact]
        public void GetInstanceValuesCollectsFieldPropertyAndMethodsAsSignatures()
        {
            var t = typeof(ReflectionTarget);
            var obj = new ReflectionTarget { X = 42, Y = "yy" };
            var dict = t.GetInstanceValues(obj, callMethods: false);
            Assert.Equal(42, dict["X"]);
            Assert.Equal("yy", dict["Y"]);
            Assert.Contains(dict.Keys, k => k.StartsWith("GetX("));
            Assert.Contains(dict.Keys, k => k.StartsWith("InstanceWithParam("));
        }

        [Fact]
        public void GetInstanceValuesCallMethodsThrowsOnMethodsWithParameters()
        {
            var t = typeof(ReflectionTarget);
            var obj = new ReflectionTarget();
            Assert.ThrowsAny<Exception>(() => t.GetInstanceValues(obj, callMethods: true));
        }

        [Fact]
        public void GetStaticValuesCollectsStatics()
        {
            var t = typeof(ReflectionTarget);
            var dict = t.GetStaticValues(callMethods: false);
            Assert.Equal(7, dict["SX"]);
            Assert.Equal("s", dict["SY"]);
            Assert.Contains(dict.Keys, k => k.StartsWith("StaticNoParam("));
        }

        [Fact]
        public void GetStaticValuesCallMethodsThrowsOnMethodsWithParameters()
        {
            var t = typeof(ReflectionTarget);
            Assert.ThrowsAny<Exception>(() => t.GetStaticValues(callMethods: true));
        }
    }

    public class NumericConversions
    {
        [Fact]
        public void HasNarrowingConversionByteToSByte()
        {
            Assert.True(typeof(byte).HasNarrowingConversion(typeof(sbyte)));
        }

        [Fact]
        public void HasConsistentWideningConversionByteToInt32()
        {
            Assert.True(typeof(byte).HasConsistentWideningConversion(typeof(int)));
        }

        [Fact]
        public void HasLossyWideningConversionInt64ToSingle()
        {
            Assert.True(typeof(long).HasLossyWideningConversion(typeof(float)));
        }

        [Fact]
        public void HasWideningConversionByteToInt32()
        {
            Assert.True(typeof(byte).HasWideningConversion(typeof(int)));
        }

        [Fact]
        public void NumericConversionsReturnFalseForSameType()
        {
            Assert.False(typeof(int).HasNarrowingConversion(typeof(int)));
            Assert.False(typeof(int).HasConsistentWideningConversion(typeof(int)));
            Assert.False(typeof(int).HasLossyWideningConversion(typeof(int)));
            Assert.False(typeof(int).HasWideningConversion(typeof(int)));
        }

        [Fact]
        public void NumericConversionsThrowOnNonNumericTypes()
        {
            Assert.Throws<ArgumentException>(() => typeof(string).HasNarrowingConversion(typeof(string)));
            Assert.Throws<ArgumentException>(() => typeof(string).HasConsistentWideningConversion(typeof(int)));
            Assert.Throws<ArgumentException>(() => typeof(int).HasLossyWideningConversion(typeof(string)));
        }
    }

    public class ReflectionGeneration
    {
        [Fact]
        public void ReflectThrowsWhenInheritingFromSealedType()
        {
            var options = new ReflectionOptions { Inherit = ReflectionOptions.InheritanceBehavior.Inherit };
            Assert.Throws<TypeAccessException>(() => typeof(SealedType).Reflect(options));
        }

        [Fact]
        public void ReflectEmitsClassDeclaration()
        {
            var code = typeof(ReflectionTarget).Reflect();
            Assert.Contains("public class ReflectionTarget", code);
            Assert.Contains("}", code);
        }
    }

    public class FriendlyNamesAndKeywords
    {
        [Fact]
        public void GetFriendlyNameForArray()
        {
            Assert.Equal("int[]", typeof(int[]).GetFriendlyName());
        }

        [Fact]
        public void GetFriendlyNameForGenericType()
        {
            Assert.Equal("System.Collections.Generic.Dictionary<string, int>", typeof(Dictionary<string, int>).GetFriendlyName());
        }

#pragma warning disable CA1515 // Consider making public types internal
#pragma warning disable CA1852 // Consider making public types internal
        private class Outer { public class Inner { } }
#pragma warning restore CA1515 // Consider making public types internal
#pragma warning restore CA1852

        [Fact]
        public void GetFriendlyNameForNestedType()
        {
            Assert.Equal("LaquaiLib.UnitTests.Extensions.TypeExtensionsTests+FriendlyNamesAndKeywords+Outer+Inner", typeof(Outer.Inner).GetFriendlyName());
            Assert.Equal("TypeExtensionsTests+FriendlyNamesAndKeywords+Outer+Inner", typeof(Outer.Inner).GetFriendlyName(includeNamespace: false));
        }

        private unsafe struct PtrHolder { }

        [Fact]
        public unsafe void GetFriendlyNameForPointerAndRef()
        {
            Assert.Equal("int*", typeof(int*).GetFriendlyName());
            Assert.Equal("ref int", typeof(int).MakeByRefType().GetFriendlyName());
        }

        [Theory]
        [InlineData("System.Int32", "int")]
        [InlineData("System.String", "string")]
        [InlineData("Foo.Bar", "Foo.Bar")]
        public void AsKeywordStaticMaps(string input, string expected)
        {
            Assert.Equal(expected, TypeExtensions.AsKeyword(input));
        }

        [Fact]
        public void AsKeywordInstanceMapsSystemInt32ToInt()
        {
            Assert.Equal("int", typeof(int).AsKeyword());
        }
    }

    public class DelegateDetection
    {
        [Fact]
        public void IsFuncDetectsReturnType()
        {
            Assert.True(typeof(Func<int>).IsFunc(out var r1));
            Assert.Equal(typeof(int), r1);
            Assert.True(typeof(Func<int, string>).IsFunc(out var r2));
            Assert.Equal(typeof(string), r2);
        }

        [Fact]
        public void IsFuncFalseForAction()
        {
            Assert.False(typeof(Action).IsFunc(out _));
        }

        [Fact]
        public void IsActionDetectsNonGenericAndGeneric()
        {
            Assert.True(typeof(Action).IsAction(out var p0) && p0 == false);
            Assert.True(typeof(Action<int, string>).IsAction(out var p2) && p2 == true);
        }
    }

    public class SubtypeDiscovery
    {
        [Fact]
        public void FindConstructibleSubtypesFindsExpected()
        {
            var types = typeof(Base).FindConstructibleSubtypes();
            Assert.Contains(typeof(DerivedPublic), types);
            Assert.DoesNotContain(typeof(DerivedAbstract), types);
            Assert.DoesNotContain(typeof(DerivedPrivateCtor), types);
            Assert.DoesNotContain(typeof(Base), types);
        }
    }

    public class SizeCalculations
    {
        [Fact]
        public void SizeOfForPrimitive()
        {
            Assert.Equal(sizeof(int), typeof(int).SizeOf);
        }

        [Fact]
        public void SizeOfForReferenceTypeEqualsPointerSize()
        {
            Assert.Equal(IntPtr.Size, typeof(object).SizeOf);
        }

        [Fact]
        public void SizeOfForCustomStruct()
        {
            Assert.Equal(sizeof(int) * 2, typeof(MyStruct).SizeOf);
        }
    }
}