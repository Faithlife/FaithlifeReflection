using System.Linq.Expressions;
using System.Reflection;

namespace Faithlife.Reflection;

/// <summary>
/// Represents a property or field of a DTO.
/// </summary>
/// <typeparam name="TSource">The type of DTO.</typeparam>
/// <typeparam name="TValue">The value type of the property or field.</typeparam>
public sealed class DtoProperty<TSource, TValue> : IDtoProperty<TSource>
{
	/// <summary>
	/// The name of the property or field.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// The value type of the property or field.
	/// </summary>
	public Type ValueType { get; }

	/// <summary>
	/// True if the property or field is read-only.
	/// </summary>
	public bool IsReadOnly { get; }

	/// <summary>
	/// The <see cref="PropertyInfo"/> or <see cref="FieldInfo"/> of the property or field.
	/// </summary>
	public MemberInfo MemberInfo { get; }

	/// <summary>
	/// Gets the value of the property or field for the specified instance of the DTO.
	/// </summary>
	/// <param name="source">The DTO instance.</param>
	public TValue? GetValue(TSource source) =>
		m_lazyGetter.Value(source ?? throw new ArgumentNullException(nameof(source)));

	/// <summary>
	/// Sets the value of the property or field for the specified instance of the DTO.
	/// </summary>
	/// <param name="source">The DTO instance.</param>
	/// <param name="value">The value to which to set the property or field.</param>
	/// <exception cref="InvalidOperationException">The property or field is read-only, or the DTO is a value type.</exception>
	public void SetValue(TSource source, TValue? value)
	{
		if (IsReadOnly)
			throw new InvalidOperationException($"'{Name}' of '{typeof(TSource).Name}' is read-only.");
		m_lazySetter.Value(source ?? throw new ArgumentNullException(nameof(source)), value!);
	}

	/// <summary>
	/// Gets the value of the property or field for the specified instance of the DTO.
	/// </summary>
	/// <remarks>See <see cref="GetValue"/>.</remarks>
	object? IDtoProperty.GetValue(object source) =>
		GetValue(source is TSource s ? s : throw new ArgumentException($"Source must be of type '{typeof(TSource).FullName}'.", nameof(source)));

	/// <summary>
	/// Sets the value of the property or field for the specified instance of the DTO.
	/// </summary>
	/// <remarks>See <see cref="SetValue"/>.</remarks>
	void IDtoProperty.SetValue(object source, object? value) =>
		SetValue(source is TSource s ? s : throw new ArgumentException($"Source must be of type '{typeof(TSource).FullName}'.", nameof(source)),
			(TValue) value!);

	/// <summary>
	/// Gets the value of the property or field for the specified instance of the DTO.
	/// </summary>
	/// <remarks>See <see cref="GetValue"/>.</remarks>
	object? IDtoProperty<TSource>.GetValue(TSource source) => GetValue(source);

	/// <summary>
	/// Sets the value of the property or field for the specified instance of the DTO.
	/// </summary>
	/// <remarks>See <see cref="SetValue"/>.</remarks>
	void IDtoProperty<TSource>.SetValue(TSource source, object? value) => SetValue(source, (TValue) value!);

	// called by DtoInfo.CreateDtoProperty via reflection
	internal DtoProperty(PropertyInfo propertyInfo)
	{
		Name = propertyInfo.Name;
		ValueType = propertyInfo.PropertyType;
		IsReadOnly = propertyInfo.SetMethod?.IsPublic != true;
		MemberInfo = propertyInfo;
		m_lazyGetter = new Lazy<Func<TSource, TValue>>(GeneratePropertyGetter);
		m_lazySetter = new Lazy<Action<TSource, TValue>>(GeneratePropertySetter);
	}

	// called by DtoInfo.CreateDtoProperty via reflection
	internal DtoProperty(FieldInfo fieldInfo)
	{
		Name = fieldInfo.Name;
		ValueType = fieldInfo.FieldType;
		IsReadOnly = fieldInfo.IsInitOnly;
		MemberInfo = fieldInfo;
		m_lazyGetter = new Lazy<Func<TSource, TValue>>(GenerateFieldGetter);
		m_lazySetter = new Lazy<Action<TSource, TValue>>(GenerateFieldSetter);
	}

	private Func<TSource, TValue> GeneratePropertyGetter()
	{
		var parameterExpression = Expression.Parameter(typeof(TSource), "value");
		var propertyValueExpression = Expression.Property(parameterExpression, Name);
		var expression = propertyValueExpression.Type == typeof(TValue) ? propertyValueExpression : (Expression) Expression.Convert(propertyValueExpression, typeof(TValue));
		return Expression.Lambda<Func<TSource, TValue>>(expression, parameterExpression).Compile();
	}

	private Action<TSource, TValue> GeneratePropertySetter()
	{
		if (typeof(TSource).IsValueType)
			throw new InvalidOperationException("Properties cannot be set on value types.");

		var instanceParameterExpression = Expression.Parameter(typeof(TSource));
		var parameterExpression = Expression.Parameter(typeof(TValue), Name);
		var propertyValueExpression = Expression.Property(instanceParameterExpression, Name);
		var conversionExpression = propertyValueExpression.Type == typeof(TValue) ? parameterExpression : (Expression) Expression.Convert(parameterExpression, propertyValueExpression.Type);
		return Expression.Lambda<Action<TSource, TValue>>(Expression.Assign(propertyValueExpression, conversionExpression), instanceParameterExpression, parameterExpression).Compile();
	}

	private Func<TSource, TValue> GenerateFieldGetter()
	{
		var parameterExpression = Expression.Parameter(typeof(TSource), "value");
		var fieldValueExpression = Expression.Field(parameterExpression, Name);
		var expression = fieldValueExpression.Type == typeof(TValue) ? fieldValueExpression : (Expression) Expression.Convert(fieldValueExpression, typeof(TValue));
		return Expression.Lambda<Func<TSource, TValue>>(expression, parameterExpression).Compile();
	}

	private Action<TSource, TValue> GenerateFieldSetter()
	{
		if (typeof(TSource).IsValueType)
			throw new InvalidOperationException("Properties cannot be set on value types.");

		var instanceParameterExpression = Expression.Parameter(typeof(TSource));
		var parameterExpression = Expression.Parameter(typeof(TValue), Name);
		var fieldValueExpression = Expression.Field(instanceParameterExpression, Name);
		var conversionExpression = fieldValueExpression.Type == typeof(TValue) ? parameterExpression : (Expression) Expression.Convert(parameterExpression, fieldValueExpression.Type);
		return Expression.Lambda<Action<TSource, TValue>>(Expression.Assign(fieldValueExpression, conversionExpression), instanceParameterExpression, parameterExpression).Compile();
	}

	private readonly Lazy<Func<TSource, TValue>> m_lazyGetter;
	private readonly Lazy<Action<TSource, TValue>> m_lazySetter;
}
