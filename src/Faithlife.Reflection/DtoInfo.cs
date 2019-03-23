using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Faithlife.Reflection
{
	/// <summary>
	/// Helpers for using reflection with DTOs.
	/// </summary>
	/// <remarks>DTOs (data transfer objects) usually consist entirely of read/write non-static properties.
	/// This library also supports DTOs with read-only non-static properties, but those properties cannot
	/// be set, and the DTOs cannot be created or cloned.</remarks>
	public static class DtoInfo
	{
		/// <summary>
		/// Gets information about the specified DTO type.
		/// </summary>
		/// <typeparam name="T">The DTO type.</typeparam>
		public static DtoInfo<T> GetInfo<T>() => DtoInfo<T>.Instance.Value;
	}

	/// <summary>
	/// Information about a DTO type.
	/// </summary>
	/// <typeparam name="T">The DTO type.</typeparam>
	public sealed class DtoInfo<T>
	{
		/// <summary>
		/// The properties of the DTO.
		/// </summary>
		/// <remarks>Includes public non-static read/write and read-only properties and fields.</remarks>
		public IReadOnlyList<IDtoProperty<T>> Properties => m_lazyProperties.Value;

		/// <summary>
		/// Returns the property of the specified name.
		/// </summary>
		/// <param name="name">The property name.</param>
		/// <exception cref="ArgumentException">The property does not exist.</exception>
		public IDtoProperty<T> GetProperty(string name) =>
			TryGetProperty(name) ?? throw new ArgumentException($"Type '{typeof(T).FullName}' does not have a public non-static property or field named '{name}'.");

		/// <summary>
		/// Returns the property of the specified name.
		/// </summary>
		/// <param name="name">The property name.</param>
		/// <returns>Returns <c>null</c> if the property does not exist.</returns>
		public IDtoProperty<T> TryGetProperty(string name) =>
			m_lazyPropertiesByName.Value.TryGetValue(name, out var property) ? property : null;

		/// <summary>
		/// Returns the property named by the specified getter.
		/// </summary>
		/// <typeparam name="TValue">The value type of the property.</typeparam>
		/// <param name="getter">A getter for the property, e.g. <c>x => x.TheProperty</c></param>
		public DtoProperty<T, TValue> GetProperty<TValue>(Expression<Func<T, TValue>> getter) => GetProperty<TValue>(GetPropertyName(getter));

		/// <summary>
		/// Returns the property of the specified name.
		/// </summary>
		/// <typeparam name="TValue">The value type of the property.</typeparam>
		/// <param name="name">The property name.</param>
		/// <exception cref="ArgumentException">The property does not exist.</exception>
		public DtoProperty<T, TValue> GetProperty<TValue>(string name) =>
			TryGetProperty<TValue>(name) ?? throw new ArgumentException($"Type '{typeof(T).FullName}' does not have a public non-static property or field named '{name}' of type '{typeof(TValue).FullName}'.");

		/// <summary>
		/// Returns the property of the specified name.
		/// </summary>
		/// <typeparam name="TValue">The value type of the property.</typeparam>
		/// <param name="name">The property name.</param>
		/// <returns>Returns <c>null</c> if the property does not exist.</returns>
		public DtoProperty<T, TValue> TryGetProperty<TValue>(string name) =>
			m_lazyPropertiesByName.Value.TryGetValue(name, out var property) ? property as DtoProperty<T, TValue> : null;

		/// <summary>
		/// Creates a new instance of the DTO.
		/// </summary>
		/// <remarks>The DTO must have a public default constructor.</remarks>
		public T CreateNew() => m_lazyCreateNew.Value();

		/// <summary>
		/// Clones the specified DTO by copying each property into a new instance.
		/// </summary>
		/// <param name="value">The instance to clone.</param>
		/// <remarks>The DTO must have a public default constructor, and all properties must be read/write.</remarks>
		public T ShallowClone(T value)
		{
			if (value == null)
				return default;

			T clone = CreateNew();
			foreach (var property in Properties)
				property.SetValue(clone, property.GetValue(value));
			return clone;
		}

		internal DtoInfo()
		{
			m_lazyCreateNew = new Lazy<Func<T>>(() => Expression.Lambda<Func<T>>(Expression.New(typeof(T))).Compile());

			m_lazyProperties = new Lazy<IReadOnlyList<IDtoProperty<T>>>(
				() => new ReadOnlyCollection<IDtoProperty<T>>(getProperties().ToList()));

			m_lazyPropertiesByName = new Lazy<IReadOnlyDictionary<string, IDtoProperty<T>>>(
				() => m_lazyProperties.Value.ToDictionary(x => x.Name));

			bool isPublicNonStaticProperty(PropertyInfo info) => info.GetMethod != null && info.GetMethod.IsPublic && !info.GetMethod.IsStatic;

			bool isPublicNonStaticField(FieldInfo info) => info.IsPublic && !info.IsStatic;

			IEnumerable<IDtoProperty<T>> getProperties()
			{
				return typeof(T).GetRuntimeProperties().Where(isPublicNonStaticProperty).Select(CreateDtoProperty)
					.Concat(typeof(T).GetRuntimeFields().Where(isPublicNonStaticField).Select(CreateDtoProperty));
			}
		}

		private static IDtoProperty<T> CreateDtoProperty(PropertyInfo propertyInfo)
		{
			return (IDtoProperty<T>) typeof(DtoProperty<,>)
				.MakeGenericType(propertyInfo.DeclaringType, propertyInfo.PropertyType)
				.GetTypeInfo()
				.DeclaredConstructors
				.Single(c => c.GetParameters().Select(p => p.ParameterType).SequenceEqual(new[] { typeof(PropertyInfo) }))
				.Invoke(new object[] { propertyInfo });
		}

		private static IDtoProperty<T> CreateDtoProperty(FieldInfo fieldInfo)
		{
			return (IDtoProperty<T>) typeof(DtoProperty<,>)
				.MakeGenericType(fieldInfo.DeclaringType, fieldInfo.FieldType)
				.GetTypeInfo()
				.DeclaredConstructors
				.Single(c => c.GetParameters().Select(p => p.ParameterType).SequenceEqual(new[] { typeof(FieldInfo) }))
				.Invoke(new object[] { fieldInfo });
		}

		private static string GetPropertyName<TValue>(Expression<Func<T, TValue>> getter)
			=> getter.Body is MemberExpression body ? body.Member.Name : throw new ArgumentException("Invalid getter.", nameof(getter));

		internal static readonly Lazy<DtoInfo<T>> Instance = new Lazy<DtoInfo<T>>(() => new DtoInfo<T>());

		private readonly Lazy<Func<T>> m_lazyCreateNew;
		private readonly Lazy<IReadOnlyList<IDtoProperty<T>>> m_lazyProperties;
		private readonly Lazy<IReadOnlyDictionary<string, IDtoProperty<T>>> m_lazyPropertiesByName;
	}
}
