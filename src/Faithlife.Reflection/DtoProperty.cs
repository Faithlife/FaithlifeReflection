using System;
using System.Linq.Expressions;
using System.Reflection;

#pragma warning disable 1591

namespace Faithlife.Reflection
{
	public sealed class DtoProperty<TSource, TValue> : IDtoProperty<TSource>, IDtoProperty
	{
		public string Name { get; }

		public Type ValueType { get; }

		public bool IsReadOnly { get; }

		public MemberInfo MemberInfo { get; }

		public TValue GetValue(TSource source) => m_lazyGetter.Value(source);

		public void SetValue(TSource source, TValue value)
		{
			if (IsReadOnly)
				throw new InvalidOperationException($"'{Name}' of '{typeof(TSource).Name}' is read-only.");
			m_lazySetter.Value(source, value);
		}

		object IDtoProperty.GetValue(object source) => GetValue((TSource) source);

		void IDtoProperty.SetValue(object source, object value) => SetValue((TSource) source, (TValue) value);

		object IDtoProperty<TSource>.GetValue(TSource source) => GetValue(source);

		void IDtoProperty<TSource>.SetValue(TSource source, object value) => SetValue(source, (TValue) value);

		internal DtoProperty(PropertyInfo propertyInfo)
		{
			Name = propertyInfo.Name;
			ValueType = propertyInfo.PropertyType;
			IsReadOnly = propertyInfo.SetMethod?.IsPublic != true;
			MemberInfo = propertyInfo;
			m_lazyGetter = new Lazy<Func<TSource, TValue>>(GeneratePropertyGetter);
			m_lazySetter = new Lazy<Action<TSource, TValue>>(GeneratePropertySetter);
		}

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
			var instanceParameterExpression = Expression.Parameter(typeof(TSource));
			var parameterExpression = Expression.Parameter(typeof(TValue), Name);
			var fieldValueExpression = Expression.Field(instanceParameterExpression, Name);
			var conversionExpression = fieldValueExpression.Type == typeof(TValue) ? parameterExpression : (Expression) Expression.Convert(parameterExpression, fieldValueExpression.Type);
			return Expression.Lambda<Action<TSource, TValue>>(Expression.Assign(fieldValueExpression, conversionExpression), instanceParameterExpression, parameterExpression).Compile();
		}

		private readonly Lazy<Func<TSource, TValue>> m_lazyGetter;
		private readonly Lazy<Action<TSource, TValue>> m_lazySetter;
	}
}
