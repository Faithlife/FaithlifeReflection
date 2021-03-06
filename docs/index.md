# Faithlife.Reflection

**Faithlife.Reflection** provides helpers for reflecting over .NET types.

[![NuGet](https://img.shields.io/nuget/v/Faithlife.Reflection.svg)](https://www.nuget.org/packages/Faithlife.Reflection)

## Overview

The **Faithlife.Reflection** class library makes reflecting over .NET types more intuitive and more efficient compared to using raw .NET reflection.

For now, the focus is on [DTOs](#dtos) and [tuples](#tuples).

Consult the [reference documentation](Faithlife.Reflection.md) for specific details.

## DTOs

A DTO (data transfer object) typically has one or more read/write properties and a default constructor.

The [`DtoInfo`](Faithlife.Reflection/DtoInfo.md) static class makes it easy to:

* enumerate the public non-static properties and fields of a DTO type, both read/write and read-only
* get the name, type, and other metadata for each property/field of the DTO type
* get or set the value of a property/field of an instance of the DTO
* create a new instance of the DTO, using constructor arguments if needed to initialize read-only properties
* shallow clone an existing instance of the DTO

Anonymous types, value types, and other DTO types with read-only properties are also supported. Enumerating properties and getting property values will work fine, but other operations like setting properties will fail at runtime.

### Accessing DTO info

To access the information for a DTO, call [`DtoInfo.GetInfo`](Faithlife.Reflection/DtoInfo/GetInfo.md). The generic overload, `DtoInfo.GetInfo<T>()`, is slightly more efficient and returns information with stronger types than the non-generic overload, `DtoInfo.GetInfo(Type)`.

### Getting properties

The following method creates a dictionary of property names and values from an arbitrary DTO instance. [(Try it!)](https://dotnetfiddle.net/27H2j1)

```csharp
static IReadOnlyDictionary<string, object> ConvertDtoToDictionary(object dto) =>
    DtoInfo.GetInfo(dto.GetType()).Properties.ToDictionary(x => x.Name, x => x.GetValue(dto));
```

For example, `ConvertDtoToDictionary(new { one = 1, two = "II" })` returns `new Dictionary<string, object> { ["one"] = 1, ["two"] = "II" }`.

### Setting properties

The following method creates a new instance of the specified type and sets its `Id` property to the specified string. [(Try it!)](https://dotnetfiddle.net/mB0jFF)

```csharp
static T CreateWithId<T>(string id)
{
    var dtoInfo = DtoInfo.GetInfo<T>();
    var dto = dtoInfo.CreateNew();
    dtoInfo.GetProperty<string>("Id").SetValue(dto, id);
    return dto;
}
```

For example, `CreateWithId<Widget>("xyzzy")` returns `new Widget { Id = "xyzzy" }`.

Better yet, pass tuples of property names and values to automatically initialize properties, even if they are read-only. [(Try it!)](https://dotnetfiddle.net/5TJSiB)

```csharp
static T CreateWithId<T>(string id) => DtoInfo.GetInfo<T>().CreateNew(("Id", id));
```

## Tuples

C# 7 introduced syntax for tuples, which use the `System.ValueTuple<...>` types. Before that, the `System.Tuple<...>` types were commonly used. This library supports both kinds of tuples.

The [`TupleInfo`](Faithlife.Reflection/TupleInfo.md) static class makes it easy to:

* determine if a type is a tuple type
* enumerate the types of the tuple items (even for tuples with more than seven items)
* create a tuple from a list of objects

### Accessing tuple info

To access the information for a tuple, call [`TupleInfo.GetInfo`](Faithlife.Reflection/TupleInfo/GetInfo.md). The generic overload, `TupleInfo.GetInfo<T>()`, is slightly more efficient and returns information with stronger types than the non-generic overload, `TupleInfo.GetInfo(Type)`.

### Creating a tuple

The following method splits a string and converts the substrings to items of the specified tuple. [(Try it!)](https://dotnetfiddle.net/TBqxXW)

```csharp
static T SplitString<T>(string text, char delim)
{
    var tupleInfo = TupleInfo.GetInfo<T>();
    var items = new object[tupleInfo.ItemTypes.Count];
    var strings = text.Split(delim, tupleInfo.ItemTypes.Count);
    for (int i = 0; i < strings.Length; i++)
    {
        var itemType = tupleInfo.ItemTypes[i];
        itemType = Nullable.GetUnderlyingType(itemType) ?? itemType;
        items[i] = Convert.ChangeType(strings[i], itemType, CultureInfo.InvariantCulture);
    }
    return tupleInfo.CreateNew(items);
}
```

For example, `SplitString<(bool, string, int?)>("true,hey,2", ',')` returns the tuple `(true, "hey", 2)`.
