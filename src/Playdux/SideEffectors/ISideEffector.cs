using System;
using System.Collections.Generic;
using Playdux.Actions;
using Playdux.Store;

namespace Playdux.SideEffectors;

public interface ISideEffector<in TRootState>
    where TRootState : class, IEquatable<TRootState>
{
    /// Determines the order in which SideEffectors will be executed.
    /// Higher priorities will run first.
    public int Priority { get; }

    /// Execute a side effect before the action is sent to the reducer.
    public bool PreEffect(DispatchedAction dispatchedAction, IStore<TRootState> store);

    /// Execute a side effect after the action has been sent to the reducer.
    public void PostEffect(DispatchedAction dispatchedAction, IStore<TRootState> store);
}

internal class SideEffectorPriorityComparer<TRootState> : IComparer<ISideEffector<TRootState>>
    where TRootState : class, IEquatable<TRootState>
{
    public int Compare(ISideEffector<TRootState> x, ISideEffector<TRootState> y) => y.Priority.CompareTo(x.Priority);
}