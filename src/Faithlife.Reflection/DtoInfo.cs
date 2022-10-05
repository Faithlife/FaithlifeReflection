using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Faithlife.Reflection;

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

	/// <summary>
	/// Gets weakly-typed information about the specified DTO type.
	/// </summary>
	/// <param name="type">The DTO type.</param>
	public static IDtoInfo GetInfo(Type type) => s_infos.GetOrAdd(type ?? throw new ArgumentNullException(nameof(type)), DoGetInfo);

	/// <summary>
	/// Calls <c>CreateNew</c> with the specified property values.
	/// </summary>
	[return: NotNull]
	public static T CreateNew<T>(this DtoInfo<T> info, params (IDtoProperty<T> Property, object? Value)[] propertyValues) =>
		info.CreateNew((propertyValues ?? throw new ArgumentNullException(nameof(propertyValues))).AsEnumerable());

	/// <summary>
	/// Calls <c>CreateNew</c> with the specified property values.
	/// </summary>
	[return: NotNull]
	public static T CreateNew<T>(this DtoInfo<T> info, IEnumerable<(string PropertyName, object? Value)> propertyValues) =>
		info.CreateNew((propertyValues ?? throw new ArgumentNullException(nameof(propertyValues))).Select(x => (info.GetProperty(x.PropertyName), x.Value)));

	/// <summary>
	/// Calls <c>CreateNew</c> with the specified property values.
	/// </summary>
	[return: NotNull]
	public static T CreateNew<T>(this DtoInfo<T> info, params (string PropertyName, object? Value)[] propertyValues) =>
		info.CreateNew((propertyValues ?? throw new ArgumentNullException(nameof(propertyValues))).AsEnumerable());

	/// <summary>
	/// Calls <c>CreateNew</c> with the specified property values.
	/// </summary>
	public static object CreateNew(this IDtoInfo info, params (IDtoProperty Property, object? Value)[] propertyValues) =>
		info.CreateNew((propertyValues ?? throw new ArgumentNullException(nameof(propertyValues))).AsEnumerable());

	/// <summary>
	/// Calls <c>CreateNew</c> with the specified property values.
	/// </summary>
	public static object CreateNew(this IDtoInfo info, IEnumerable<(string PropertyName, object? Value)> propertyValues) =>
		info.CreateNew((propertyValues ?? throw new ArgumentNullException(nameof(propertyValues))).Select(x => (info.GetProperty(x.PropertyName), x.Value)));

	/// <summary>
	/// Calls <c>CreateNew</c> with the specified property values.
	/// </summary>
	public static object CreateNew(this IDtoInfo info, params (string PropertyName, object? Value)[] propertyValues) =>
		info.CreateNew((propertyValues ?? throw new ArgumentNullException(nameof(propertyValues))).AsEnumerable());

	private static IDtoInfo DoGetInfo(Type type)
	{
		try
		{
			return (IDtoInfo) s_getInfo.MakeGenericMethod(type).Invoke(null, Array.Empty<object>())!;
		}
		catch (TargetInvocationException exception) when (exception.InnerException is not null)
		{
			ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
			throw;
		}
	}

	private static readonly ConcurrentDictionary<Type, IDtoInfo> s_infos = new();
	private static readonly MethodInfo s_getInfo = typeof(DtoInfo).GetRuntimeMethod("GetInfo", Array.Empty<Type>())!;
}

