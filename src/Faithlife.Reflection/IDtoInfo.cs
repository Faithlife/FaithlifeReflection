using System;
using System.Collections.Generic;

namespace Faithlife.Reflection
{
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
		/// <remarks>The DTO must have a public default constructor.</remarks>
		object CreateNew();

		/// <summary>
		/// Clones the specified DTO by copying each property into a new instance.
		/// </summary>
		/// <param name="value">The instance to clone.</param>
		/// <remarks>The DTO must have a public default constructor, and all properties must be read/write.</remarks>
		object ShallowClone(object value);
	}
}
