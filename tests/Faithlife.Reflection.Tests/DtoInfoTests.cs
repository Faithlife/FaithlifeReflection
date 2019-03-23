using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;

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
			info.ShallowClone(null).Should().BeNull();
			info.ShallowClone(new EmptyDto()).Should().NotBeNull();
			Invoking(() => info.GetProperty("Nope")).Should().Throw<ArgumentException>();
			info.TryGetProperty("Nope").Should().BeNull();
			Invoking(() => info.GetProperty<int>("Nope")).Should().Throw<ArgumentException>();
			info.TryGetProperty<int>("Nope").Should().BeNull();
		}

		[Test]
		public void OnePropertyInfoTests()
		{
			DtoInfo<OneProperty> info = DtoInfo.GetInfo<OneProperty>();
			info.Properties.Count.Should().Be(1);
			info.CreateNew().GetType().Should().Be(typeof(OneProperty));
			info.ShallowClone(null).Should().BeNull();

			OneProperty dto = new OneProperty { Integer = 42 };
			info.ShallowClone(dto).Integer.Should().Be(dto.Integer);
		}

		[Test]
		public void OnePropertyWeakInfoTests()
		{
			IDtoInfo info = DtoInfo.GetInfo(typeof(OneProperty));
			info.Properties.Count.Should().Be(1);
			info.CreateNew().GetType().Should().Be(typeof(OneProperty));
			info.ShallowClone(null).Should().BeNull();

			OneProperty dto = new OneProperty { Integer = 42 };
			((OneProperty) info.ShallowClone(dto)).Integer.Should().Be(dto.Integer);

			info.GetProperty("Integer").Name.Should().Be("Integer");
			info.TryGetProperty("Integer").Name.Should().Be("Integer");
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
			info.ShallowClone(null).Should().BeNull();

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

		private sealed class EmptyDto
		{
		}

		private sealed class OneProperty
		{
			public int Integer { get; set; }
		}

		private sealed class OneField
		{
#pragma warning disable 649
			public int Integer;
#pragma warning restore 649
		}

		private class TwoReadOnly
		{
			public TwoReadOnly(int one, int two)
			{
				IntegerProperty = one;
				IntegerField = 2;
			}

			public int IntegerProperty { get; }

#pragma warning disable 414
			public readonly int IntegerField;
#pragma warning restore 414

			public int WriteOnlyProperty { set { } }

			public static int StaticIntegerProperty { get; } = 3;

			public const int ConstIntegerField = 4;

#pragma warning disable 414
			public static readonly int StaticIntegerField = 5;
#pragma warning restore 414

			protected int ProtectedProperty { get; } = 6;

#pragma warning disable 414
			readonly int m_privateField = 7;
#pragma warning restore 414
		}

		private static Action Invoking(Action action) => action;
	}
}
