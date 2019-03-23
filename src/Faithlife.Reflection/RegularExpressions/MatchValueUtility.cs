using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Faithlife.Reflection.RegularExpressions
{
	/// <summary>
	/// Extension methods for extracting values from regular expression matches.
	/// </summary>
	/// <remarks>See the library documentation for a detailed list of supported types.</remarks>
	public static class MatchValueUtility
	{
		/// <summary>
		/// Attempts to return a value of the specified type for the match.
		/// </summary>
		/// <typeparam name="T">The desired type.</typeparam>
		/// <param name="match">The match.</param>
		/// <param name="value">The returned value.</param>
		/// <returns>True if the match was successful; false otherwise.</returns>
		public static bool TryGet<T>(this Match match, out T value)
		{
			if (!match.Success)
			{
				value = default;
				return false;
			}

			var type = typeof(T);
			object result;

			if (TupleInfo.IsTupleType(type))
			{
				var tupleInfo = TupleInfo.GetInfo(type);
				var tupleTypes = tupleInfo.ItemTypes;
				var count = tupleTypes.Count;
				if (count < 2)
					throw new InvalidOperationException($"Tuple must have at least two types: {type.FullName}");
				if (match.Groups.Count < count + 1)
					throw new InvalidOperationException($"Regex must have at least {count} capturing groups; it has {match.Groups.Count - 1}.");

				var items = new object[count];
				for (int index = 0; index < count; index++)
					items[index] = ConvertSimple(tupleTypes[index], match.Groups[index + 1]);
				result = tupleInfo.CreateNew(items);
			}
			else
			{
				result = ConvertSimple(type, match.Groups.Count > 1 ? match.Groups[1] : match.Groups[0]);
			}

			value = (T) result;
			return true;
		}

		/// <summary>
		/// Return a value of the specified type for the match.
		/// </summary>
		/// <typeparam name="T">The desired type.</typeparam>
		/// <param name="match">The match.</param>
		/// <returns>The corresponding value if the match was successful; <c>default(T)</c> otherwise.</returns>
		public static T Get<T>(this Match match) => match.TryGet(out T value) ? value : default;

		private static object ConvertSimple(Type type, Group group)
		{
			if (type == typeof(bool))
				return group.Success;
			else if (type == typeof(Group))
				return group;
			else if (type.IsArray)
				return group.Success ? ConvertArray(type.GetElementType(), group) : null;
			else
				return ConvertSimple(type, group, group.Success);
		}

		private static object ConvertArray(Type itemType, Group group)
		{
			var captures = group.Captures;
			int count = captures.Count;
			var array = Array.CreateInstance(itemType, count);
			for (int index = 0; index < count; index++)
				array.SetValue(ConvertSimple(itemType, captures[index]), index);
			return array;
		}

		private static object ConvertSimple(Type type, Capture capture, bool success = true)
		{
			if (type == typeof(string))
				return success ? capture.Value : null;
			else if (type == typeof(int))
				return success ? int.Parse(capture.Value, CultureInfo.InvariantCulture) : default(int);
			else if (type == typeof(int?))
				return success ? int.Parse(capture.Value, CultureInfo.InvariantCulture) : default(int?);
			else if (type == typeof(long))
				return success ? long.Parse(capture.Value, CultureInfo.InvariantCulture) : default(long);
			else if (type == typeof(long?))
				return success ? long.Parse(capture.Value, CultureInfo.InvariantCulture) : default(long?);
			else if (type == typeof(uint))
				return success ? uint.Parse(capture.Value, CultureInfo.InvariantCulture) : default(uint);
			else if (type == typeof(uint?))
				return success ? uint.Parse(capture.Value, CultureInfo.InvariantCulture) : default(uint?);
			else if (type == typeof(ulong))
				return success ? ulong.Parse(capture.Value, CultureInfo.InvariantCulture) : default(ulong);
			else if (type == typeof(ulong?))
				return success ? ulong.Parse(capture.Value, CultureInfo.InvariantCulture) : default(ulong?);
			else if (type == typeof(Capture))
				return success ? capture : null;
			else
				throw new InvalidOperationException($"Type not supported: {type.FullName}");
		}
	}
}
