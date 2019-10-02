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
* create a new instance of the DTO
* shallow clone an existing instance of the DTO

Anonymous types and other read-only DTO types also work well with this class library for enumerating and getting property values, but setting property values, creating new instances, etc., will fail at runtime.

### Accessing info

The easiest way to access the DTO info is to call [`DtoInfo.GetInfo`](Faithlife.Reflection/DtoInfo/GetInfo.md) with the `Type` of the DTO.

```csharp
int GetPropertyCount(object dto)
{
    IDtoInfo info = DtoInfo.GetInfo(dto.GetType());
    return info.Properties.Count;
}
```

If possible, use the generic overload of `DtoInfo.GetInfo`, which is slightly more efficient and supports stronger types.

```csharp
int GetPropertyCount<T>()
{
    DtoInfo<T> info = DtoInfo.GetInfo<T>();
    return info.Properties.Count;
}
```

### Getting properties

This method creates a dictionary of property names and values from an arbitrary DTO instance:

```csharp
IReadOnlyDictionary<string, object> ConvertDtoToDictionary(object dto) =>
    DtoInfo.GetInfo(dto.GetType()).Properties.ToDictionary(x => x.Name, x => x.GetValue(dto));
```

### Setting properties

This method creates a new instance of the specified type and sets its `Id` property to the specified string:

```csharp
T CreateWithId<T>(string id)
{
    var info = DtoInfo.GetInfo<T>();
    var dto = info.CreateNew();
    info.GetProperty<string>("Id").SetValue(dto, id);
    return dto;
}
```

## Tuples

C# 7 introduced syntax for tuples, which use the `System.ValueTuple<...>` types. Before that, the `System.Tuple<...>` types were commonly used. This library supports both kinds of tuples.

The [`TupleInfo`](Faithlife.Reflection/TupleInfo.md) static class makes it easy to:

* determine if a type is a tuple type
* enumerate the types of the tuple items (even for tuples with more than seven items)
* create a tuple from a list of objects
