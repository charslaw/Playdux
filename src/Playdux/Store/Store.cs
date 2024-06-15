using System;
using System.Collections.Generic;
using Playdux.Actions;
using Playdux.DataStructures;
using Playdux.Exceptions;
using Playdux.SideEffectors;
using Playdux.Utils;

namespace Playdux.Store;

/// <summary>
/// A Playdux state container. The core of Playdux.
/// Includes capability to dispatch actions to the store, get the current state, get the current state narrowed by
/// a selector, and get an IObservable to the "selected" state.
/// </summary>
public class Store<TRootState> : IStore<TRootState>
    where TRootState : class, IEquatable<TRootState>
{
    private TRootState _state;
    /// The current state within the store.
    public TRootState State
    {
        get => _state;
        private set
        {
            var old = _state;
            _state = value;
            if (EquatableExtensions.NullableEquals(old, _state)) return;
            OnStateChanged(old, _state);
        }
    }

    /// Reduces state according to actions dispatched to the store.
    private readonly Func<TRootState, IAction<TRootState>, TRootState> _rootReducer;

    /// Holds actions in a defined FIFO order to ensure actions are processed in the order that they are received.
    private readonly ActionQueue<TRootState> _actionQueue;

    private readonly PostInsertSortedList<IPreSideEffector<TRootState>> _preSideEffectors = new();
    private readonly PostInsertSortedList<IPostSideEffector<TRootState>> _postSideEffectors = new();
    
    /// Holds observables that have been created for ObservableFor
    private readonly List<IStateObservable<TRootState>> _observables = [];

    /// Create a new store with a given initial state and reducer
    public Store(TRootState initialState, Func<TRootState, IAction<TRootState>, TRootState> rootReducer)
    {
        _state = initialState;
        _rootReducer = rootReducer;
        _actionQueue = new ActionQueue<TRootState>(DispatchInternal);
    }

    /// <inheritdoc cref="IActionDispatcher{TRootState}.Dispatch"/>
    public void Dispatch(IAction<TRootState> action)
    {
        _actionQueue.Dispatch(new DispatchedAction<TRootState>(action));
    }

    public void RegisterSideEffector(int priority, IPreSideEffector<TRootState> sideEffector) =>
        _preSideEffectors.Add(priority, sideEffector);

    public void RegisterSideEffector(int priority, IPostSideEffector<TRootState> sideEffector) =>
        _postSideEffectors.Add(priority, sideEffector);

    public void RegisterSideEffector(int priority, ISideEffector<TRootState> sideEffector)
    {
        _preSideEffectors.Add(priority, sideEffector);
        _postSideEffectors.Add(priority, sideEffector);
    }

    public void RegisterSideEffector(IPreSideEffector<TRootState> sideEffector) =>
        RegisterSideEffector(0, sideEffector);

    public void RegisterSideEffector(IPostSideEffector<TRootState> sideEffector) =>
        RegisterSideEffector(0, sideEffector);

    public void RegisterSideEffector(ISideEffector<TRootState> sideEffector) =>
        RegisterSideEffector(0, sideEffector);

    public void UnregisterSideEffector(IPreSideEffector<TRootState> sideEffector) =>
        _preSideEffectors.Remove(sideEffector);

    public void UnregisterSideEffector(IPostSideEffector<TRootState> sideEffector) =>
        _postSideEffectors.Remove(sideEffector);

    public void UnregisterSideEffector(ISideEffector<TRootState> sideEffector)
    {
        _preSideEffectors.Remove(sideEffector);
        _postSideEffectors.Remove(sideEffector);
    }

    /// Handles a single dispatched action from the queue, activating pre effects, reducing state, updating state, then activating post effects.
    private void DispatchInternal(DispatchedAction<TRootState> dispatchedAction)
    {
        // Pre Effects
        foreach (var preEffector in _preSideEffectors)
        {
            try
            {
                var shouldAllow = preEffector.PreEffect(dispatchedAction, this);
                if (!shouldAllow) dispatchedAction = dispatchedAction with { IsCanceled = true };
            }
            catch (Exception e) { throw new SideEffectorExecutionException(SideEffectorType.Pre, e); }
        }

        if (dispatchedAction.IsCanceled) return;

        // Reduce
        var action = dispatchedAction.Action;
        var oldState = State;
        State = _rootReducer(oldState, action);

        // Post Effects
        foreach (var postEffector in _postSideEffectors)
        {
            try { postEffector.PostEffect(dispatchedAction, this); }
            catch (Exception e) { throw new SideEffectorExecutionException(SideEffectorType.Post, e); }
        }
    }

    /// <inheritdoc cref="IStateContainer{TRootState}.Select{TSelectedState}"/>
    public TSelectedState Select<TSelectedState>(Func<TRootState, TSelectedState> selector) => selector(State);
    
    private void OnStateChanged(TRootState prevState, TRootState newState)
    {
        foreach (var observable in _observables)
        {
            observable.OnStateChanged(prevState, newState);
        }
    }

    public IObservable<TSelectedState> ObservableFor<TSelectedState>(
        Func<TRootState, TSelectedState> selector,
        bool notifyImmediately = false
    ) where TSelectedState : IEquatable<TSelectedState>
    {
        var observable = new StateObservable<TRootState, TSelectedState>(this, selector, notifyImmediately);
        _observables.Add(observable);

        return observable;
    }
}