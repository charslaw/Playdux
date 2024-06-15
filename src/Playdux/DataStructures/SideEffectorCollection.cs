using System;
using System.Collections.Generic;
using Playdux.SideEffectors;

namespace Playdux.DataStructures;

public class SideEffectorCollection<TRootState>
    where TRootState : class, IEquatable<TRootState>
{
    private readonly Dictionary<Guid, ISideEffector<TRootState>> _table = new();
    private readonly List<ISideEffector<TRootState>> _priority = new();

    private readonly IComparer<ISideEffector<TRootState>> _comparer = new SideEffectorPriorityComparer<TRootState>();

    public IEnumerable<ISideEffector<TRootState>> ByPriority => _priority;

    public Guid Register(ISideEffector<TRootState> sideEffector)
    {
            var id = Guid.NewGuid();
            _table.Add(id, sideEffector);

            var index = _priority.BinarySearch(sideEffector, _comparer);

            if (index < 0)
            {
                // a side effector with the same priority was not found, so this one should be inserted at the bitwise complement of the returned index.
                // See <https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1.BinarySearch>
                index = ~index;
            }
            else
            {
                // a side effector with the same priority already exists in the list, insert this one after the existing one.
                while (++index < _priority.Count && _priority[index].Priority == sideEffector.Priority) { }
            }

            _priority.Insert(index, sideEffector);

            return id;
        }

    public void Unregister(Guid id)
    {
            if (!_table.TryGetValue(id, out var sideEffector)) throw new ArgumentException("Given ID does not correspond with a known side effector", nameof(id));
            _table.Remove(id);
            
            var index = _priority.FindIndex(other => other == sideEffector);

            _priority.RemoveAt(index);
        }
}