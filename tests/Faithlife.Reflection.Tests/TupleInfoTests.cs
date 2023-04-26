using FluentAssertions;
using NUnit.Framework;
using static FluentAssertions.FluentActions;

namespace Faithlife.Reflection.Tests;

[TestFixture]
public class TupleInfoTests
{
	[Test]
	public void OneValueTupleTests()
	{
		var info = TupleInfo.GetInfo<ValueTuple<int>>();
		info.TupleType.Should().Be(typeof(ValueTuple<int>));
		info.ItemTypes.Should().Equal(typeof(int));
		info.CreateNew(new object[] { 1 }).Should().Be(ValueTuple.Create(1));
	}

	[Test]
	public void TwoValueTupleTests()
	{
		var info = TupleInfo.GetInfo<(int, string)>();
		info.TupleType.Should().Be(typeof((int, string)));
		info.ItemTypes.Should().Equal(typeof(int), typeof(string));
		info.CreateNew(new object[] { 1, "one" }).Should().Be((1, "one"));
	}

	[Test]
	public void TenValueTupleTests()
	{
		var info = TupleInfo.GetInfo<(bool, byte, short, int, long, long, int, short, byte, bool)>();
		info.TupleType.Should().Be(typeof((bool, byte, short, int, long, long, int, short, byte, bool)));
		info.ItemTypes.Should().Equal(typeof(bool), typeof(byte), typeof(short), typeof(int), typeof(long), typeof(long), typeof(int), typeof(short), typeof(byte), typeof(bool));
		info.CreateNew(new object[] { false, (byte) 0, (short) 0, 0, 0L, 1L, 1, (short) 1, (byte) 1, true }).Should().Be((false, 0, 0, 0, 0L, 1L, 1, 1, 1, true));
	}

	[Test]
	public void WeakTenValueTupleTests()
	{
		var info = TupleInfo.GetInfo(typeof((bool, byte, short, int, long, long, int, short, byte, bool)));
		info.TupleType.Should().Be(typeof((bool, byte, short, int, long, long, int, short, byte, bool)));
		info.ItemTypes.Should().Equal(typeof(bool), typeof(byte), typeof(short), typeof(int), typeof(long), typeof(long), typeof(int), typeof(short), typeof(byte), typeof(bool));
		info.CreateNew(new object[] { false, (byte) 0, (short) 0, 0, 0L, 1L, 1, (short) 1, (byte) 1, true }).Should().Be((false, (byte) 0, (short) 0, 0, 0L, 1L, 1, (short) 1, (byte) 1, true));
	}

	[Test]
	public void ThirtyValueTupleTests()
	{
		var info = TupleInfo.GetInfo<(int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int)>();
		info.TupleType.Should().Be(typeof((int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int)));
		info.ItemTypes.Should().Equal(Enumerable.Repeat(typeof(int), 30));
		info.CreateNew(Enumerable.Repeat((object) 1, 30)).Should().Be((1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1));
	}

	[Test]
	public void ZeroValueTupleTests()
	{
		var info = TupleInfo.GetInfo<ValueTuple>();
		info.TupleType.Should().Be(typeof(ValueTuple));
		info.ItemTypes.Should().BeEmpty();
		info.CreateNew(Array.Empty<object>()).Should().Be(ValueTuple.Create());
	}

	[Test]
	public void NullableTwoValueTupleTests()
	{
		var info = TupleInfo.GetInfo<(int, string)?>();
		info.TupleType.Should().Be(typeof((int, string)?));
		info.ItemTypes.Should().Equal(typeof(int), typeof(string));
		info.CreateNew(new object[] { 1, "one" }).Should().Be((1, "one"));
	}

	[Test]
	public void CreateNewValueTupleBadItemType()
	{
		Invoking(() => TupleInfo.GetInfo<(int, string)>().CreateNew(new object[] { 1, 2 }))
			.Should().Throw<ArgumentException>();
	}

	[Test]
	public void CreateNewValueTupleTooFewItems()
	{
		Invoking(() => TupleInfo.GetInfo<(int, string)>().CreateNew(new object[] { 1 }))
			.Should().Throw<ArgumentException>();
	}

	[Test]
	public void CreateNewValueTupleTooManyItems()
	{
		Invoking(() => TupleInfo.GetInfo<(int, string)>().CreateNew(new object[] { 1, "one", true }))
			.Should().Throw<ArgumentException>();
	}

	[Test]
	public void StrongNonTupleType()
	{
		Invoking(TupleInfo.GetInfo<int>)
			.Should().Throw<InvalidOperationException>();
	}

	[Test]
	public void WeakNonTupleType()
	{
		Invoking(() => TupleInfo.GetInfo(typeof(int)))
			.Should().Throw<InvalidOperationException>();
	}

	[Test]
	public void OneTupleTests()
	{
		var info = TupleInfo.GetInfo<Tuple<int>>();
		info.TupleType.Should().Be(typeof(Tuple<int>));
		info.ItemTypes.Should().Equal(typeof(int));
		info.CreateNew(new object[] { 1 }).Should().Be(new Tuple<int>(1));
	}

	[Test]
	public void SevenTupleTests()
	{
		var info = TupleInfo.GetInfo<Tuple<int, int, int, int, int, int, int>>();
		info.TupleType.Should().Be(typeof(Tuple<int, int, int, int, int, int, int>));
		info.ItemTypes.Should().Equal(Enumerable.Repeat(typeof(int), 7));
		info.CreateNew(new object[] { 1, 2, 3, 4, 5, 6, 7 }).Should().Be(Tuple.Create(1, 2, 3, 4, 5, 6, 7));
	}

	[Test]
	public void EightTupleTests()
	{
		var info = TupleInfo.GetInfo<Tuple<int, int, int, int, int, int, int, Tuple<int>>>();
		info.TupleType.Should().Be(typeof(Tuple<int, int, int, int, int, int, int, Tuple<int>>));
		info.ItemTypes.Should().Equal(Enumerable.Repeat(typeof(int), 8));
		info.CreateNew(new object[] { 1, 2, 3, 4, 5, 6, 7, 8 }).Should().Be(Tuple.Create(1, 2, 3, 4, 5, 6, 7, 8));
	}

	[Test]
	public void IsTupleType()
	{
		TupleInfo.IsTupleType(typeof(object)).Should().BeFalse();
		TupleInfo.IsTupleType(typeof(string)).Should().BeFalse();
		TupleInfo.IsTupleType(typeof(ValueTuple<bool>)).Should().BeTrue();
		TupleInfo.IsTupleType(typeof(ValueTuple)).Should().BeTrue();
		TupleInfo.IsTupleType(typeof(Tuple<bool>)).Should().BeTrue();
		TupleInfo.IsTupleType(typeof(Tuple)).Should().BeFalse();
	}

	[Test]
	public void IsNullableValueTupleType()
	{
		TupleInfo.IsTupleType(typeof(ValueTuple<bool>?)).Should().BeTrue();
		TupleInfo.IsTupleType(typeof(ValueTuple?)).Should().BeTrue();
	}

	[Test]
	public void IsTuple()
	{
		TupleInfo.IsTuple(null!).Should().BeFalse();
		TupleInfo.IsTuple("ValueTuple").Should().BeFalse();
		TupleInfo.IsTuple(ValueTuple.Create(true)).Should().BeTrue();
		TupleInfo.IsTuple(default(ValueTuple)).Should().BeTrue();
		TupleInfo.IsTuple(Tuple.Create(true)).Should().BeTrue();
		TupleInfo.IsTuple(default(ValueTuple?)).Should().BeFalse();
	}
}
