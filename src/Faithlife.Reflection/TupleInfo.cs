using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
		/// Gets strongly-typed information for the specified tuple type.
		/// </summary>
		/// <typeparam name="T">The tuple type.</typeparam>
		public static TupleInfo<T> GetInfo<T>() => TupleInfo<T>.Instance.Value;

		/// <summary>
		/// Gets weakly-typed information for the specified tuple type.
		/// </summary>
		/// <param name="type">The tuple type.</param>
		public static ITupleInfo GetInfo(Type type) => s_infos.GetOrAdd(type, DoGetInfo);

		private static ITupleInfo DoGetInfo(Type type)
		{
			try
			{
				return (ITupleInfo) s_getInfo.MakeGenericMethod(type).Invoke(null, new object[0]);
			}
			catch (TargetInvocationException exception) when (exception.InnerException != null)
			{
				ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
				throw;
			}
		}

		private static readonly ConcurrentDictionary<Type, ITupleInfo> s_infos = new ConcurrentDictionary<Type, ITupleInfo>();
		private static readonly MethodInfo s_getInfo = typeof(TupleInfo).GetRuntimeMethod("GetInfo", new Type[0]);
	}

	/// <summary>
	/// Strongly-typed information about a tuple type.
	/// </summary>
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
		public T CreateNew(IEnumerable<object> items) => m_lazyCreator.Value(items);

		/// <summary>
		/// Creates a tuple from the specified items.
		/// </summary>
		object ITupleInfo.CreateNew(IEnumerable<object> items) => CreateNew(items);

		internal TupleInfo()
		{
			string typeName = typeof(T).FullName;
			if (typeName == null ||
				!typeName.StartsWith("System.ValueTuple`", StringComparison.Ordinal) &&
				!typeName.StartsWith("System.Tuple`", StringComparison.Ordinal) &&
				typeName != "System.ValueTuple")
			{
				throw new InvalidOperationException($"Type is not a tuple: {typeName}");
			}

			var genericTypeArguments = typeof(T).GenericTypeArguments;
			ItemTypes = genericTypeArguments.Length < 8 ?
				new ReadOnlyCollection<Type>(genericTypeArguments) :
				new ReadOnlyCollection<Type>(genericTypeArguments.Take(7).Concat(TupleInfo.GetInfo(genericTypeArguments[7]).ItemTypes).ToList());

			m_lazyCreator = new Lazy<Func<IEnumerable<object>, T>>(GetCreator);
		}

		internal static readonly Lazy<TupleInfo<T>> Instance = new Lazy<TupleInfo<T>>(() => new TupleInfo<T>());

		private Func<IEnumerable<object>, T> GetCreator()
		{
			var type = typeof(T);
			int itemCount = ItemTypes.Count;
			if (itemCount == 0)
				return items => (T) (object) ValueTuple.Create();

			int genericTypeCount = Math.Min(8, itemCount);
			var constructor = type.GetTypeInfo().DeclaredConstructors.Single(x => x.GetParameters().Length == genericTypeCount);
			var restInfo = genericTypeCount < 8 ? null : TupleInfo.GetInfo(type.GenericTypeArguments[7]);
			return items =>
			{
				var arguments = new object[genericTypeCount];
				var restItems = new object[Math.Max(0, itemCount - genericTypeCount + 1)];
				int index = 0;
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
				if (restInfo != null)
					arguments[7] = restInfo.CreateNew(restItems);
				return (T) constructor.Invoke(arguments);
			};
		}

		private readonly Lazy<Func<IEnumerable<object>, T>> m_lazyCreator;
	}
}
