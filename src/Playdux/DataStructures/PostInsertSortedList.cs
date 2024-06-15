using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Playdux.DataStructures;

/// <summary>
/// A list in which items are inserted with a priority ordered by their priority.
/// Items with the same priority are ordered by the order in which they were added.
/// </summary>
public class PostInsertSortedList<T> : IEnumerable<T>
{
    private readonly List<KeyValuePair<int, T>> _list = [];

    /// <summary>Insert the given value into the list with the given priority.</summary>
    public void Add(int priority, T value)
    {
        var pair = KeyValuePair.Create(priority, value);
        if (_list.Count == 0)
        {
            _list.Add(pair);
            return;
        }

        // roll our own binary search that will find the index *after* the given priority
        // (maintains insertion order for same priority).
        var left = 0;
        var right = _list.Count - 1;

        while (left <= right)
        {
            var middle = (left + right) / 2;
            var middlePriority = _list[middle].Key;
            
            if (middlePriority > priority)
            {
                right = middle - 1;
            }
            else if (middlePriority < priority)
            {
                left = middle + 1;
            }
            else
            {
                // loop until we find the first element after the matching priority
                while (++left < _list.Count && _list[left].Key == priority) { }

                break;
            }
        }
        
        _list.Insert(left, pair);
    }

    /// <summary>Removes the given element from the list.</summary>
    public void Remove(T value)
    {
        var index = _list.FindIndex(pair =>
            (pair.Value is null && value is null)
            || (pair.Value?.Equals(value) ?? false)
        );
        
        _list.RemoveAt(index);
    }

    public IEnumerator<T> GetEnumerator() => _list.Select(kvp => kvp.Value).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}