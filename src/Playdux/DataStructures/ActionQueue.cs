using System;
using System.Collections.Concurrent;
using System.Threading;
using Playdux.Actions;

namespace Playdux.DataStructures;

/// Holds actions and ensures that they get handled in FIFO order.
public class ActionQueue(Action<DispatchedAction> actionHandler)
{
    private readonly ConcurrentQueue<DispatchedAction> _queue = new();

    private int _isBeingConsumed;

    /// <summary>
    /// Adds an action to the action queue. The action will be sent to the provided action handler when it is at the
    /// head of the queue.
    /// </summary>
    /// <remarks>
    /// Dispatching an action into the queue will also begin consuming from the queue, unless it is already being
    /// consumed from another thread, in which case the dispatched action will be consumed on that thread.
    /// </remarks>
    public void Dispatch(DispatchedAction action)
    {
        _queue.Enqueue(action);
        if (Interlocked.CompareExchange(ref _isBeingConsumed, 1, 0) != 0) return;
    
        while (_queue.TryDequeue(out var next)) actionHandler(next);
        Interlocked.Exchange(ref _isBeingConsumed, 0);
    }
}