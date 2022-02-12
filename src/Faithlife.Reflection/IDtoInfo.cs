using System;
using System.Collections.Generic;

namespace Faithlife.Reflection;

/// <summary>
/// Information about a DTO type.
/// </summary>
/// <remarks>Do not implement this interface. Adding members to this interface will
/// not be considered a breaking change to this library.</remarks>
public interface IDtoInfo
{
	/// <summary>
	/// The properties of the DTO.
	/// </summary>
	/// <remarks>Includes public non-static read/write and read-only properties and fields.</remarks>
	IReadOnlyList<IDtoProperty> Properties { get; }

	/// <summary>
	/// Returns the property of the specified name.
	/// </summary>
	/// <param name="name">The property name.</param>
	/// <exception cref="ArgumentException">The property does not exist.</exception>
	IDtoProperty GetProperty(string name);

	/// <summary>
	/// Returns the property of the specified name.
	/// </summary>
	/// <param name="name">The property name.</param>
	/// <returns>Returns <c>null</c> if the property does not exist.</returns>
	IDtoProperty? TryGetProperty(string name);

	/// <summary>
	/// Creates a new instance of the DTO.
	/// </summary>
	object CreateNew();

	/// <summary>
	/// Creates a new instance of the DTO, assigning properties with the specified names to the specified values.
	/// </summary>
	/// <param name="propertyValues">The property names and values.</param>
	/// <remarks>If possible, the instance is created with the public default constructor, after which the specified
	/// properties (if any) are set to the specified values. If there is no public default constructor and/or one or more
	/// of the specified properties are read-only, the instance is created with a public constructor whose parameters
	/// match the properties of the DTO.</remarks>
	object CreateNew(IEnumerable<(IDtoProperty Property, object? Value)> propertyValues);

	/// <summary>
	/// Clones the specified DTO by copying each property into a new instance.
	/// </summary>
	/// <param name="value">The instance to clone.</param>
	object ShallowClone(object value);
}
