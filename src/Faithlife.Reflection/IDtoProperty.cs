using System;
using System.Reflection;

#pragma warning disable 1591

namespace Faithlife.Reflection
{
	public interface IDtoProperty
	{
		string Name { get; }

		Type ValueType { get; }

		bool IsReadOnly { get; }

		MemberInfo MemberInfo { get; }

		object GetValue(object source);

		void SetValue(object source, object value);
	}

	public interface IDtoProperty<in T>
	{
		string Name { get; }

		Type ValueType { get; }

		bool IsReadOnly { get; }

		MemberInfo MemberInfo { get; }

		object GetValue(T dto);

		void SetValue(T dto, object value);
	}
}
