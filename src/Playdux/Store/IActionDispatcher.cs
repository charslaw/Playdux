using System;
using Playdux.Actions;
using Playdux.SideEffectors;

namespace Playdux.Store;

public interface IActionDispatcher<TRootState>
    where TRootState : class, IEquatable<TRootState>
{
    /// <summary>
    /// Dispatch an action to the Store. Changes store state according to the reducer provided at creation.
    /// </summary>
    /// <remarks>Actions will be consumed in FIFO order.</remarks>
    void Dispatch(IAction<TRootState> action);

    /// <summary>Registers a new pre side effector to observe this Store with the given priority.</summary>
    void RegisterSideEffector(int priority, IPreSideEffector<TRootState> sideEffector);
    
    /// <summary>Registers a new post side effector to observe this Store with the given priority.</summary>
    void RegisterSideEffector(int priority, IPostSideEffector<TRootState> sideEffector);
    
    /// <summary>Registers a new side effector to observe this Store with the given priority.</summary>
    void RegisterSideEffector(int priority, ISideEffector<TRootState> sideEffector);
    
    /// <summary>Registers a new pre side effector to observe this Store with the default priority.</summary>
    void RegisterSideEffector(IPreSideEffector<TRootState> sideEffector);
    
    /// <summary>Registers a new post side effector to observe this Store with the default priority.</summary>
    void RegisterSideEffector(IPostSideEffector<TRootState> sideEffector);
    
    /// <summary>Registers a new side effector to observe this Store with the default priority.</summary>
    void RegisterSideEffector(ISideEffector<TRootState> sideEffector);

    /// <summary>Unregisters a previously registered pre side effector.</summary>
    void UnregisterSideEffector(IPreSideEffector<TRootState> sideEffector);

    /// <summary>Unregisters a previously registered post side effector.</summary>
    void UnregisterSideEffector(IPostSideEffector<TRootState> sideEffector);

    /// <summary>Unregisters a previously registered side effector.</summary>
    void UnregisterSideEffector(ISideEffector<TRootState> sideEffector);
}