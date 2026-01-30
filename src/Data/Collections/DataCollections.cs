using System;
using System.Collections.Generic;
using System.Linq;

namespace Vacuum.Data.Collections;

/// <summary>
/// Generic in-memory collection with Dictionary-based indexing for fast lookups.
/// </summary>
public class DataCollection<T> where T : class
{
    private readonly Dictionary<string, T> _items = new();
    private readonly Func<T, string> _idSelector;

    public DataCollection(Func<T, string> idSelector)
    {
        _idSelector = idSelector;
    }

    public int Count => _items.Count;

    public void Add(T item)
    {
        _items[_idSelector(item)] = item;
    }

    public void AddRange(IEnumerable<T> items)
    {
        foreach (var item in items)
            Add(item);
    }

    public T? GetById(string id)
    {
        return _items.TryGetValue(id, out var item) ? item : null;
    }

    public bool Remove(string id) => _items.Remove(id);

    public bool Contains(string id) => _items.ContainsKey(id);

    public IReadOnlyCollection<T> GetAll() => _items.Values;

    public List<T> Where(Func<T, bool> predicate) => _items.Values.Where(predicate).ToList();

    public T? FirstOrDefault(Func<T, bool> predicate) => _items.Values.FirstOrDefault(predicate);

    public void Clear() => _items.Clear();
}
