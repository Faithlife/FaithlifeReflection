using System;
using System.Linq;
using System.Text.RegularExpressions;
using Faithlife.Reflection.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;

namespace Faithlife.Reflection.Tests.RegularExpressions
{
	[TestFixture]
	public class MatchValueUtilityTests
	{
		[Test]
		public void StringFailedMatch()
		{
			Regex.Match("expressions", "c+").Get<string>().Should().BeNull();
		}

		[Test]
		public void StringNoGroupMatch()
		{
			Regex.Match("expressions", "s+").Get<string>().Should().Be("ss");
		}

		[Test]
		public void StringOneGroupMatch()
		{
			Regex.Match("expressions", "s+([aeiou]+)").Get<string>().Should().Be("io");
		}

		[Test]
		public void IntegerFailedMatch()
		{
			var text = "number";
			s_signedIntegerRegex.Match(text).Get<int>().Should().Be(0);
			s_signedIntegerRegex.Match(text).Get<int?>().Should().BeNull();
			s_signedIntegerRegex.Match(text).Get<long>().Should().Be(0);
			s_signedIntegerRegex.Match(text).Get<long?>().Should().BeNull();
			s_unsignedIntegerRegex.Match(text).Get<uint>().Should().Be(0);
			s_unsignedIntegerRegex.Match(text).Get<uint?>().Should().BeNull();
			s_unsignedIntegerRegex.Match(text).Get<ulong>().Should().Be(0);
			s_unsignedIntegerRegex.Match(text).Get<ulong?>().Should().BeNull();
		}

		[Test]
		public void IntegerNoGroupMatch()
		{
			var text = "number: -123";
			s_signedIntegerRegex.Match(text).Get<int>().Should().Be(-123);
			s_signedIntegerRegex.Match(text).Get<int?>().Should().Be(-123);
			s_signedIntegerRegex.Match(text).Get<long>().Should().Be(-123);
			s_signedIntegerRegex.Match(text).Get<long?>().Should().Be(-123);
			s_unsignedIntegerRegex.Match(text).Get<uint>().Should().Be(123);
			s_unsignedIntegerRegex.Match(text).Get<uint?>().Should().Be(123);
			s_unsignedIntegerRegex.Match(text).Get<ulong>().Should().Be(123);
			s_unsignedIntegerRegex.Match(text).Get<ulong?>().Should().Be(123);
		}

		[Test]
		public void IntegerOverflowException()
		{
			var text = "number: -123";
			Invoking(() => s_signedIntegerRegex.Match(text).Get<ulong>()).Should().Throw<OverflowException>();
		}

		[Test]
		public void ThreeTupleMatch()
		{
			var match = Regex.Match("on 22 March 2019", @"([0-9]+)\s+([A-Z][a-z]+)\s+([0-9]+)");
			match.Get<(int, string, long)>().Should().Be((22, "March", 2019L));
			match.Get<(int, string, long)?>().Should().Be((22, "March", 2019L));
		}

		[Test]
		public void TupleNoMatch()
		{
			var match = Regex.Match("nope", "(a) (b)");
			match.Get<(string, int)>().Should().Be((default(string), 0));
			match.Get<(string, int?)>().Should().Be((default(string), default(int?)));
			match.Get<(string, int)?>().Should().BeNull();
		}

		[Test]
		public void TypeNotSupported()
		{
			Invoking(() => Regex.Match("type", "t+").Get<Type>()).Should().Throw<InvalidOperationException>();
		}

		[Test]
		public void SmallTuplesNotSupported()
		{
			Invoking(() => Regex.Match("type", "t+").Get<ValueTuple>()).Should().Throw<InvalidOperationException>();
			Invoking(() => Regex.Match("type", "t+").Get<ValueTuple<string>>()).Should().Throw<InvalidOperationException>();
		}

		[Test]
		public void NotEnoughGroups()
		{
			Invoking(() => Regex.Match("type", "t+").Get<(string, string)>()).Should().Throw<InvalidOperationException>();
			Invoking(() => Regex.Match("type", "(t+)").Get<(string, string)>()).Should().Throw<InvalidOperationException>();
		}

		[Test]
		public void TooManyGroups()
		{
			Regex.Match("1 2 3", @"(\d) (\d) (\d)").Get<string>().Should().Be("1");
			Regex.Match("1 2 3", @"(\d) (\d) (\d)").Get<(string, string)>().Should().Be(("1", "2"));
		}

		[Test]
		public void NonCapturingGroups()
		{
			Regex.Match("1 2 3", @"(?:\d) (\d) (\d)").Get<string>().Should().Be("2");
			Regex.Match("1 2 3", @"(\d) (?:\d) (\d)").Get<(string, string)>().Should().Be(("1", "3"));
		}

		[Test]
		public void GetGroup()
		{
			Regex.Match("1 2 3", @"(\d) (\d) (\d)").Get<Group>().Value.Should().Be("1");
			Regex.Match("1 2 3", @"(\d) (\d) (\d)").Get<(string, Group)>().Item2.Value.Should().Be("2");
		}

		[Test]
		public void OptionalGroups()
		{
			Regex.Match("ac", @"(a)(b)?(c)").Get<(string, string, string)>().Should().Be(("a", null, "c"));
			Regex.Match("ac", @"(a)(b?)(c)").Get<(string, string, string)>().Should().Be(("a", "", "c"));
		}

		[Test]
		public void BooleanValues()
		{
			Regex.Match("ac", @"(a)(b)?(c)").Get<(string, bool, bool)>().Should().Be(("a", false, true));
			Regex.Match("ac", @"(a)(b?)(c)").Get<(string, bool, bool)>().Should().Be(("a", true, true));
		}

		[Test]
		public void Matches()
		{
			Regex.Matches("867-5309", "[0-9]+").Select(x => x.Get<int>()).Should().Equal(867, 5309);
		}

		[Test]
		public void AddInPlace()
		{
			string addPairs(Match match)
			{
				(int first, int second) = match.Get<(int, int)>();
				return $"{first + second}";
			}

			Regex.Replace("1+2 3+4 5+6", @"([0-9]+)\+([0-9]+)", addPairs).Should().Be("3 7 11");
		}

		[Test]
		public void LastCapture()
		{
			Regex.Match("find 1 2 3 5 8", @"(([0-9]+)\s*)+").Get<(bool, int)>().Should().Be((true, 8));
		}

		[Test]
		public void CapturesAsIntegers()
		{
			(bool match, int[] numbers) = Regex.Match("find 1 2 3 5 8", @"(([0-9]+)\s*)+").Get<(bool, int[])>();
			match.Should().BeTrue();
			numbers.Should().Equal(1, 2, 3, 5, 8);
		}

		[Test]
		public void CapturesAsCaptures()
		{
			Regex.Match("find 1 2 3 5 8", @"(?:([0-9]+)\s*)+").Get<Capture[]>().Select(x => x.Value).Should().Equal("1", "2", "3", "5", "8");
		}

		static readonly Regex s_signedIntegerRegex = new Regex(@"[-0-9]+");
		static readonly Regex s_unsignedIntegerRegex = new Regex(@"[0-9]+");

		private static Action Invoking(Action action) => action;
	}
}
