using System;
using System.Collections.Generic;
using Vacuum.Data.Collections;

namespace Vacuum.Data.Operations;

/// <summary>
/// Generic CRUD operations for in-memory data collections.
/// </summary>
public class CrudOperations<T> where T : class
{
    private readonly DataCollection<T> _collection;
    private readonly Func<T, string> _idSelector;

    public CrudOperations(Func<T, string> idSelector)
    {
        _idSelector = idSelector;
        _collection = new DataCollection<T>(idSelector);
    }

    public T Create(T item)
    {
        _collection.Add(item);
        return item;
    }

    public T? Read(string id) => _collection.GetById(id);

    public IReadOnlyCollection<T> ReadAll() => _collection.GetAll();

    public T Update(T item)
    {
        _collection.Add(item); // overwrites by ID
        return item;
    }

    public bool Delete(string id) => _collection.Remove(id);

    public List<T> Query(Func<T, bool> predicate) => _collection.Where(predicate);

    public int Count => _collection.Count;
}
