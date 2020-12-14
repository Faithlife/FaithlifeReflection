using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Faithlife.Reflection
{
	/// <summary>
	/// Helpers for using reflection with tuples.
	/// </summary>
	/// <remarks>This class works with both <c>ValueTuple</c> and <c>Tuple</c>.</remarks>
	public static class TupleInfo
	{
		/// <summary>
		/// Gets information for the specified tuple type.
		/// </summary>
		/// <typeparam name="T">The tuple type.</typeparam>
		public static TupleInfo<T> GetInfo<T>() => TupleInfo<T>.Instance.Value;

		/// <summary>
		/// Gets weakly-typed information for the specified tuple type.
		/// </summary>
		/// <param name="type">The tuple type.</param>
		public static ITupleInfo GetInfo(Type type) => s_infos.GetOrAdd(type, DoGetInfo);

		/// <summary>
		/// True if the specified object is a supported tuple.
		/// </summary>
		/// <param name="value">The possible tuple.</param>
		public static bool IsTuple(object? value) => value is not null && IsTupleType(value.GetType());

		/// <summary>
		/// True if the specified type is a supported tuple type.
		/// </summary>
		/// <param name="type">The possible tuple type.</param>
		public static bool IsTupleType(Type? type)
		{
			var typeName = type?.FullName;
			return typeName is not null &&
				(typeName.StartsWith("System.ValueTuple`", StringComparison.Ordinal) ||
					typeName.StartsWith("System.Tuple`", StringComparison.Ordinal) ||
					typeName == "System.ValueTuple" ||
					IsTupleType(Nullable.GetUnderlyingType(type!)));
		}

		private static ITupleInfo DoGetInfo(Type type)
		{
			try
			{
				return (ITupleInfo) s_getInfo.MakeGenericMethod(type).Invoke(null, Array.Empty<object>());
			}
			catch (TargetInvocationException exception) when (exception.InnerException is not null)
			{
				ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
				throw;
			}
		}

		private static readonly ConcurrentDictionary<Type, ITupleInfo> s_infos = new();
		private static readonly MethodInfo s_getInfo = typeof(TupleInfo).GetRuntimeMethod("GetInfo", Array.Empty<Type>());
	}

	/// <summary>
	/// Information about a tuple type.
	/// </summary>
	[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Same name.")]
	public sealed class TupleInfo<T> : ITupleInfo
	{
		/// <summary>
		/// The type of tuple.
		/// </summary>
		public Type TupleType => typeof(T);

		/// <summary>
		/// The types of items in the tuple.
		/// </summary>
		public IReadOnlyList<Type> ItemTypes { get; }

		/// <summary>
		/// Creates a tuple from the specified items.
		/// </summary>
		[return: NotNull]
		public T CreateNew(IEnumerable<object?> items) => m_lazyCreator.Value(items)!;

		/// <summary>
		/// Creates a tuple from the specified items.
		/// </summary>
		object ITupleInfo.CreateNew(IEnumerable<object?> items) => CreateNew(items);

		internal TupleInfo()
		{
			var type = typeof(T);
			if (!TupleInfo.IsTupleType(type))
				throw new InvalidOperationException($"Type is not a tuple: {type.FullName}");

			var genericTypeArguments = (Nullable.GetUnderlyingType(type) ?? type).GenericTypeArguments;
			ItemTypes = genericTypeArguments.Length < 8 ?
				new ReadOnlyCollection<Type>(genericTypeArguments) :
				new ReadOnlyCollection<Type>(genericTypeArguments.Take(7).Concat(TupleInfo.GetInfo(genericTypeArguments[7]).ItemTypes).ToList());

			m_lazyCreator = new Lazy<Func<IEnumerable<object?>, T>>(GetCreator);
		}

		internal static readonly Lazy<TupleInfo<T>> Instance = new(() => new TupleInfo<T>());

		private Func<IEnumerable<object?>, T> GetCreator()
		{
			var type = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
			var itemCount = ItemTypes.Count;
			if (itemCount == 0)
				return _ => default!;

			var genericTypeCount = Math.Min(8, itemCount);
			var constructor = type.GetTypeInfo().DeclaredConstructors.Single(x => x.GetParameters().Length == genericTypeCount);
			var restInfo = genericTypeCount < 8 ? null : TupleInfo.GetInfo(type.GenericTypeArguments[7]);
			return items =>
			{
				var arguments = new object?[genericTypeCount];
				var restItems = new object?[Math.Max(0, itemCount - genericTypeCount + 1)];
				var index = 0;
				foreach (var item in items)
				{
					if (index == itemCount)
						throw new ArgumentException("Too many items.");
					if (index < 7)
						arguments[index] = item;
					else
						restItems[index - 7] = item;
					index++;
				}
				if (index != itemCount)
					throw new ArgumentException("Too few items.");
				if (restInfo is not null)
					arguments[7] = restInfo.CreateNew(restItems);
				return (T) constructor.Invoke(arguments);
			};
		}

		private readonly Lazy<Func<IEnumerable<object?>, T>> m_lazyCreator;
	}
}
