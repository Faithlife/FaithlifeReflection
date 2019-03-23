using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

#pragma warning disable 1591

namespace Faithlife.Reflection
{
	public static class DtoInfo
	{
		public static DtoInfo<T> GetInfo<T>() => DtoInfo<T>.Instance.Value;
	}

	public sealed class DtoInfo<T>
	{
		public IReadOnlyList<IDtoProperty<T>> Properties => m_lazyProperties.Value;

		public IDtoProperty<T> GetProperty(string name) =>
			TryGetProperty(name) ?? throw new ArgumentException($"Type '{typeof(T).FullName}' does not have a public non-static property or field named '{name}'.");

		public IDtoProperty<T> TryGetProperty(string name) =>
			m_lazyPropertiesByName.Value.TryGetValue(name, out var property) ? property : null;

		public DtoProperty<T, TValue> GetProperty<TValue>(Expression<Func<T, TValue>> getter) => GetProperty<TValue>(GetPropertyName(getter));

		public DtoProperty<T, TValue> GetProperty<TValue>(string name) =>
			TryGetProperty<TValue>(name) ?? throw new ArgumentException($"Type '{typeof(T).FullName}' does not have a public non-static property or field named '{name}' of type '{typeof(TValue).FullName}'.");

		public DtoProperty<T, TValue> TryGetProperty<TValue>(string name) =>
			m_lazyPropertiesByName.Value.TryGetValue(name, out var property) ? property as DtoProperty<T, TValue> : null;

		public T CreateNew() => m_lazyCreateNew.Value();

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
