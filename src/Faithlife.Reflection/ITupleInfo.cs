using System;
using System.Collections.Generic;

namespace Faithlife.Reflection;

/// <summary>
/// Weakly-typed information about a tuple type.
/// </summary>
/// <remarks>Do not implement this interface. Adding members to this interface will
/// not be considered a breaking change to this library.</remarks>
public interface ITupleInfo
{
	/// <summary>
	/// The type of tuple.
	/// </summary>
	Type TupleType { get; }

	/// <summary>
	/// The types of items in the tuple.
	/// </summary>
	IReadOnlyList<Type> ItemTypes { get; }

	/// <summary>
	/// Creates a tuple from the specified items.
	/// </summary>
	object CreateNew(IEnumerable<object?> items);
}