/// <summary>
/// Information about a DTO type.
/// </summary>
/// <typeparam name="T">The DTO type.</typeparam>
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Same name.")]
public sealed class DtoInfo<T> : IDtoInfo
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
	public IDtoProperty<T>? TryGetProperty(string name) =>
		m_lazyPropertiesByName.Value.TryGetValue(name ?? throw new ArgumentNullException(nameof(name)), out var property) ? property : null;

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
	public DtoProperty<T, TValue>? TryGetProperty<TValue>(string name) =>
		m_lazyPropertiesByName.Value.TryGetValue(name ?? throw new ArgumentNullException(nameof(name)), out var property) ? property as DtoProperty<T, TValue> : null;

	/// <summary>
	/// Creates a new instance of the DTO.
	/// </summary>
	[return: NotNull]
	public T CreateNew() => DoCreateNew(Array.Empty<(IDtoProperty<T>, object?)>());

	/// <summary>
	/// Clones the specified DTO by copying each property into a new instance.
	/// </summary>
	/// <param name="value">The instance to clone.</param>
	[return: NotNull]
	public T ShallowClone(T value)
	{
		if (value is null)
			throw new ArgumentNullException(nameof(value));

		return CreateNew(Properties.Select(x => (x, x.GetValue(value))));
	}

	/// <summary>
	/// Creates a new instance of the DTO.
	/// </summary>
	/// <remarks>If possible, the instance is created with the public default constructor, after which the specified
	/// properties (if any) are set to the specified values. If there is no public default constructor and/or one or more
	/// of the specified properties are read-only, the instance is created with a public constructor whose parameters
	/// match the properties of the DTO.</remarks>
	[return: NotNull]
	public T CreateNew(IEnumerable<(IDtoProperty<T> Property, object? Value)> propertyValues) =>
		DoCreateNew(propertyValues as IReadOnlyCollection<(IDtoProperty<T>, object?)> ?? (propertyValues ?? throw new ArgumentNullException(nameof(propertyValues))).ToList());

	[return: NotNull]
	private T DoCreateNew(IReadOnlyCollection<(IDtoProperty<T> Property, object? Value)> propertyValues)
	{
		// find the constructor with the fewest parameters that works with the specified property values
		foreach (var creator in m_lazyCreators.Value)
		{
			if (creator is null)
			{
				// use the default constructor if all property values can be set
				if (propertyValues.Count == 0 || (!m_isValueType && propertyValues.All(x => !x.Property.IsReadOnly)))
				{
					var newValue = m_lazyCreateNew.Value()!;
					foreach (var (property, value) in propertyValues)
						property.SetValue(newValue, value);
					return newValue;
				}
			}
			else
			{
				var parameters = new object?[creator.Properties.Length];

				// use the default values for the constructor parameters, if any
				if (creator.DefaultValues is not null)
					Array.Copy(creator.DefaultValues, parameters, parameters.Length);

				var canCreate = true;
				List<(IDtoProperty<T> Property, object? Value)>? propertyValuesToSet = null;

				foreach (var (property, value) in propertyValues)
				{
					if (creator.GetPropertyParameterIndex(property) is { } index)
					{
						parameters[index] = value;
					}
					else if (!m_isValueType && !property.IsReadOnly)
					{
						(propertyValuesToSet ??= new List<(IDtoProperty<T> Property, object? Value)>(capacity: propertyValues.Count)).Add((property, value));
					}
					else
					{
						canCreate = false;
						break;
					}
				}

				if (canCreate)
				{
					var newValue = (T) creator.Constructor.Invoke(parameters);
					if (propertyValuesToSet is not null)
					{
						foreach (var (property, value) in propertyValuesToSet)
							property.SetValue(newValue, value);
					}
					return newValue!;
				}
			}
		}

		throw new InvalidOperationException("No matching constructors found.");
	}

	IReadOnlyList<IDtoProperty> IDtoInfo.Properties => Properties;

	IDtoProperty IDtoInfo.GetProperty(string name) => GetProperty(name);

	IDtoProperty? IDtoInfo.TryGetProperty(string name) => TryGetProperty(name);

	object IDtoInfo.CreateNew() => CreateNew();

	object IDtoInfo.ShallowClone(object value) =>
		ShallowClone(value is T t ? t : throw new ArgumentException($"Value must be of type '{typeof(T).FullName}'.", nameof(value)));

	object IDtoInfo.CreateNew(IEnumerable<(IDtoProperty Property, object? Value)> propertyValues)
	{
		return CreateNew(Cast(propertyValues ?? throw new ArgumentNullException(nameof(propertyValues))));

		static IEnumerable<(IDtoProperty<T> Property, object? Value)> Cast(IEnumerable<(IDtoProperty Property, object? Value)> source)
		{
			foreach (var (property, value) in source)
			{
				if (!(property is IDtoProperty<T> typedProperty))
					throw new InvalidOperationException($"Property '{property.Name}' must be from type '{typeof(T).FullName}'.");
				yield return (typedProperty, value);
			}
		}
	}

	internal DtoInfo()
	{
		m_lazyCreateNew = new Lazy<Func<T>>(() => Expression.Lambda<Func<T>>(Expression.New(typeof(T))).Compile());

		m_lazyProperties = new Lazy<IReadOnlyList<IDtoProperty<T>>>(
			() => new ReadOnlyCollection<IDtoProperty<T>>(GetProperties().ToArray()));

		m_lazyPropertiesByName = new Lazy<IReadOnlyDictionary<string, IDtoProperty<T>>>(
			() => m_lazyProperties.Value.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase));

		m_lazyCreators = new Lazy<Creator?[]>(
			() => GetCreators().OrderBy(x => x?.Properties.Length ?? 0).ToArray());

		m_isValueType = typeof(T).IsValueType;

		static bool IsPublicNonStaticProperty(PropertyInfo info) => info.GetMethod is not null && info.GetMethod.IsPublic && !info.GetMethod.IsStatic;

		static bool IsPublicNonStaticField(FieldInfo info) => info.IsPublic && !info.IsStatic;

		static IEnumerable<IDtoProperty<T>> GetProperties()
		{
			var type = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
			return type.GetRuntimeProperties().Where(IsPublicNonStaticProperty).Select(CreateDtoProperty)
				.Concat(type.GetRuntimeFields().Where(IsPublicNonStaticField).Select(CreateDtoProperty));
		}
	}

	private static IDtoProperty<T> CreateDtoProperty(PropertyInfo propertyInfo) =>
		(IDtoProperty<T>) typeof(DtoProperty<,>)
			.MakeGenericType(typeof(T), propertyInfo.PropertyType)
			.GetTypeInfo()
			.DeclaredConstructors
			.Single(c => c.GetParameters().Select(p => p.ParameterType).SequenceEqual(new[] { typeof(PropertyInfo) }))
			.Invoke(new object[] { propertyInfo });

	private static IDtoProperty<T> CreateDtoProperty(FieldInfo fieldInfo) =>
		(IDtoProperty<T>) typeof(DtoProperty<,>)
			.MakeGenericType(typeof(T), fieldInfo.FieldType)
			.GetTypeInfo()
			.DeclaredConstructors
			.Single(c => c.GetParameters().Select(p => p.ParameterType).SequenceEqual(new[] { typeof(FieldInfo) }))
			.Invoke(new object[] { fieldInfo });

	private static string GetPropertyName<TValue>(Expression<Func<T, TValue>> getter) =>
		getter.Body is MemberExpression body ? body.Member.Name : throw new ArgumentException("Invalid getter.", nameof(getter));

	private IEnumerable<Creator?> GetCreators()
	{
		foreach (var constructor in (Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T)).GetConstructors())
		{
			var parameters = constructor.GetParameters();
			if (parameters.Length == 0)
			{
				// null means default constructor
				yield return null;
			}
			else
			{
				var isCreator = true;
				var properties = new IDtoProperty<T>[parameters.Length];
				object?[]? defaultValues = null;
				for (var index = 0; index < parameters.Length; index++)
				{
					var parameter = parameters[index];
					var property = parameter.Name is { } name ? TryGetProperty(name) : null;
					if (property is null)
					{
						isCreator = false;
						break;
					}
					properties[index] = property;
					if (parameter.HasDefaultValue)
						(defaultValues ??= new object?[parameters.Length])[index] = parameter.DefaultValue;
				}

				if (isCreator)
					yield return new Creator(constructor, properties, defaultValues);
			}
		}
	}

	private sealed class Creator
	{
		public Creator(ConstructorInfo constructor, IDtoProperty<T>[] properties, object?[]? defaultValues)
		{
			Constructor = constructor;
			Properties = properties;
			DefaultValues = defaultValues;

			m_propertyIndices = new Dictionary<IDtoProperty<T>, int>();
			for (var index = 0; index < properties.Length; index++)
				m_propertyIndices.Add(properties[index], index);
		}

		public ConstructorInfo Constructor { get; }

		public IDtoProperty<T>[] Properties { get; }

		public object?[]? DefaultValues { get; }

		public int? GetPropertyParameterIndex(IDtoProperty<T> property) =>
			m_propertyIndices.TryGetValue(property, out var index) ? index : default(int?);

		private readonly Dictionary<IDtoProperty<T>, int> m_propertyIndices;
	}

	internal static readonly Lazy<DtoInfo<T>> Instance = new(() => new DtoInfo<T>());

	private readonly Lazy<Func<T>> m_lazyCreateNew;
	private readonly Lazy<IReadOnlyList<IDtoProperty<T>>> m_lazyProperties;
	private readonly Lazy<IReadOnlyDictionary<string, IDtoProperty<T>>> m_lazyPropertiesByName;
	private readonly Lazy<Creator?[]> m_lazyCreators;
	private readonly bool m_isValueType;
}
