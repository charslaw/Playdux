using System;
using Playdux.Actions;
using Playdux.Store;

namespace Playdux.SideEffectors;

public interface IPreSideEffector<TRootState> where TRootState : class, IEquatable<TRootState>
{
    /// Execute a side effect before the action is sent to the reducer.
    public bool PreEffect(DispatchedAction<TRootState> dispatchedAction, IStore<TRootState> store);
}

public interface IPostSideEffector<TRootState>
    where TRootState : class, IEquatable<TRootState>
{
    /// Execute a side effect after the action has been sent to the reducer.
    public void PostEffect(DispatchedAction<TRootState> dispatchedAction, IStore<TRootState> store);
}

public interface ISideEffector<TRootState>: IPreSideEffector<TRootState>, IPostSideEffector<TRootState>
    where TRootState : class, IEquatable<TRootState>;