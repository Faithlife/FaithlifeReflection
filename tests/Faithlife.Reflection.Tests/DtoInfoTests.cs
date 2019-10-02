using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using static FluentAssertions.FluentActions;

#pragma warning disable 414, 649

namespace Faithlife.Reflection.Tests
{
	[TestFixture]
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

			OneProperty dto = new OneProperty { Integer = 42 };
			info.ShallowClone(dto).Integer.Should().Be(dto.Integer);
		}

		[Test]
		public void OnePropertyWeakInfoTests()
		{
			IDtoInfo info = DtoInfo.GetInfo(typeof(OneProperty));
			info.Properties.Count.Should().Be(1);
			info.CreateNew().GetType().Should().Be(typeof(OneProperty));

			OneProperty dto = new OneProperty { Integer = 42 };
			((OneProperty) info.ShallowClone(dto)).Integer.Should().Be(dto.Integer);

			info.GetProperty("Integer").Name.Should().Be("Integer");
			info.TryGetProperty("Integer")!.Name.Should().Be("Integer");
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
			((PropertyInfo) property.MemberInfo).GetMethod.Name.Should().Be("get_Integer");

			OneProperty dto = new OneProperty { Integer = 42 };
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
			((PropertyInfo) property.MemberInfo).GetMethod.Name.Should().Be("get_Integer");

			OneProperty dto = new OneProperty { Integer = 42 };
			property.GetValue(dto).Should().Be(dto.Integer);
			property.SetValue(dto, 24);
			dto.Integer.Should().Be(24);
		}

		[Test]
		public void OnePropertyWeakestPropertyTests()
		{
			DtoInfo<OneProperty> info = DtoInfo.GetInfo<OneProperty>();
			IDtoProperty property = (IDtoProperty) info.Properties.Single();
			property.Name.Should().Be("Integer");
			property.ValueType.Should().Be(typeof(int));
			property.IsReadOnly.Should().BeFalse();
			((PropertyInfo) property.MemberInfo).GetMethod.Name.Should().Be("get_Integer");

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

			OneField dto = new OneField { Integer = 42 };
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

			OneField dto = new OneField { Integer = 42 };
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

			OneField dto = new OneField { Integer = 42 };
			property.GetValue(dto).Should().Be(dto.Integer);
			property.SetValue(dto, 24);
			dto.Integer.Should().Be(24);
		}

		[Test]
		public void OneFieldWeakestFieldTests()
		{
			DtoInfo<OneField> info = DtoInfo.GetInfo<OneField>();
			IDtoProperty property = (IDtoProperty) info.Properties.Single();
			property.Name.Should().Be("Integer");
			property.ValueType.Should().Be(typeof(int));
			property.IsReadOnly.Should().BeFalse();
			((FieldInfo) property.MemberInfo).Name.Should().Be("Integer");

			OneField dto = new OneField { Integer = 42 };
			property.GetValue(dto).Should().Be(dto.Integer);
			property.SetValue(dto, 24);
			dto.Integer.Should().Be(24);
		}

		[Test]
		public void TwoReadOnlyTests()
		{
			DtoInfo<TwoReadOnly> info = DtoInfo.GetInfo<TwoReadOnly>();
			info.Properties.Count.Should().Be(2);
			Invoking(() => info.CreateNew()).Should().Throw<ArgumentException>();

			TwoReadOnly dto = new TwoReadOnly(1, 2);
			Invoking(() => info.ShallowClone(dto)).Should().Throw<ArgumentException>();

			var property = info.GetProperty<int>("IntegerProperty");
			info.GetProperty(x => x.IntegerProperty).Should().Be(property);
			property.Name.Should().Be("IntegerProperty");
			property.ValueType.Should().Be(typeof(int));
			property.IsReadOnly.Should().BeTrue();
			((PropertyInfo) property.MemberInfo).GetMethod.Name.Should().Be("get_IntegerProperty");
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
			DtoInfo<T> getInfo<T>(T _) => DtoInfo.GetInfo<T>();
			var obj = new { Integer = 3, String = "three" };
			var info = getInfo(obj);
			info.Properties.Should().HaveCount(2);
			var property = info.GetProperty("Integer");
			property.IsReadOnly.Should().BeTrue();
			property.GetValue(obj).Should().Be(3);
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

		private class TwoReadOnly
		{
			public TwoReadOnly(int one, int two)
			{
				IntegerProperty = one;
				IntegerField = 2;
			}

			public int IntegerProperty { get; }

			public readonly int IntegerField;

			public int WriteOnlyProperty { set { } }

			public static int StaticIntegerProperty { get; } = 3;

			public const int ConstIntegerField = 4;

			public static readonly int StaticIntegerField = 5;

			protected int ProtectedProperty { get; } = 6;

			readonly int m_privateField = 7;
		}
	}
}
