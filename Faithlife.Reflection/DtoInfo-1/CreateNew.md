# DtoInfo&lt;T&gt;.CreateNew method (1 of 2)

Creates a new instance of the DTO.

```csharp
public T CreateNew()
```

## See Also

* class [DtoInfo&lt;T&gt;](../DtoInfo-1.md)
* namespace [Faithlife.Reflection](../../Faithlife.Reflection.md)

---

# DtoInfo&lt;T&gt;.CreateNew method (2 of 2)

Creates a new instance of the DTO.

```csharp
public T CreateNew(IEnumerable<(IDtoProperty<T> Property, object? Value)> propertyValues)
```

## Remarks

If possible, the instance is created with the public default constructor, after which the specified properties (if any) are set to the specified values. If there is no public default constructor and/or one or more of the specified properties are read-only, the instance is created with a public constructor whose parameters match the properties of the DTO.

## See Also

* interface [IDtoProperty&lt;T&gt;](../IDtoProperty-1.md)
* class [DtoInfo&lt;T&gt;](../DtoInfo-1.md)
* namespace [Faithlife.Reflection](../../Faithlife.Reflection.md)

<!-- DO NOT EDIT: generated by xmldocmd for Faithlife.Reflection.dll -->