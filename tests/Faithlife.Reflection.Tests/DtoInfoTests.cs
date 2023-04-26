using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using static FluentAssertions.FluentActions;

#pragma warning disable 414, 628, 649

namespace Faithlife.Reflection.Tests;

[TestFixture]
[SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Testing.")]
[SuppressMessage("Performance", "CA1802:Use literals where appropriate", Justification = "Testing.")]
[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Testing.")]
[SuppressMessage("Performance", "CA1823:Avoid unused private fields", Justification = "Testing.")]
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Testing.")]
[SuppressMessage("ReSharper", "UnusedParameter.Local", Justification = "Testing.")]
[SuppressMessage("ReSharper", "ValueParameterNotUsed", Justification = "Testing.")]
public class DtoInfoTests
{
	[Test]
	public void EmptyDtoTests()
	{
		DtoInfo<EmptyDto> info = DtoInfo.GetInfo<EmptyDto>();
		info.Properties.Should().BeEmpty();
		info.CreateNew().GetType().Should().Be(typeof(EmptyDto));
		info.ShallowClone(new EmptyDto()).Should().NotBeNull();
		Invoking(() => info.GetProperty("Nope")).Should().Throw<ArgumentException>();
		info.TryGetProperty("Nope").Should().BeNull();
		Invoking(() => info.GetProperty<int>("Nope")).Should().Throw<ArgumentException>();
		info.TryGetProperty<int>("Nope").Should().BeNull();
	}

	[Test]
	public void ShallowCloneThrowsOnNull()
	{
		DtoInfo<EmptyDto> info = DtoInfo.GetInfo<EmptyDto>();
		Invoking(() => info.ShallowClone(null!)).Should().Throw<ArgumentNullException>();
	}

	[Test]
	public void OnePropertyInfoTests()
	{
		DtoInfo<OneProperty> info = DtoInfo.GetInfo<OneProperty>();
		info.Properties.Count.Should().Be(1);
		info.CreateNew().GetType().Should().Be(typeof(OneProperty));

		OneProperty dto = new() { Integer = 42 };
		info.ShallowClone(dto).Integer.Should().Be(dto.Integer);
	}

	[Test]
	public void OnePropertyWeakInfoTests()
	{
		IDtoInfo info = DtoInfo.GetInfo(typeof(OneProperty));
		info.Properties.Count.Should().Be(1);
		info.CreateNew().GetType().Should().Be(typeof(OneProperty));

		OneProperty dto = new() { Integer = 42 };
		((OneProperty) info.ShallowClone(dto)).Integer.Should().Be(dto.Integer);

		info.GetProperty("Integer").Name.Should().Be("Integer");
		info.TryGetProperty("Integer")!.Name.Should().Be("Integer");

		info.GetProperty("integer").Name.Should().Be("Integer");
		info.TryGetProperty("integer")!.Name.Should().Be("Integer");
	}

	[Test]
	public void OnePropertyStrongPropertyTests()
	{
		DtoInfo<OneProperty> info = DtoInfo.GetInfo<OneProperty>();
		DtoProperty<OneProperty, int> property = info.GetProperty<int>("Integer");
		info.GetProperty(x => x.Integer).Should().Be(property);
		property.Name.Should().Be("Integer");
		property.ValueType.Should().Be(typeof(int));
		property.IsReadOnly.Should().BeFalse();
		((PropertyInfo) property.MemberInfo).GetMethod!.Name.Should().Be("get_Integer");

		OneProperty dto = new() { Integer = 42 };
		property.GetValue(dto).Should().Be(dto.Integer);
		property.SetValue(dto, 24);
		dto.Integer.Should().Be(24);
	}

	[Test]
	public void OnePropertyWeakPropertyTests()
	{
		DtoInfo<OneProperty> info = DtoInfo.GetInfo<OneProperty>();
		IDtoProperty<OneProperty> property = info.GetProperty("Integer");
		property.Name.Should().Be("Integer");
		property.ValueType.Should().Be(typeof(int));
		property.IsReadOnly.Should().BeFalse();
		((PropertyInfo) property.MemberInfo).GetMethod!.Name.Should().Be("get_Integer");

		OneProperty dto = new() { Integer = 42 };
		property.GetValue(dto).Should().Be(dto.Integer);
		property.SetValue(dto, 24);
		dto.Integer.Should().Be(24);
	}

	[Test]
	public void OnePropertyWeakestPropertyTests()
	{
		DtoInfo<OneProperty> info = DtoInfo.GetInfo<OneProperty>();
		IDtoProperty property = info.Properties.Single();
		property.Name.Should().Be("Integer");
		property.ValueType.Should().Be(typeof(int));
		property.IsReadOnly.Should().BeFalse();
		((PropertyInfo) property.MemberInfo).GetMethod!.Name.Should().Be("get_Integer");

		var dto = new OneProperty { Integer = 42 };
		property.GetValue(dto).Should().Be(dto.Integer);
		property.SetValue(dto, 24);
		dto.Integer.Should().Be(24);
	}

	[Test]
	public void OneFieldInfoTests()
	{
		DtoInfo<OneField> info = DtoInfo.GetInfo<OneField>();
		info.Properties.Count.Should().Be(1);
		info.CreateNew().GetType().Should().Be(typeof(OneField));

		OneField dto = new() { Integer = 42 };
		info.ShallowClone(dto).Integer.Should().Be(dto.Integer);
	}

	[Test]
	public void OneFieldStrongFieldTests()
	{
		DtoInfo<OneField> info = DtoInfo.GetInfo<OneField>();
		DtoProperty<OneField, int> property = info.GetProperty<int>("Integer");
		info.GetProperty(x => x.Integer).Should().Be(property);
		property.Name.Should().Be("Integer");
		property.ValueType.Should().Be(typeof(int));
		property.IsReadOnly.Should().BeFalse();
		((FieldInfo) property.MemberInfo).Name.Should().Be("Integer");

		OneField dto = new() { Integer = 42 };
		property.GetValue(dto).Should().Be(dto.Integer);
		property.SetValue(dto, 24);
		dto.Integer.Should().Be(24);
	}

	[Test]
	public void OneFieldWeakFieldTests()
	{
		DtoInfo<OneField> info = DtoInfo.GetInfo<OneField>();
		IDtoProperty<OneField> property = info.GetProperty("Integer");
		property.Name.Should().Be("Integer");
		property.ValueType.Should().Be(typeof(int));
		property.IsReadOnly.Should().BeFalse();
		((FieldInfo) property.MemberInfo).Name.Should().Be("Integer");

		OneField dto = new() { Integer = 42 };
		property.GetValue(dto).Should().Be(dto.Integer);
		property.SetValue(dto, 24);
		dto.Integer.Should().Be(24);
	}

	[Test]
	public void OneFieldWeakestFieldTests()
	{
		DtoInfo<OneField> info = DtoInfo.GetInfo<OneField>();
		IDtoProperty property = info.Properties.Single();
		property.Name.Should().Be("Integer");
		property.ValueType.Should().Be(typeof(int));
		property.IsReadOnly.Should().BeFalse();
		((FieldInfo) property.MemberInfo).Name.Should().Be("Integer");

		OneField dto = new() { Integer = 42 };
		property.GetValue(dto).Should().Be(dto.Integer);
		property.SetValue(dto, 24);
		dto.Integer.Should().Be(24);
	}

	[Test]
	public void StrongMixedDto()
	{
		var info = DtoInfo.GetInfo<MixedDto>();
		info.CreateNew().Should().BeEquivalentTo(new MixedDto());
		info.CreateNew(("string", "hey")).Should().BeEquivalentTo(new MixedDto { String = "hey" });
		info.CreateNew(("integer", 1), ("string", "wow")).Should().BeEquivalentTo(new MixedDto(1) { String = "wow" });
	}

	[Test]
	public void WeakMixedDto()
	{
		IDtoInfo info = DtoInfo.GetInfo(typeof(MixedDto));
		info.CreateNew().Should().BeEquivalentTo(new MixedDto());
		info.CreateNew(("string", "hey")).Should().BeEquivalentTo(new MixedDto { String = "hey" });
		info.CreateNew(("integer", 1), ("string", "wow")).Should().BeEquivalentTo(new MixedDto(1) { String = "wow" });
	}

	[Test]
	public void StrongPoint()
	{
		var info = DtoInfo.GetInfo<Point>();
		info.CreateNew().Should().Be(default(Point));
		info.CreateNew(("x", 1)).Should().Be(new Point { X = 1 });
	}

	[Test]
	public void WeakPoint()
	{
		var info = DtoInfo.GetInfo(typeof(Point));
		info.CreateNew().Should().Be(default(Point));
		info.CreateNew(("x", 1)).Should().Be(new Point { X = 1 });
	}

	[Test]
	public void StrongNullablePoint()
	{
		var info = DtoInfo.GetInfo<Point?>();
		info.CreateNew().Should().Be(default(Point));
		info.CreateNew(("x", 1)).Should().Be(new Point { X = 1 });
	}

	[Test]
	public void WeakNullablePoint()
	{
		var info = DtoInfo.GetInfo(typeof(Point?));
		info.CreateNew().Should().Be(default(Point));
		info.CreateNew(("x", 1)).Should().Be(new Point { X = 1 });
	}

	[Test]
	public void StrongColor()
	{
		var info = DtoInfo.GetInfo<Color>();
		info.CreateNew().Should().Be(new Color(0, 0, 0));
		info.CreateNew(("r", (byte) 1), ("g", (byte) 2), ("b", (byte) 3)).Should().Be(new Color(255, 1, 2, 3));
		info.CreateNew(("r", (byte) 1), ("g", (byte) 2), ("b", (byte) 3), ("a", (byte) 4)).Should().Be(new Color(4, 1, 2, 3));
	}

	[Test]
	public void WeakColor()
	{
		var info = DtoInfo.GetInfo(typeof(Color));
		info.CreateNew().Should().Be(new Color(0, 0, 0));
		info.CreateNew(("r", (byte) 1), ("g", (byte) 2), ("b", (byte) 3)).Should().Be(new Color(255, 1, 2, 3));
		info.CreateNew(("r", (byte) 1), ("g", (byte) 2), ("b", (byte) 3), ("a", (byte) 4)).Should().Be(new Color(4, 1, 2, 3));
	}

	[Test]
	public void WeirdDtoTests()
	{
		DtoInfo<WeirdDto> info = DtoInfo.GetInfo<WeirdDto>();
		info.Properties.Count.Should().Be(2);
		Invoking(() => info.CreateNew()).Should().Throw<InvalidOperationException>();

		WeirdDto dto = new(1, 2);
		Invoking(() => info.ShallowClone(dto)).Should().Throw<InvalidOperationException>();

		var property = info.GetProperty<int>("IntegerProperty");
		info.GetProperty(x => x.IntegerProperty).Should().Be(property);
		property.Name.Should().Be("IntegerProperty");
		property.ValueType.Should().Be(typeof(int));
		property.IsReadOnly.Should().BeTrue();
		((PropertyInfo) property.MemberInfo).GetMethod!.Name.Should().Be("get_IntegerProperty");
		property.GetValue(dto).Should().Be(dto.IntegerProperty);
		Invoking(() => property.SetValue(dto, 24)).Should().Throw<InvalidOperationException>();

		var field = info.GetProperty<int>("IntegerField");
		info.GetProperty(x => x.IntegerField).Should().Be(field);
		field.Name.Should().Be("IntegerField");
		field.ValueType.Should().Be(typeof(int));
		field.IsReadOnly.Should().BeTrue();
		((FieldInfo) field.MemberInfo).Name.Should().Be("IntegerField");
		field.GetValue(dto).Should().Be(dto.IntegerField);
		Invoking(() => field.SetValue(dto, 24)).Should().Throw<InvalidOperationException>();
	}

	[Test]
	public void StrongAnonymousType()
	{
		DtoInfo<T> GetInfo<T>(T t) => DtoInfo.GetInfo<T>();
		var obj = new { Integer = 3, String = "three" };
		var info = GetInfo(obj);
		info.Properties.Should().HaveCount(2);
		var property = info.GetProperty("Integer");
		property.IsReadOnly.Should().BeTrue();
		property.GetValue(obj).Should().Be(3);
		info.ShallowClone(obj).Integer.Should().Be(3);
		info.CreateNew().Integer.Should().Be(0);
		info.CreateNew((property, 4)).Integer.Should().Be(4);
		info.CreateNew(("integer", 4)).Integer.Should().Be(4);
		Invoking(() => info.CreateNew(("nope", 4))).Should().Throw<ArgumentException>();
		Invoking(() => info.CreateNew(("integer", "4"))).Should().Throw<ArgumentException>();
	}

	[Test]
	public void WeakAnonymousType()
	{
		var obj = new { Integer = 3, String = "three" };
		var info = DtoInfo.GetInfo(obj.GetType());
		info.Properties.Should().HaveCount(2);
		var property = info.GetProperty("Integer");
		property.IsReadOnly.Should().BeTrue();
		property.GetValue(obj).Should().Be(3);
		property.GetValue(info.ShallowClone(obj)).Should().Be(3);
		property.GetValue(info.CreateNew()).Should().Be(0);
		property.GetValue(info.CreateNew((property, 4))).Should().Be(4);
		property.GetValue(info.CreateNew(("integer", 4))).Should().Be(4);
		Invoking(() => info.CreateNew(("nope", 4))).Should().Throw<ArgumentException>();
		Invoking(() => info.CreateNew(("integer", "4"))).Should().Throw<ArgumentException>();
	}

	[Test]
	public void StrongInitOnly()
	{
		var info = DtoInfo.GetInfo<InitOnlyDto>();
		var dto = info.CreateNew(("a", 1), ("b", 2));
		dto.A.Should().Be(1);
		dto.B.Should().Be(2);
		info.ShallowClone(dto).A.Should().Be(1);
	}

	private sealed class EmptyDto
	{
	}

	private sealed class OneProperty
	{
		public int Integer { get; set; }
	}

	private sealed class OneField
	{
		public int Integer;
	}

	private sealed class MixedDto
	{
		public MixedDto(int integer = 42)
		{
			Integer = integer;
		}

		public int Integer { get; }

		public string? String { get; set; }
	}

	private struct Point
	{
		public Point(int x, int y) => (X, Y) = (x, y);
		public int X { get; set; }
		public int Y { get; set; }
	}

	private readonly struct Color
	{
		public Color(byte r, byte g, byte b) => (A, R, G, B) = (255, r, g, b);
		public Color(byte a, byte r, byte g, byte b) => (A, R, G, B) = (a, r, g, b);
		public byte A { get; }
		public byte R { get; }
		public byte G { get; }
		public byte B { get; }
	}

	private sealed class WeirdDto
	{
		public WeirdDto(int one, int two)
		{
			IntegerProperty = one;
			IntegerField = two;
		}

		public int IntegerProperty { get; }

		public readonly int IntegerField;

		public int WriteOnlyProperty
		{
			set { }
		}

		public static int StaticIntegerProperty { get; } = 3;

		public const int ConstIntegerField = 4;

		public static readonly int StaticIntegerField = 5;

		protected int ProtectedProperty { get; } = 6;

		private readonly int m_privateField = 7;
	}

	private sealed class InitOnlyDto
	{
		public int A { get; init; }

		public int B { get; init; }
	}
}
