using System;
using System.Collections.Generic;

namespace Playdux.Store;

/// <summary>
/// A Playdux state container. The core of Playdux.
/// Includes capability to dispatch actions to the store, get the current state, get the current state narrowed by
/// a selector, and get an IObservable to the "selected" state.
/// </summary>
public sealed class Store<TRootState> : IStore<TRootState> where TRootState : class
{
    /// The current state within the store.
    public TRootState State { get; private set; }

    /// Reduces state according to actions dispatched to the store.
    private readonly Func<TRootState, IAction, TRootState> rootReducer;

    /// Holds actions in a defined FIFO order to ensure actions are processed in the order that they are received.
    private readonly ActionQueue actionQueue;

    /// Holds side effectors in a collection that preserves priority while also providing fast addition and removal.
    private readonly SideEffectorCollection<TRootState> sideEffectors = new();

    /// Create a new store with a given initial state and reducer
    public Store(TRootState initialState, Func<TRootState, IAction, TRootState> rootReducer, IEnumerable<ISideEffector<TRootState>>? initialSideEffectors = null)
    {
        State = initialState;
        this.rootReducer = rootReducer;
        actionQueue = new ActionQueue(DispatchInternal);

        if (initialSideEffectors is null) return;

        foreach (var sideEffector in initialSideEffectors) RegisterSideEffector(sideEffector);

        Dispatch(new InitializeAction<TRootState>(initialState));
    }

    /// <inheritdoc cref="IActionDispatcher{TRootState}.Dispatch"/>
    public void Dispatch(IAction action)
    {
        ValidateInitializeAction(action);
        actionQueue.Dispatch(new DispatchedAction(action));
    }

    /// Handles a single dispatched action from the queue, activating pre effects, reducing state, updating state, then activating post effects.
    private void DispatchInternal(DispatchedAction dispatchedAction)
    {
        // Pre Effects
        foreach (var sideEffector in sideEffectors.ByPriority)
        {
            try
            {
                var shouldAllow = sideEffector.PreEffect(dispatchedAction, this);
                if (!shouldAllow) dispatchedAction = dispatchedAction with { IsCanceled = true };
            }
            catch (Exception e) { throw new SideEffectorExecutionException(SideEffectorType.Pre, e); }
        }

        if (dispatchedAction.IsCanceled) return;

        // Reduce
        var action = dispatchedAction.Action;
        var state = State;
        if (action is InitializeAction<TRootState> castAction) state = castAction.InitialState;
        State = rootReducer(state, action);

        // Post Effects
        foreach (var sideEffector in sideEffectors.ByPriority)
        {
            try { sideEffector.PostEffect(dispatchedAction, this); }
            catch (Exception e) { throw new SideEffectorExecutionException(SideEffectorType.Post, e); }
        }
    }

    /// <inheritdoc cref="IActionDispatcher{TRootState}.RegisterSideEffector"/>
    public Guid RegisterSideEffector(ISideEffector<TRootState> sideEffector) => sideEffectors.Register(sideEffector);

    /// <inheritdoc cref="IActionDispatcher{TRootState}.UnregisterSideEffector"/>
    public void UnregisterSideEffector(Guid sideEffectorId) => sideEffectors.Unregister(sideEffectorId);

    /// <inheritdoc cref="IStateContainer{TRootState}.Select{TSelectedState}"/>
    public TSelectedState Select<TSelectedState>(Func<TRootState, TSelectedState> selector) => selector(State);

    public IObservable<TSelectedState> ObservableFor<TSelectedState>(Func<TRootState, TSelectedState> selector, bool notifyImmediately = false)
    {
        throw new NotImplementedException();
    }

    /// Throws an error if an incorrectly typed InitializeAction is dispatched to this store.
    private static void ValidateInitializeAction(IAction action)
    {
        var actionType = action.GetType();
        var isInitializeAction = actionType.IsGenericType && actionType.GetGenericTypeDefinition() == typeof(InitializeAction<>);
        var isInitializeActionCorrectType = isInitializeAction && action is InitializeAction<TRootState>;

        if (isInitializeAction && !isInitializeActionCorrectType) throw new InitializeTypeMismatchException(actionType.GetGenericArguments()[0], typeof(TRootState));
    }
}